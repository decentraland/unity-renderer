import { debounce, fork, put, takeEvery } from 'redux-saga/effects'
import type { Avatar } from '@dcl/schemas'
import { unsignedCRC32 } from 'lib/encoding/crc32'
import defaultLogger from 'lib/logger'
import { generateRandomUserProfile as buildRandomProfile } from 'lib/decentraland/profiles/generateRandomUserProfile'
import { USER_AUTHENTICATED } from 'shared/session/actions'
import type { ProfileSuccessAction } from '../actions'
import {
  DEPLOY_PROFILE_REQUEST,
  PROFILE_REQUEST,
  PROFILE_SUCCESS,
  SAVE_DELTA_PROFILE_REQUEST,
  sendProfileToRenderer
} from '../actions'
import { ensureAvatarCompatibilityFormat } from 'lib/decentraland/profiles/transformations/profileToServerFormat'
import type { RemoteProfile } from '../types'
import { createFakeName } from 'lib/decentraland/profiles/names/fakeName'
import { takeLatestById } from './takeLatestById'
import { fetchProfile } from './fetchProfile'
import { handleCommsProfile, handleCommsVersionUpdates } from './handleCommsProfile'
import { handleDeployProfile } from './handleDeployProfile'
import { handleSaveLocalAvatar } from './handleSaveLocalAvatar'
import { initialRemoteProfileLoad } from './initialRemoteProfileLoad'
import { cachedRequest } from './content/cachedRequest'

const concatenatedActionTypeUserId = (action: { type: string; payload: { userId: string } }) =>
  action.type + action.payload.userId

export const takeLatestByUserId = (patternOrChannel: any, saga: any, ...args: any) =>
  takeLatestById(patternOrChannel, concatenatedActionTypeUserId, saga, ...args)

export function* profileSaga(): any {
  /**
   * When the user is authentified, we fetch the profile from the catalyst, to
   * cover the case where the profile was updated on a different device.
   */
  yield takeEvery(USER_AUTHENTICATED, initialRemoteProfileLoad)
  /**
   * Handle profile requests (see the `fetchProfile` folder)
   */
  yield takeLatestByUserId(PROFILE_REQUEST, fetchProfile)
  /**
   * Redirect successful profile updates/fetches to the rendering engine
   *
   * It's *very* important for the renderer to never receive a passport with
   * items that have not been loaded into the catalog.
   */
  yield takeLatestByUserId(PROFILE_SUCCESS, forwardProfileToRenderer)
  /**
   * Receive profiles from the comms system
   */
  yield fork(handleCommsProfile)
  yield fork(handleCommsVersionUpdates)
  /**
   * We debounce this because sometimes `DEPLOY_PROFILE_REQUEST` gets called too frequently by renderer
   */
  yield debounce(200, DEPLOY_PROFILE_REQUEST, handleDeployProfile)
  /**
   * Manage a request by the user to save the current user profile
   */
  yield takeEvery(SAVE_DELTA_PROFILE_REQUEST, handleSaveLocalAvatar)
}

function* forwardProfileToRenderer(action: ProfileSuccessAction) {
  yield put(sendProfileToRenderer(action.payload.profile.userId))
}

export async function generateRandomUserProfile(userId: string, reason: string): Promise<Avatar> {
  defaultLogger.info('Generating random profile for ' + userId + ' ' + reason)

  const bytes = new TextEncoder().encode(userId)

  // deterministically find the same random profile for each user
  const _number = 1 + (unsignedCRC32(bytes) % 160)

  let profile: Avatar | undefined = undefined
  try {
    const profiles: RemoteProfile | null = await cachedRequest(`default${_number}`)
    if (!profiles) {
      throw new Error('Could not request default avatars!')
    }
    if (profiles?.avatars.length !== 0) {
      profile = profiles.avatars[0]
    }
  } catch (e: any) {
    // in case something fails keep going and use backup profile
    defaultLogger.warn(e.message)
  }

  if (!profile) {
    profile = buildRandomProfile(userId)
  }

  profile.ethAddress = userId
  profile.userId = userId
  profile.avatar.snapshots.face256 = profile.avatar.snapshots.face256 ?? (profile.avatar.snapshots as any).face
  profile.name = createFakeName(userId)
  profile.hasClaimedName = false
  profile.tutorialStep = 0
  profile.version = 0

  return ensureAvatarCompatibilityFormat(profile)
}
