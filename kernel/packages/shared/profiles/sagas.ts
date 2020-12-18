import { Store } from 'redux'
import { EntityType } from 'dcl-catalyst-commons'
import { ContentClient, DeploymentBuilder, DeploymentData } from 'dcl-catalyst-client'
import { call, throttle, put, race, select, take, takeEvery } from 'redux-saga/effects'

import { getServerConfigurations, ALL_WEARABLES, PREVIEW, ethereumConfigurations, RESET_TUTORIAL } from 'config'

import defaultLogger from 'shared/logger'
import {
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
  saveProfileRequest,
  LOCAL_PROFILE_RECEIVED,
  LocalProfileReceived,
  deployProfile,
  DEPLOY_PROFILE_REQUEST,
  deployProfileSuccess,
  deployProfileFailure,
  profileSavedNotDeployed,
  DeployProfile
} from './actions'
import { generateRandomUserProfile } from './generateRandomUserProfile'
import { getProfile, getProfileDownloadServer, hasConnectedWeb3 } from './selectors'
import { processServerProfile } from './transformations/processServerProfile'
import { profileToRendererFormat } from './transformations/profileToRendererFormat'
import { buildServerMetadata, ensureServerFormat } from './transformations/profileToServerFormat'
import { Profile, ContentFile, Avatar, ProfileType } from './types'
import { ExplorerIdentity } from 'shared/session/types'
import { Authenticator } from 'dcl-crypto'
import { getUpdateProfileServer, getResizeService, isResizeServiceUrl } from '../dao/selectors'
import { WORLD_EXPLORER } from '../../config/index'
import { backupProfile } from 'shared/profiles/generateRandomUserProfile'
import { getResourcesURL } from '../location'
import { takeLatestById } from './utils/takeLatestById'
import { UnityInterfaceContainer } from 'unity-interface/dcl'
import { StoreContainer } from '../store/rootTypes'
import { getCurrentUserId, getCurrentIdentity, getCurrentNetwork } from 'shared/session/selectors'
import { USER_AUTHENTIFIED } from 'shared/session/actions'
import { ProfileAsPromise } from './ProfileAsPromise'
import { fetchOwnedENS } from 'shared/web3'
import { RootState } from 'shared/store/rootTypes'
import { requestLocalProfileToPeers, updateCommsUser } from 'shared/comms'
import { ensureRealmInitialized } from 'shared/dao/sagas'
import { ensureRenderer } from 'shared/renderer/sagas'
import { ensureBaseCatalogs } from 'shared/catalogs/sagas'
import { getExclusiveCatalog } from 'shared/catalogs/selectors'
import { base64ToBlob } from 'atomicHelpers/base64ToBlob'
import { Wearable } from 'shared/catalogs/types'
import { LocalProfilesRepository } from './LocalProfilesRepository'
import { getProfileType } from './getProfileType'
import { ReportFatalError } from 'shared/loading/ReportFatalError'
import { UNEXPECTED_ERROR } from 'shared/loading/types'
import { fetchParcelsWithAccess } from './fetchLand'
import { ParcelsWithAccess } from 'decentraland-ecs/src'

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

// This repository is for local profiles owned by this browser (without wallet)
const localProfilesRepo = new LocalProfilesRepository()

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

  yield takeLatestByUserId(PROFILE_REQUEST, handleFetchProfile)
  yield takeLatestByUserId(PROFILE_SUCCESS, submitProfileToRenderer)
  yield takeLatestByUserId(PROFILE_RANDOM, handleRandomAsSuccess)

  yield takeLatestByUserId(SAVE_PROFILE_REQUEST, handleSaveAvatar)

  yield takeLatestByUserId(INVENTORY_REQUEST, handleFetchInventory)

  yield takeLatestByUserId(LOCAL_PROFILE_RECEIVED, handleLocalProfile)

  yield throttle(3000, DEPLOY_PROFILE_REQUEST, handleDeployProfile)
}

function* initialProfileLoad() {
  yield call(ensureRealmInitialized)

  // initialize profile
  const identity: ExplorerIdentity = yield select(getCurrentIdentity)
  const userId = identity.address

  let profile = undefined

  try {
    profile = yield ProfileAsPromise(userId, undefined, getProfileType(identity))
  } catch (e) {
    ReportFatalError(UNEXPECTED_ERROR)
    throw e
  }

  if (!PREVIEW) {
    let profileDirty: boolean = false

    if (!profile.hasClaimedName) {
      const net: keyof typeof ethereumConfigurations = yield select(getCurrentNetwork)
      const names = yield fetchOwnedENS(ethereumConfigurations[net].names, userId)

      // patch profile to readd missing name
      profile = { ...profile, name: names[0], hasClaimedName: true }

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

    if (RESET_TUTORIAL) {
      profile = { ...profile, tutorialStep: 0 }
      profileDirty = true
    }

    if (profileDirty) {
      scheduleProfileUpdate(profile)
    }
  }

  updateCommsUser({ version: profile.version })
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

export function* getProfileByUserId(userId: string): any {
  try {
    const server = yield select(getProfileDownloadServer)
    const profiles: { avatars: object[] } = yield profileServerRequest(server, userId)

    if (profiles.avatars.length !== 0) {
      return profiles.avatars[0]
    }
  } catch (error) {
    if (error.message !== 'Profile not found') {
      defaultLogger.log(`Error requesting profile for auth check ${userId}, `, error)
    }
  }
  return null
}

export function* handleFetchProfile(action: ProfileRequestAction): any {
  const userId = action.payload.userId
  const email = ''

  const currentId = yield select(getCurrentUserId)
  let profile: any
  let hasConnectedWeb3 = false
  if (WORLD_EXPLORER) {
    try {
      if (action.payload.profileType === ProfileType.LOCAL && currentId !== userId) {
        const peerProfile: Profile = yield requestLocalProfileToPeers(action.payload.userId)
        if (peerProfile) {
          profile = ensureServerFormat(peerProfile)
          profile.hasClaimedName = false // for now, comms profiles can't have claimed names
        }
      } else {
        const serverUrl = yield select(getProfileDownloadServer)
        const profiles: { avatars: object[] } = yield call(profileServerRequest, serverUrl, userId)

        if (profiles.avatars.length !== 0) {
          profile = profiles.avatars[0]
          profile.hasClaimedName = !!profile.name && profile.hasClaimedName // old lambdas profiles don't have claimed names if they don't have the "name" property
          hasConnectedWeb3 = true
        }
      }
    } catch (error) {
      defaultLogger.warn(`Error requesting profile for ${userId}, `, error)
    }

    if (currentId === userId) {
      const localProfile = fetchProfileLocally(userId)
      if (!profile || (localProfile && profile.version < localProfile.version)) {
        profile = localProfile
      }

      const identity: ExplorerIdentity = yield select(getCurrentIdentity)
      profile.ethAddress = identity.rawAddress
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

function dropIndexFromExclusives(exclusive: string) {
  return exclusive.split('/').slice(0, 4).join('/')
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

export function* createSignUpProfile(profile: Profile, identity: ExplorerIdentity) {
  const url: string = yield select(getUpdateProfileServer)
  const userId = profile.userId
  // to prevent save a email on profile
  profile.email = ''
  return yield modifyAvatar({
    url,
    userId,
    identity,
    profile
  })
}

function* handleRandomAsSuccess(action: ProfileRandomAction): any {
  // TODO (eordano, 16/Sep/2019): See if there's another way around people expecting PASSPORT_SUCCESS
  yield put(profileSuccess(action.payload.userId, action.payload.profile))
}

function* handleLocalProfile(action: LocalProfileReceived) {
  const { userId, profile } = action.payload

  const existingProfile = yield select(getProfile, userId)
  const connectedWeb3 = yield select(hasConnectedWeb3, userId)

  if (!existingProfile || existingProfile.version < profile.version) {
    yield put(profileSuccess(userId, profile, connectedWeb3))
  }
}

function* submitProfileToRenderer(action: ProfileSuccessAction): any {
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
  yield call(ensureBaseCatalogs)

  const identity = yield select(getCurrentIdentity)
  const parcels: ParcelsWithAccess = !identity.hasConnectedWeb3 ? [] : yield fetchParcelsWithAccess(identity.address)
  const rendererFormat = profileToRendererFormat(profile, { identity, parcels })
  globalThis.unityInterface.LoadProfile(rendererFormat)
}

function* handleFetchInventory(action: InventoryRequest) {
  try {
    const inventoryItems = yield call(fetchInventoryItemsByAddress, action.payload.userId)
    yield put(inventorySuccess(action.payload.userId, inventoryItems))
  } catch (error) {
    yield put(inventoryFailure(action.payload.userId, error))
  }
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

function* handleSaveAvatar(saveAvatar: SaveProfileRequest) {
  const userId = saveAvatar.payload.userId ? saveAvatar.payload.userId : yield select(getCurrentUserId)

  try {
    const savedProfile: Profile | null = yield select(getProfile, userId)
    const currentVersion: number = savedProfile?.version && savedProfile?.version > 0 ? savedProfile?.version : 0
    const profile = { ...savedProfile, ...saveAvatar.payload.profile, ...{ version: currentVersion + 1 } } as Profile

    const identity: ExplorerIdentity = yield select(getCurrentIdentity)

    localProfilesRepo.persist(identity.address, profile)

    yield put(saveProfileSuccess(userId, profile.version, profile))

    // only update profile on server if wallet is connected
    if (identity.hasConnectedWeb3) {
      yield put(deployProfile(profile))
    } else {
      yield put(profileSavedNotDeployed(userId, profile.version, profile))
    }

    yield put(profileRequest(userId))
  } catch (error) {
    yield put(saveProfileFailure(userId, 'unknown reason'))
  }
}

function* handleDeployProfile(deployProfileAction: DeployProfile) {
  const url: string = yield select(getUpdateProfileServer)
  const identity: ExplorerIdentity = yield select(getCurrentIdentity)
  const userId: string = yield select(getCurrentUserId)
  const profile: Profile = deployProfileAction.payload.profile
  try {
    yield modifyAvatar({
      url,
      userId,
      identity,
      profile
    })
    yield put(deployProfileSuccess(userId, profile.version, profile))
  } catch (e) {
    defaultLogger.error('Error deploying profile!', e)
    yield put(deployProfileFailure(userId, profile, e))
  }
}

export function fetchProfileLocally(address: string) {
  const profile: Profile | null = localProfilesRepo.get(address)
  if (profile?.userId === address) {
    return ensureServerFormat(profile)
  } else {
    return null
  }
}

async function calculateBufferHash(buffer: Buffer): Promise<string> {
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

async function modifyAvatar(params: { url: string; userId: string; identity: ExplorerIdentity; profile: Profile }) {
  const { url, profile, identity } = params
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

  const metadata = buildServerMetadata({ ...profile, avatar: newAvatar })

  return deploy(url, identity, metadata, files, content)
}

async function deploy(
  url: string,
  identity: ExplorerIdentity,
  metadata: any,
  contentFiles: Map<string, Buffer>,
  contentHashes: Map<string, string>
) {
  // Build entity and group all files
  const preparationData = await (contentFiles.size
    ? DeploymentBuilder.buildEntity(EntityType.PROFILE, [identity.address], contentFiles, metadata)
    : DeploymentBuilder.buildEntityWithoutNewFiles(EntityType.PROFILE, [identity.address], contentHashes, metadata))
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
