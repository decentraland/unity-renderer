import { Store } from 'redux'
import { EntityType } from 'dcl-catalyst-commons'
import { ContentClient, DeploymentBuilder, DeploymentData } from 'dcl-catalyst-client'
import { call, put, race, select, take, takeEvery, takeLatest } from 'redux-saga/effects'

import {
  getServerConfigurations,
  ALL_WEARABLES,
  getWearablesSafeURL,
  PIN_CATALYST,
  PREVIEW,
  ethereumConfigurations,
  RESET_TUTORIAL,
  WSS_ENABLED,
  TEST_WEARABLES_OVERRIDE
} from 'config'

import defaultLogger from 'shared/logger'
import { isInitialized } from 'shared/renderer/selectors'
import { RENDERER_INITIALIZED } from 'shared/renderer/types'
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
  addedProfileToCatalog,
  saveProfileRequest
} from './actions'
import { generateRandomUserProfile } from './generateRandomUserProfile'
import { baseCatalogsLoaded, getProfile, getProfileDownloadServer, getExclusiveCatalog } from './selectors'
import { processServerProfile } from './transformations/processServerProfile'
import { profileToRendererFormat } from './transformations/profileToRendererFormat'
import { ensureServerFormat } from './transformations/profileToServerFormat'
import { Catalog, Profile, Wearable, Collection, ContentFile, Avatar } from './types'
import { ExplorerIdentity } from 'shared/session/types'
import { Authenticator } from 'dcl-crypto'
import { getUpdateProfileServer, getResizeService, isResizeServiceUrl } from '../dao/selectors'
import { getUserProfile } from '../comms/peers'
import { WORLD_EXPLORER } from '../../config/index'
import { backupProfile } from 'shared/profiles/generateRandomUserProfile'
import { getResourcesURL } from '../location'
import { takeLatestById } from './utils/takeLatestById'
import { UnityInterfaceContainer } from 'unity-interface/dcl'
import { RarityEnum } from '../airdrops/interface'
import { StoreContainer } from '../store/rootTypes'
import { retrieve, store } from 'shared/cache'
import { getCurrentUserId, getCurrentIdentity, getCurrentNetwork } from 'shared/session/selectors'
import { USER_AUTHENTIFIED } from 'shared/session/actions'
import { ProfileAsPromise } from './ProfileAsPromise'
import { fetchOwnedENS } from 'shared/web3'
import { RootState } from 'shared/store/rootTypes'
import { persistCurrentUser } from 'shared/comms'
import { ensureRealmInitialized } from 'shared/dao/sagas'

const CID = require('cids')
const multihashing = require('multihashing-async')
const toBuffer = require('blob-to-buffer')

declare const globalThis: Window & UnityInterfaceContainer & StoreContainer

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
  yield takeEvery(USER_AUTHENTIFIED, initialProfileLoad)

  yield takeEvery(RENDERER_INITIALIZED, initialLoad)

  yield takeLatest(ADD_CATALOG, handleAddCatalog)

  yield takeLatestByUserId(PROFILE_REQUEST, handleFetchProfile)
  yield takeLatestByUserId(PROFILE_SUCCESS, submitProfileToRenderer)
  yield takeLatestByUserId(PROFILE_RANDOM, handleRandomAsSuccess)

  yield takeLatestByUserId(SAVE_PROFILE_REQUEST, handleSaveAvatar)

  yield takeLatestByUserId(INVENTORY_REQUEST, handleFetchInventory)
}

function* initialProfileLoad() {
  yield call(ensureRealmInitialized)

  // initialize profile
  const userId = yield select(getCurrentUserId)
  let profile = yield ProfileAsPromise(userId)

  if (!PREVIEW) {
    let profileDirty: boolean = false

    if (!profile.hasClaimedName) {
      const net: keyof typeof ethereumConfigurations = yield select(getCurrentNetwork)
      const names = yield fetchOwnedENS(ethereumConfigurations[net].names, userId)

      // patch profile to readd missing name
      profile = { ...profile, name: names[0], hasClaimedName: true, version: (profile.version || 0) + 1 }

      if (names && names.length > 0) {
        defaultLogger.info(`Found missing claimed name '${names[0]}' for profile ${userId}, consolidating profile... `)
        profileDirty = true
      }
    }

    const isFace128Resized = yield select(isResizeServiceUrl, profile.avatar.snapshots?.face128)
    const isFace256Resized = yield select(isResizeServiceUrl, profile.avatar.snapshots?.face256)

    if (isFace128Resized || isFace256Resized) {
      // setting dirty profile, as at least one of the face images are taken from a local blob
      profileDirty = true
    }

    const localTutorialStep = getUserProfile().profile ? getUserProfile().profile.tutorialStep : 0

    if (localTutorialStep !== profile.tutorialStep) {
      let finalTutorialStep = Math.max(localTutorialStep, profile.tutorialStep)
      profile = { ...profile, tutorialStep: finalTutorialStep }
      profileDirty = true
    }

    if (RESET_TUTORIAL) {
      profile = { ...profile, tutorialStep: 0 }
      profileDirty = true
    }

    if (profileDirty) {
      scheduleProfileUpdate(profile)
    }
  }

  const identity = yield select(getCurrentIdentity)

  persistCurrentUser({
    version: profile.version,
    profile: profileToRendererFormat(profile, identity)
  })
}

/**
 * Schedule profile update post login (i.e. comms authenticated & established).
 *
 * @param profile Updated profile
 */
function scheduleProfileUpdate(profile: Profile) {
  new Promise(() => {
    const store: Store<RootState> = globalThis.globalStore

    const unsubscribe = store.subscribe(() => {
      const initialized = store.getState().comms.initialized
      if (initialized) {
        unsubscribe()
        store.dispatch(saveProfileRequest(profile))
      }
    })
  }).catch((e) => defaultLogger.error(`error while updating profile`, e))
}

function overrideBaseUrl(wearable: Wearable) {
  if (!TEST_WEARABLES_OVERRIDE) {
    return {
      ...wearable,
      baseUrl: getWearablesSafeURL() + '/contents/',
      baseUrlBundles: PIN_CATALYST ? '' : getServerConfigurations().contentAsBundle + '/'
    }
  } else {
    return wearable ?? {}
  }
}

function overrideSwankyRarity(wearable: Wearable) {
  if ((wearable.rarity as any) === 'swanky') {
    return {
      ...wearable,
      rarity: 'rare' as RarityEnum
    }
  }
  return wearable
}

export function* initialLoad() {
  yield call(ensureRealmInitialized)

  if (WORLD_EXPLORER) {
    try {
      const catalogUrl = getServerConfigurations().avatar.catalog

      let collections: Collection[] | undefined
      if (globalThis.location.search.match(/TEST_WEARABLES/)) {
        collections = [{ id: 'all', wearables: (yield call(fetchCatalog, catalogUrl))[0] }]
      } else {
        const cached = yield retrieve('catalog')

        if (cached) {
          const version = yield headCatalog(catalogUrl)
          if (cached.version === version) {
            collections = cached.data
          }
        }

        if (!collections) {
          const response = yield call(fetchCatalog, catalogUrl)
          collections = response[0]

          const version = response[1]
          if (version) {
            yield store('catalog', { version, data: response[0] })
          }
        }
      }
      const catalog = collections!
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
      const catalogPath = '/default-profile/basecatalog.json'
      const response = yield fetch(getResourcesURL() + catalogPath)
      baseCatalog = yield response.json()

      if (WSS_ENABLED) {
        for (let item of baseCatalog) {
          item.baseUrl = `http://localhost:8000${item.baseUrl}`
        }
      }
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
    const baseUrl = yield call(getResourcesURL)
    profile = yield call(backupProfile, baseUrl + '/default-profile/snapshots', userId)
  }

  if (currentId === userId) {
    profile.email = email
  }

  yield populateFaceIfNecessary(profile, '256')
  yield populateFaceIfNecessary(profile, '128')

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

function lastSegment(url: string) {
  const segments = url.split('/')
  const segment = segments[segments.length - 1]
  return segment
}

function* populateFaceIfNecessary(profile: any, resolution: string) {
  const selector = `face${resolution}`
  if (
    profile.avatar?.snapshots &&
    (!profile.avatar?.snapshots[selector] || lastSegment(profile.avatar.snapshots[selector]) === resolution) && // XXX - check if content === resolution to fix current issue with corrupted profiles https://github.com/decentraland/explorer/issues/1061 - moliva - 25/06/2020
    profile.avatar?.snapshots?.face
  ) {
    try {
      const resizeServiceUrl: string = yield select(getResizeService)
      const faceUrlSegments = profile.avatar.snapshots.face.split('/')
      const path = `${faceUrlSegments[faceUrlSegments.length - 1]}/${resolution}`
      let faceUrl = `${resizeServiceUrl}/${path}`

      // head to resize url in the current catalyst before populating
      let response = yield call(fetch, faceUrl, { method: 'HEAD' })
      if (!response.ok) {
        // if resize service is not available for this image, try with fallback server
        const fallbackServiceUrl = getServerConfigurations().fallbackResizeServiceUrl
        if (fallbackServiceUrl !== resizeServiceUrl) {
          faceUrl = `${fallbackServiceUrl}/${path}`

          response = yield call(fetch, faceUrl, { method: 'HEAD' })
        }
      }

      if (response.ok) {
        // only populate image field if resize service responsed correctly
        profile.avatar = { ...profile.avatar, snapshots: { ...profile.avatar?.snapshots, [selector]: faceUrl } }
      }
    } catch (e) {
      defaultLogger.error(`error while resizing image for user ${profile.userId} for resolution ${resolution}`, e)
    }
  }
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

async function headCatalog(url: string) {
  const request = await fetch(url, { method: 'HEAD' })
  if (!request.ok) {
    throw new Error('Catalog not found')
  }
  return request.headers.get('etag')
}

export async function fetchCatalog(url: string) {
  const request = await fetch(url)
  if (!request.ok) {
    throw new Error('Catalog not found')
  }
  const etag = request.headers.get('etag')
  return [await request.json(), etag]
}

export function sendWearablesCatalog(catalog: Catalog) {
  globalThis.unityInterface.AddWearablesToCatalog(catalog)
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
  if (profile.avatar) {
    const { snapshots } = profile.avatar
    // set face variants if missing before sending profile to renderer
    profile.avatar.snapshots = {
      ...snapshots,
      face128: snapshots.face128 || snapshots.face,
      face256: snapshots.face256 || snapshots.face
    }
  }
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
  const identity = yield select(getCurrentIdentity)
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
  return exclusive.split('/').slice(0, 4).join('/')
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

  return inventory.map((wearable) => wearable.id)
}

export function* handleSaveAvatar(saveAvatar: SaveProfileRequest) {
  const userId = saveAvatar.payload.userId ? saveAvatar.payload.userId : yield select(getCurrentUserId)

  try {
    const savedProfile = yield select(getProfile, userId)
    const currentVersion = savedProfile.version || 0
    const url: string = yield select(getUpdateProfileServer)
    const profile = { ...savedProfile, ...saveAvatar.payload.profile }

    const identity = yield select(getCurrentIdentity)

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

export async function calculateBufferHash(buffer: Buffer): Promise<string> {
  const hash = await multihashing(buffer, 'sha2-256')
  return new CID(0, 'dag-pb', hash).toBaseEncodedString()
}

async function buildSnapshotContent(selector: string, value: string): Promise<[string, string, ContentFile?]> {
  let hash: string
  let contentFile: ContentFile | undefined

  const name = `./${selector}.png`

  if (isResizeServiceUrl(globalThis.globalStore.getState(), value)) {
    // value is coming in a resize service url => generate image & upload content
    const blob = await fetch(value).then((r) => r.blob())

    contentFile = await makeContentFile(name, blob)
    hash = await calculateBufferHash(contentFile.content)
  } else if (value.includes('://')) {
    // value is already a URL => use existing hash
    hash = value.split('/').pop()!
  } else {
    // value is coming in base 64 => convert to blob & upload content
    const blob = base64ToBlob(value)

    contentFile = await makeContentFile(name, blob)
    hash = await calculateBufferHash(contentFile.content)
  }

  return [name, hash, contentFile]
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

  let files = new Map<string, Buffer>()

  const snapshots = avatar.snapshots || (profile as any).snapshots
  const content = new Map()
  if (snapshots) {
    const newSnapshots: Record<string, string> = {}
    for (const [selector, value] of Object.entries(snapshots)) {
      const [name, hash, contentFile] = await buildSnapshotContent(selector, value as any)

      newSnapshots[selector] = hash
      content.set(name, hash)
      contentFile && files.set(contentFile.name, contentFile.content)
    }
    newAvatar.snapshots = newSnapshots as Avatar['snapshots']
  }
  const newProfile = ensureServerFormat({ ...profile, avatar: newAvatar }, currentVersion)

  return deploy(url, identity, { avatars: [newProfile] }, files)
}

async function deploy(url: string, identity: ExplorerIdentity, metadata: any, contentFiles: Map<string, Buffer>) {
  // Build entity and group all files
  const preparationData = await DeploymentBuilder.buildEntity(
    EntityType.PROFILE,
    [identity.address],
    contentFiles,
    metadata
  )
  // sign the entity id fetchMetaContentServer
  const authChain = Authenticator.signPayload(identity, preparationData.entityId)
  // Build the client
  const catalyst = new ContentClient(url, 'explorer-kernel-profile')
  // Build the deploy data
  const deployData: DeploymentData = { ...preparationData, authChain }
  // Deploy the actual entity
  return catalyst.deployEntity(deployData)
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

export function delay(time: number) {
  return new Promise((resolve) => setTimeout(resolve, time))
}
