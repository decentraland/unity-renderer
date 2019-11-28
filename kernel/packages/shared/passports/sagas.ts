import { getFromLocalStorage, saveToLocalStorage } from 'atomicHelpers/localStorage'
import { call, fork, put, race, select, take, takeEvery, takeLatest, cancel, ForkEffect } from 'redux-saga/effects'
import { NotificationType } from 'shared/types'
import { getServerConfigurations, ALL_WEARABLES } from '../../config'
import { getAccessToken, getCurrentUserId, getEmail } from '../auth/selectors'
import defaultLogger from '../logger'
import { isInitialized } from '../renderer/selectors'
import { RENDERER_INITIALIZED } from '../renderer/types'
import {
  addCatalog,
  AddCatalogAction,
  ADD_CATALOG,
  catalogLoaded,
  CATALOG_LOADED,
  inventoryFailure,
  InventoryRequest,
  inventoryRequest,
  inventorySuccess,
  INVENTORY_FAILURE,
  INVENTORY_REQUEST,
  INVENTORY_SUCCESS,
  notifyNewInventoryItem,
  NOTIFY_NEW_INVENTORY_ITEM,
  passportRandom,
  PassportRandomAction,
  passportRequest,
  PassportRequestAction,
  passportSuccess,
  PassportSuccessAction,
  PASSPORT_RANDOM,
  PASSPORT_REQUEST,
  PASSPORT_SUCCESS,
  saveAvatarFailure,
  SaveAvatarRequest,
  saveAvatarSuccess,
  SAVE_AVATAR_REQUEST,
  setProfileServer,
  InventorySuccess
} from './actions'
import { generateRandomUserProfile } from './generateRandomUserProfile'
import {
  baseCatalogsLoaded,
  getEthereumAddress,
  getInventory,
  getProfile,
  getProfileDownloadServer,
  getExclusiveCatalog
} from './selectors'
import { processServerProfile } from './transformations/processServerProfile'
import { profileToRendererFormat } from './transformations/profileToRendererFormat'
import { ensureServerFormat } from './transformations/profileToServerFormat'
import { Avatar, Catalog, Profile, WearableId, Wearable } from './types'
import { Action } from 'redux'

const isActionFor = (type: string, userId: string) => (action: any) =>
  action.type === type && action.payload.userId === userId

const concatenatedActionTypeUserId = (action: { type: string; payload: { userId: string } }) =>
  action.type + action.payload.userId

const takeLatestByUserId = (patternOrChannel: any, saga: any, ...args: any) =>
  takeLatestById(patternOrChannel, concatenatedActionTypeUserId, saga, ...args)

/**
 * This saga handles both passports and assets required for the renderer to show the
 * users' inventory and avatar editor.
 *
 * When the renderer is initialized, it will fetch the asset catalog and submit it to the renderer.
 *
 * Whenever a passport is requested, it will fetch it and store it locally (see also: `selectors.ts`)
 *
 * If a user avatar was not found, it will create a random passport (see: `handleRandomAsSuccess`)
 *
 * Lastly, we handle save requests by submitting both to the avatar legacy server as well as to the profile server.
 *
 * It's *very* important for the renderer to never receive a passport with items that have not been loaded into the catalog.
 */
export function* passportSaga(): any {
  yield put(setProfileServer(getServerConfigurations().profile))
  yield takeEvery(RENDERER_INITIALIZED, initialLoad)

  yield takeLatest(ADD_CATALOG, handleAddCatalog)

  yield takeLatestByUserId(PASSPORT_REQUEST, handleFetchProfile)
  yield takeLatestByUserId(PASSPORT_SUCCESS, submitPassportToRenderer)
  yield takeLatestByUserId(PASSPORT_RANDOM, handleRandomAsSuccess)

  yield takeLatestByUserId(SAVE_AVATAR_REQUEST, handleSaveAvatar)

  yield takeLatestByUserId(INVENTORY_REQUEST, handleFetchInventory)

  yield takeLatest(NOTIFY_NEW_INVENTORY_ITEM, handleNewInventoryItem)

  yield fork(queryInventoryEveryMinute)
}

function takeLatestById<T extends Action>(
  patternOrChannel: any,
  keyFunction: (action: T) => string,
  saga: any,
  ...args: any
): ForkEffect<never> {
  return fork(function*() {
    let lastTasks = new Map<any, any>()
    while (true) {
      const action = yield take(patternOrChannel)
      const key = keyFunction(action)
      const task = lastTasks.get(key)
      if (task) {
        lastTasks.delete(key)
        yield cancel(task) // cancel is no-op if the task has already terminated
      }
      lastTasks.set(key, yield fork(saga, ...args.concat(action)))
    }
  })
}

export function* initialLoad() {
  try {
    const catalog = yield call(fetchCatalog, getServerConfigurations().avatar.catalog)
    const baseAvatars = catalog.filter((_: Wearable) => !_.tags.includes('exclusive'))
    const baseExclusive = catalog.filter((_: Wearable) => _.tags.includes('exclusive'))
    if (!(yield select(isInitialized))) {
      yield take(RENDERER_INITIALIZED)
    }
    yield put(addCatalog('base-avatars', baseAvatars))
    yield put(addCatalog('base-exclusive', baseExclusive))
  } catch (error) {
    defaultLogger.error('[FATAL]: Could not load catalog!', error)
  }
}

export function* handleFetchProfile(action: PassportRequestAction): any {
  const userId = action.payload.userId
  try {
    const serverUrl = yield select(getProfileDownloadServer)
    const accessToken = yield select(getAccessToken)
    const profile = yield call(profileServerRequest, serverUrl, userId, accessToken)
    const currentId = yield select(getCurrentUserId)
    if (currentId === userId) {
      profile.email = yield select(getEmail)
    }
    if (profile.ethAddress) {
      yield put(inventoryRequest(userId, profile.ethAddress))
      const inventoryResult = yield race({
        success: take(isActionFor(INVENTORY_SUCCESS, userId)),
        failure: take(isActionFor(INVENTORY_FAILURE, userId))
      })
      if (inventoryResult.failure) {
        defaultLogger.error(`Unable to fetch inventory for ${userId}:`, inventoryResult.failure)
      } else {
        profile.inventory = (inventoryResult.success as InventorySuccess).payload.inventory.map(dropIndexFromExclusives)
      }
    } else {
      profile.inventory = []
    }
    const passport = yield call(processServerProfile, userId, profile)
    yield put(passportSuccess(userId, passport))
  } catch (error) {
    const randomizedUserProfile = yield call(generateRandomUserProfile, userId)
    const currentId = yield select(getCurrentUserId)
    if (currentId === userId) {
      randomizedUserProfile.email = yield select(getEmail)
    }
    yield put(inventorySuccess(userId, randomizedUserProfile.inventory))
    yield put(passportRandom(userId, randomizedUserProfile))
  }
}

export async function profileServerRequest(serverUrl: string, userId: string, accessToken: string) {
  try {
    const request = await fetch(`${serverUrl}/profile/${userId}`, {
      headers: {
        Authorization: 'Bearer ' + accessToken
      }
    })
    if (!request.ok) {
      throw new Error('Profile not found')
    }
    return await request.json()
  } catch (up) {
    throw up
  }
}

export function* handleRandomAsSuccess(action: PassportRandomAction): any {
  // TODO (eordano, 16/Sep/2019): See if there's another way around people expecting PASSPORT_SUCCESS
  yield put(passportSuccess(action.payload.userId, action.payload.profile))
}

export function* handleAddCatalog(action: AddCatalogAction): any {
  // TODO (eordano, 16/Sep/2019): Validate correct schema
  if (!action.payload.catalog) {
    return
  }
  if (!(yield select(isInitialized))) {
    yield take(RENDERER_INITIALIZED)
  }
  yield call(sendWearablesCatalog, action.payload.catalog)
  yield put(catalogLoaded(action.payload.name))
}

export async function fetchCatalog(url: string) {
  const request = await fetch(url)
  if (!request.ok) {
    throw new Error('Catalog not found')
  }
  return request.json()
}

declare var window: any

export function sendWearablesCatalog(catalog: Catalog) {
  window['unityInterface'].AddWearablesToCatalog(catalog)
}

export function handleNewInventoryItem() {
  window['unityInterface'].ShowNotification({
    type: NotificationType.GENERIC,
    message: 'You received an exclusive wearable NFT mask! Check it out in the avatar editor.',
    buttonMessage: 'OK',
    timer: 7
  })
}

export function* submitPassportToRenderer(action: PassportSuccessAction): any {
  if ((yield select(getCurrentUserId)) === action.payload.userId) {
    if (!(yield select(isInitialized))) {
      yield take(RENDERER_INITIALIZED)
    }
    while (!(yield select(baseCatalogsLoaded))) {
      yield take(CATALOG_LOADED)
    }
    const profile = { ...action.payload.profile }
    if (ALL_WEARABLES) {
      profile.inventory = (yield select(getExclusiveCatalog)).map((_: Wearable) => _.id)
    }
    yield call(sendLoadProfile, profile)
  }
}

export function* sendLoadProfile(profile: Profile) {
  while (!(yield select(baseCatalogsLoaded))) {
    yield take(CATALOG_LOADED)
  }
  window['unityInterface'].LoadProfile(profileToRendererFormat(profile))
}

export function fetchCurrentProfile(accessToken: string, uuid: string) {
  const authHeader = {
    headers: {
      Authorization: 'Bearer ' + accessToken
    }
  }
  const request = `${getServerConfigurations().profile}/profile${uuid ? '/' + uuid : ''}`
  return fetch(request, authHeader)
}

export function* handleFetchInventory(action: InventoryRequest) {
  const { userId, ethAddress } = action.payload
  try {
    const inventoryItems = yield call(fetchInventoryItemsByAddress, ethAddress)
    yield put(inventorySuccess(userId, inventoryItems))
  } catch (error) {
    yield put(inventoryFailure(userId, error))
  }
}

function dropIndexFromExclusives(exclusive: string) {
  return exclusive
    .split('/')
    .slice(0, 4)
    .join('/')
}

export async function fetchInventoryItemsByAddress(address: string) {
  const result = await fetch(getServerConfigurations().wearablesApi + '/address/' + address)
  if (!result.ok) {
    throw new Error('Unable to fetch inventory for address ' + address)
  }
  return (await result.json()).inventory
}

export function* handleSaveAvatar(saveAvatar: SaveAvatarRequest) {
  const userId = saveAvatar.payload.userId ? saveAvatar.payload.userId : yield select(getCurrentUserId)
  try {
    const currentVersion = (yield select(getProfile, userId)).version || 0
    const accessToken = yield select(getAccessToken)
    const url = getServerConfigurations().profile + '/profile/' + userId + '/avatar'
    const profile = saveAvatar.payload.profile
    const result = yield call(modifyAvatar, {
      url,
      method: 'PUT',
      userId,
      currentVersion,
      accessToken,
      profile
    })
    const { version } = result
    yield put(saveAvatarSuccess(userId, version, profile))
    yield put(passportRequest(userId))
  } catch (error) {
    yield put(saveAvatarFailure(userId, 'unknown reason'))
  }
}

/**
 * @TODO (eordano, 16/Sep/2019): Upgrade the avatar schema on Profile Server
 */
export async function modifyAvatar(params: {
  url: string
  method: string
  currentVersion: number
  userId: string
  accessToken: string
  profile: { avatar: Avatar; face: string; body: string }
}) {
  const { url, method, currentVersion, profile, accessToken } = params
  const { face, avatar, body } = profile
  const snapshots = await saveSnapshots(
    getServerConfigurations().profile + '/profile/' + params.userId,
    accessToken,
    face,
    body
  )
  const avatarData: any = avatar
  avatarData.snapshots = snapshots
  const payload = JSON.stringify(ensureServerFormat(avatarData, currentVersion))
  const options = {
    method,
    body: payload,
    headers: {
      Authorization: 'Bearer ' + accessToken
    }
  }

  const response = await fetch(url, options)
  return response.json()
}

async function saveSnapshots(userURL: string, accessToken: string, face: string, body: string) {
  const data = new FormData()
  data.append('face', base64ToBlob(face), 'face.png')
  data.append('body', base64ToBlob(body), 'body.png')
  return (await fetch(`${userURL}/snapshot`, {
    method: 'POST',
    body: data,
    headers: {
      Authorization: 'Bearer ' + accessToken
    }
  })).json()
}

export function base64ToBlob(base64: string): Blob {
  const sliceSize = 1024
  const byteChars = window.atob(base64)
  const byteArrays = []
  let len = byteChars.length

  for (let offset = 0; offset < len; offset += sliceSize) {
    const slice = byteChars.slice(offset, offset + sliceSize)

    const byteNumbers = new Array(slice.length)
    for (let i = 0; i < slice.length; i++) {
      byteNumbers[i] = slice.charCodeAt(i)
    }

    const byteArray = new Uint8Array(byteNumbers)

    byteArrays.push(byteArray)
    len = byteChars.length
  }

  return new Blob(byteArrays, { type: 'image/jpeg' })
}

const MILLIS_PER_SECOND = 1000
const ONE_MINUTE = 60 * MILLIS_PER_SECOND

export function delay(time: number) {
  return new Promise(resolve => setTimeout(resolve, time))
}

export function* queryInventoryEveryMinute() {
  while (true) {
    yield delay(ONE_MINUTE)
    yield call(checkInventoryForUpdates)
  }
}

export function* checkInventoryForUpdates() {
  const userId = yield select(getCurrentUserId)
  if (!userId) {
    return
  }
  const ethAddress = yield select(getEthereumAddress, userId)
  const inventory = yield select(getInventory, userId)
  if (!inventory || (Array.isArray(inventory) && inventory.length === 0)) {
    return
  }
  yield put(inventoryRequest(userId, ethAddress))
  const fetchNewInventory = yield race({
    success: take(INVENTORY_SUCCESS),
    fail: take(INVENTORY_FAILURE)
  })
  if (fetchNewInventory.success) {
    const newInventory: string[] = (fetchNewInventory.success as InventorySuccess).payload.inventory
    yield call(compareInventoriesAndTriggerNotification, userId, inventory, newInventory)
  }
}

export function* compareInventoriesAndTriggerNotification(
  userId: string,
  oldInventory: string[],
  newInventory: string[],
  fetchFromDb = getFromLocalStorage,
  saveToDb = saveToLocalStorage
) {
  if (areInventoriesDifferent(oldInventory, newInventory)) {
    const oldItemsDict = oldInventory.reduce(
      (cumm: Record<WearableId, boolean>, id: string) => ({ ...cumm, [id]: true }),
      {}
    )
    let shouldSendNotification = false
    for (let item of newInventory) {
      if (!oldItemsDict[item]) {
        const storeKey = '__notified_' + item
        if (!fetchFromDb(storeKey)) {
          saveToDb(storeKey, 'notified')
          shouldSendNotification = true
        }
      }
    }
    if (shouldSendNotification) {
      yield put(notifyNewInventoryItem())
    }
    yield call(sendLoadProfile, yield select(getProfile, userId))
  }
}

function areInventoriesDifferent(inventory1: WearableId[], inventory2: WearableId[]) {
  const sort1 = inventory1.sort()
  const sort2 = inventory2.sort()
  return (
    inventory1.length !== inventory2.length ||
    sort1.reduce((result: boolean, next, index) => result && next !== sort2[index], true)
  )
}
