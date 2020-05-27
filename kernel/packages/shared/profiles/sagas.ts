import { getFromLocalStorage, saveToLocalStorage } from 'atomicHelpers/localStorage'
import { call, fork, put, race, select, take, takeEvery, takeLatest } from 'redux-saga/effects'
import { NotificationType } from 'shared/types'
import { getServerConfigurations, ALL_WEARABLES, getWearablesSafeURL } from '../../config'
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
  InventorySuccess,
  PROFILE_REQUEST,
  PROFILE_SUCCESS,
  PROFILE_RANDOM,
  SAVE_PROFILE_REQUEST,
  ProfileRequestAction,
  profileSuccess,
  ProfileRandomAction,
  ProfileSuccessAction,
  SaveProfileRequest,
  saveProfileSuccess,
  profileRequest,
  saveProfileFailure,
  addedProfileToCatalog
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
import {
  Catalog,
  Profile,
  WearableId,
  Wearable,
  Collection,
  Entity,
  EntityField,
  ControllerEntity,
  ControllerEntityContent,
  EntityType,
  Pointer,
  ContentFile,
  ENTITY_FILE_NAME,
  DeployData
} from './types'
import { identity, ExplorerIdentity } from '../index'
import { Authenticator, AuthLink } from 'dcl-crypto'
import { sha3 } from 'web3x/utils'
import { CATALYST_REALM_INITIALIZED } from '../dao/actions'
import { isRealmInitialized, getUpdateProfileServer } from '../dao/selectors'
import { getUserProfile } from '../comms/peers'
import { WORLD_EXPLORER } from '../../config/index'
import { backupProfile } from 'shared/profiles/generateRandomUserProfile'
import { getTutorialBaseURL } from '../location'
import { takeLatestById } from './utils/takeLatestById'
import { UnityInterfaceContainer } from 'unity-interface/dcl'
import { RarityEnum } from '../airdrops/interface'

type Timestamp = number
type ContentFileHash = string

const CID = require('cids')
const multihashing = require('multihashing-async')
const toBuffer = require('blob-to-buffer')

declare const globalThis: Window & UnityInterfaceContainer

export const getCurrentUserId = () => identity.address

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
export function* profileSaga(): any {
  if (!(yield select(isRealmInitialized))) {
    yield take(CATALYST_REALM_INITIALIZED)
  }
  yield takeEvery(RENDERER_INITIALIZED, initialLoad)

  yield takeLatest(ADD_CATALOG, handleAddCatalog)

  yield takeLatestByUserId(PROFILE_REQUEST, handleFetchProfile)
  yield takeLatestByUserId(PROFILE_SUCCESS, submitProfileToRenderer)
  yield takeLatestByUserId(PROFILE_RANDOM, handleRandomAsSuccess)

  yield takeLatestByUserId(SAVE_PROFILE_REQUEST, handleSaveAvatar)

  yield takeLatestByUserId(INVENTORY_REQUEST, handleFetchInventory)

  yield takeLatest(NOTIFY_NEW_INVENTORY_ITEM, handleNewInventoryItem)

  yield fork(queryInventoryEveryMinute)
}

function overrideBaseUrl(wearable: Wearable) {
  return {
    ...wearable,
    baseUrl: getWearablesSafeURL() + '/contents/',
    baseUrlBundles: getServerConfigurations().contentAsBundle + '/'
  }
}

function overrideSwankyRarity(wearable: Wearable) {
  if (wearable.rarity as any === 'swanky') {
    return {
      ...wearable,
      rarity: 'rare' as RarityEnum
    }
  }
  return wearable
}

export function* initialLoad() {
  if (WORLD_EXPLORER) {
    try {
      let collections: Collection[]
      if (globalThis.location.search.match(/TEST_WEARABLES/)) {
        collections = [{ id: 'all', wearables: yield call(fetchCatalog, getServerConfigurations().avatar.catalog) }]
      } else {
        collections = yield call(fetchCatalog, getServerConfigurations().avatar.catalog)
      }
      const catalog = collections
        .reduce((flatten, collection) => flatten.concat(collection.wearables), [] as Wearable[])
        .map(overrideBaseUrl)
        // TODO - remove once all swankies are removed from service! - moliva - 22/05/2020
        .map(overrideSwankyRarity)
      const baseAvatars = catalog.filter((_: Wearable) => _.tags && !_.tags.includes('exclusive'))
      const baseExclusive = catalog.filter((_: Wearable) => _.tags && _.tags.includes('exclusive'))
      if (!(yield select(isInitialized))) {
        yield take(RENDERER_INITIALIZED)
      }
      yield put(addCatalog('base-avatars', baseAvatars))
      yield put(addCatalog('base-exclusive', baseExclusive))
    } catch (error) {
      defaultLogger.error('[FATAL]: Could not load catalog!', error)
    }
  } else {
    let baseCatalog = []
    try {
      const response = yield fetch(getTutorialBaseURL() + '/default-profile/basecatalog.json')
      baseCatalog = yield response.json()
    } catch (e) {
      defaultLogger.warn(`Could not load base catalog`)
    }
    yield put(addCatalog('base-avatars', baseCatalog))
    yield put(addCatalog('base-exclusive', []))
  }
}

export function* handleFetchProfile(action: ProfileRequestAction): any {
  const userId = action.payload.userId
  const email = ''

  const currentId = yield select(getCurrentUserId)
  let profile: any
  let hasConnectedWeb3 = false
  if (WORLD_EXPLORER) {
    try {
      const serverUrl = yield select(getProfileDownloadServer)
      const profiles: { avatars: object[] } = yield call(profileServerRequest, serverUrl, userId)

      if (profiles.avatars.length !== 0) {
        profile = profiles.avatars[0]
        hasConnectedWeb3 = true
      }
    } catch (error) {
      defaultLogger.warn(`Error requesting profile for ${userId}, `, error)
    }

    const userInfo = getUserProfile()
    if (!profile && userInfo && userInfo.userId && userId === userInfo.userId && userInfo.profile) {
      defaultLogger.info(`Recover profile from local storage`)
      profile = yield call(
        ensureServerFormat,
        { ...userInfo.profile, avatar: { ...userInfo.profile.avatar, snapshots: userInfo.profile.snapshots } },
        userInfo.version || 0
      )
    }

    if (!profile) {
      defaultLogger.info(`Profile for ${userId} not found, generating random profile`)
      profile = yield call(generateRandomUserProfile, userId)
    }
  } else {
    const baseUrl = yield call(getTutorialBaseURL)
    profile = yield call(backupProfile, baseUrl + '/default-profile/snapshots', userId)
  }

  if (currentId === userId) {
    profile.email = email
  }

  if (!ALL_WEARABLES && WORLD_EXPLORER) {
    yield put(inventoryRequest(userId, userId))
    const inventoryResult = yield race({
      success: take(isActionFor(INVENTORY_SUCCESS, userId)),
      failure: take(isActionFor(INVENTORY_FAILURE, userId))
    })
    if (inventoryResult.failure) {
      defaultLogger.error(`Unable to fetch inventory for ${userId}:`, inventoryResult.failure)
    } else {
      profile.inventory = (inventoryResult.success as InventorySuccess).payload.inventory.map(dropIndexFromExclusives)
    }
  }

  const passport = yield call(processServerProfile, userId, profile)
  yield put(profileSuccess(userId, passport, hasConnectedWeb3))
}

export async function profileServerRequest(serverUrl: string, userId: string) {
  try {
    const request = await fetch(`${serverUrl}/${userId}`)
    if (!request.ok) {
      throw new Error('Profile not found')
    }
    return await request.json()
  } catch (up) {
    throw up
  }
}

export function* handleRandomAsSuccess(action: ProfileRandomAction): any {
  // TODO (eordano, 16/Sep/2019): See if there's another way around people expecting PASSPORT_SUCCESS
  yield put(profileSuccess(action.payload.userId, action.payload.profile))
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

export function sendWearablesCatalog(catalog: Catalog) {
  globalThis.unityInterface.AddWearablesToCatalog(catalog)
}

export function handleNewInventoryItem() {
  globalThis.unityInterface.ShowNotification({
    type: NotificationType.GENERIC,
    message: 'You received an exclusive wearable NFT mask! Check it out in the avatar editor.',
    buttonMessage: 'OK',
    timer: 7
  })
}

export function* ensureRenderer() {
  while (!(yield select(isInitialized))) {
    yield take(RENDERER_INITIALIZED)
  }
}

export function* ensureBaseCatalogs() {
  while (!(yield select(baseCatalogsLoaded))) {
    yield take(CATALOG_LOADED)
  }
}

export function* submitProfileToRenderer(action: ProfileSuccessAction): any {
  const profile = { ...action.payload.profile }
  if ((yield select(getCurrentUserId)) === action.payload.userId) {
    yield call(ensureRenderer)
    yield call(ensureBaseCatalogs)
    // FIXIT - need to have this duplicated here, as the inventory won't be used if not - moliva - 17/12/2019
    if (ALL_WEARABLES) {
      profile.inventory = (yield select(getExclusiveCatalog)).map((_: Wearable) => _.id)
    }
    yield call(sendLoadProfile, profile)
  } else {
    yield call(ensureRenderer)
    yield call(ensureBaseCatalogs)

    const forRenderer = profileToRendererFormat(profile)
    forRenderer.hasConnectedWeb3 = action.payload.hasConnectedWeb3

    globalThis.unityInterface.AddUserProfileToCatalog(forRenderer)

    yield put(addedProfileToCatalog(action.payload.userId, forRenderer))
  }
}

function* sendLoadProfile(profile: Profile) {
  while (!(yield select(baseCatalogsLoaded))) {
    yield take(CATALOG_LOADED)
  }
  const rendererFormat = profileToRendererFormat(profile, identity)
  globalThis.unityInterface.LoadProfile(rendererFormat)
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
  if (!WORLD_EXPLORER) {
    return []
  }
  const result = await fetch(`${getServerConfigurations().wearablesApi}/addresses/${address}/wearables?fields=id`)
  if (!result.ok) {
    throw new Error('Unable to fetch inventory for address ' + address)
  }
  const inventory: { id: string }[] = await result.json()

  return inventory.map(wearable => wearable.id)
}

export function* handleSaveAvatar(saveAvatar: SaveProfileRequest) {
  const userId = saveAvatar.payload.userId ? saveAvatar.payload.userId : yield select(getCurrentUserId)

  try {
    const savedProfile = yield select(getProfile, userId)
    const currentVersion = savedProfile.version || 0
    const url: string = yield select(getUpdateProfileServer)
    const profile = { ...savedProfile, ...saveAvatar.payload.profile }

    // only update profile if wallet is connected
    if (identity.hasConnectedWeb3) {
      const result = yield call(modifyAvatar, {
        url,
        userId,
        currentVersion,
        identity,
        profile
      })

      const { creationTimestamp: version } = result

      yield put(saveProfileSuccess(userId, version, profile))
      yield put(profileRequest(userId))
    }
  } catch (error) {
    yield put(saveProfileFailure(userId, 'unknown reason'))
  }
}

export class ControllerEntityFactory {
  static maskEntity(fullEntity: Entity, fields?: EntityField[]): ControllerEntity {
    const { id, type, timestamp } = fullEntity
    let content: ControllerEntityContent[] | undefined = undefined
    let metadata: any
    let pointers: string[] = []
    if ((!fields || fields.includes(EntityField.CONTENT)) && fullEntity.content) {
      content = Array.from(fullEntity.content.entries()).map(([file, hash]) => ({ file, hash }))
    }
    if (!fields || fields.includes(EntityField.METADATA)) {
      metadata = fullEntity.metadata
    }
    if ((!fields || fields.includes(EntityField.POINTERS)) && fullEntity.pointers) {
      pointers = fullEntity.pointers
    }
    return { id, type, timestamp, pointers, content, metadata }
  }
}

async function buildControllerEntityAndFile(
  type: EntityType,
  pointers: Pointer[],
  timestamp: Timestamp,
  content?: Map<string, ContentFileHash>,
  metadata?: any
): Promise<[ControllerEntity, ContentFile]> {
  const [entity, file]: [Entity, ContentFile] = await buildEntityAndFile(type, pointers, timestamp, content, metadata)
  return [ControllerEntityFactory.maskEntity(entity), file]
}

export async function buildEntityAndFile(
  type: EntityType,
  pointers: Pointer[],
  timestamp: Timestamp,
  content?: Map<string, ContentFileHash>,
  metadata?: any
): Promise<[Entity, ContentFile]> {
  const entity: Entity = new Entity('temp-id', type, pointers, timestamp, content, metadata)
  const file: ContentFile = entityToFile(entity, ENTITY_FILE_NAME)
  const fileHash: ContentFileHash = await calculateBufferHash(file.content)
  const entityWithCorrectId = new Entity(
    fileHash,
    entity.type,
    entity.pointers,
    entity.timestamp,
    entity.content,
    entity.metadata
  )
  return [entityWithCorrectId, file]
}

export function entityToFile(entity: Entity, fileName?: string): ContentFile {
  let copy: any = Object.assign({}, entity)
  copy.content =
    !copy.content || !(copy.content instanceof Map)
      ? copy.content
      : Array.from(copy.content.entries()).map(([key, value]: any) => ({ file: key, hash: value }))
  delete copy.id
  return { name: fileName || 'name', content: Buffer.from(JSON.stringify(copy)) }
}

export async function calculateBufferHash(buffer: Buffer): Promise<string> {
  const hash = await multihashing(buffer, 'sha2-256')
  return new CID(0, 'dag-pb', hash).toBaseEncodedString()
}

export async function modifyAvatar(params: {
  url: string
  currentVersion: number
  userId: string
  identity: ExplorerIdentity
  profile: Profile
}) {
  const { url, currentVersion, profile, identity } = params
  const { avatar } = profile

  const newAvatar = { ...avatar }

  let files: ContentFile[] = []

  const snapshots = avatar.snapshots || (profile as any).snapshots
  if (snapshots) {
    if (snapshots.face.includes('://') && snapshots.body.includes('://')) {
      newAvatar.snapshots = {
        face: snapshots.face.split('/').pop()!,
        body: snapshots.body.split('/').pop()!
      }
    } else {
      // replace base64 snapshots with their respective hashes
      const faceFile: ContentFile = await makeContentFile('./face.png', base64ToBlob(snapshots.face))
      const bodyFile: ContentFile = await makeContentFile('./body.png', base64ToBlob(snapshots.body))

      const faceFileHash: string = await calculateBufferHash(faceFile.content)
      const bodyFileHash: string = await calculateBufferHash(bodyFile.content)

      newAvatar.snapshots = {
        face: faceFileHash,
        body: bodyFileHash
      }
      files = [faceFile, bodyFile]
    }
  }
  const newProfile = ensureServerFormat({ ...profile, avatar: newAvatar }, currentVersion)

  const [data] = await buildDeployData(
    [identity.address],
    {
      avatars: [newProfile]
    },
    files
  )
  return deploy(url, data)
}

export function makeContentFile(path: string, content: string | Blob): Promise<ContentFile> {
  return new Promise((resolve, reject) => {
    if (typeof content === 'string') {
      const buffer = Buffer.from(content)
      resolve({ name: path, content: buffer })
    } else if (content instanceof Blob) {
      toBuffer(content, (err: Error, buffer: Buffer) => {
        if (err) reject(err)
        resolve({ name: path, content: buffer })
      })
    } else {
      reject(new Error('Unable to create ContentFile: content must be a string or a Blob'))
    }
  })
}

export async function buildDeployData(
  pointers: Pointer[],
  metadata: any,
  files: ContentFile[] = [],
  afterEntity?: ControllerEntity
): Promise<[DeployData, ControllerEntity]> {
  const hashes: Map<ContentFileHash, ContentFile> = await calculateHashes(files)
  const content: Map<string, string> = new Map(Array.from(hashes.entries()).map(([hash, file]) => [file.name, hash]))

  const [entity, entityFile] = await buildControllerEntityAndFile(
    EntityType.PROFILE,
    pointers,
    (afterEntity ? afterEntity.timestamp : Date.now()) + 1,
    content,
    metadata
  )

  const body = await hashAndSignMessage(entity.id)
  const deployData: DeployData = {
    entityId: entity.id,
    // Every position in the body ethAddress every position in the authLink is an slavon.
    // 1 = ethAddress, 2 ehpemeral & signature w/ethAdrress, 3 request & signature with ephemeral
    authChain: body,
    files: [entityFile, ...files]
  }

  return [deployData, entity]
}

export async function hashAndSignMessage(message: string): Promise<AuthLink[]> {
  return Authenticator.signPayload(identity, message)
}

export function createEthereumMessageHash(msg: string) {
  let msgWithPrefix: string = `\x19Ethereum Signed Message:\n${msg.length}${msg}`
  return sha3(msgWithPrefix)
}

export async function calculateHashes(files: ContentFile[]): Promise<Map<string, ContentFile>> {
  const entries: Promise<[string, ContentFile]>[] = Array.from(files).map(file =>
    calculateBufferHash(file.content).then((hash: string) => [hash, file])
  )
  return new Map(await Promise.all(entries))
}

export async function deploy(contentServerUrl: string, data: DeployData) {
  const form = new FormData()
  form.append('entityId', data.entityId)
  convertModelToFormData(data.authChain, form, 'authChain')
  for (let file of data.files) {
    form.append(file.name, new Blob([file.content]), file.name)
  }
  const deployResponse = await fetch(`${contentServerUrl}/entities`, {
    method: 'POST',
    body: form
  })
  return deployResponse.json()
}

function convertModelToFormData(model: any, form: FormData = new FormData(), namespace = ''): FormData {
  let formData = form || new FormData()
  for (let propertyName in model) {
    if (!model.hasOwnProperty(propertyName) || !model[propertyName]) continue
    let formKey = namespace ? `${namespace}[${propertyName}]` : propertyName
    if (model[propertyName] instanceof Date) {
      formData.append(formKey, model[propertyName].toISOString())
    } else if (model[propertyName] instanceof Array) {
      model[propertyName].forEach((element: any, index: number) => {
        const tempFormKey = `${formKey}[${index}]`
        convertModelToFormData(element, formData, tempFormKey)
      })
    } else if (typeof model[propertyName] === 'object' && !(model[propertyName] instanceof File)) {
      convertModelToFormData(model[propertyName], formData, formKey)
    } else {
      formData.append(formKey, model[propertyName].toString())
    }
  }
  return formData
}

export function base64ToBlob(base64: string): Blob {
  const sliceSize = 1024
  const byteChars = globalThis.atob(base64)
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
    const oldItemsDict = oldInventory.reduce<Record<WearableId, boolean>>(
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
