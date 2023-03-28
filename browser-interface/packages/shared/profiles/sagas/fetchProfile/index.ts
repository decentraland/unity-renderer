import type { Avatar } from '@dcl/schemas'
import { FETCH_REMOTE_PROFILE_RETRIES } from 'config'
import { generateRandomUserProfile } from 'lib/decentraland/profiles/generateRandomUserProfile'
import { ensureAvatarCompatibilityFormat } from 'lib/decentraland/profiles/transformations/profileToServerFormat'
import defaultLogger from 'lib/logger'
import { call, CallEffect, put, race, select } from 'redux-saga/effects'
import { trackEvent } from 'shared/analytics/trackEvent'
import type { RoomConnection } from 'shared/comms/interface'
import { getCommsRoom } from 'shared/comms/selectors'
import type { ProfileRequestAction } from 'shared/profiles/actions'
import { profileFailure, profileSuccess } from 'shared/profiles/actions'
import { isCurrentUserId, isGuestLogin as isGuestLoginSelector } from 'shared/session/selectors'
import type { RootState } from 'shared/store/rootTypes'
import { getProfileStatusAndData } from '../../selectors'
import type { ProfileStatus } from '../../types'
import { fetchPeerProfile } from '../comms'
import { fetchCatalystProfile } from '../content'

export function* fetchProfile(action: ProfileRequestAction): any {
  const { userId, minimumVersion } = action.payload
  const {
    roomConnection,
    loadingCurrentUser,
    hasRoomConnection,
    existingProfile,
    isGuestLogin,
    existingProfileWithCorrectVersion
  } = yield select(getInformationToFetchProfileFromStore, action)

  if (existingProfileWithCorrectVersion) {
    yield put(profileSuccess(existingProfile))
    return existingProfile
  }

  try {
    const isCurrentUserGuest = loadingCurrentUser && isGuestLogin

    const canFetchFromComms = !loadingCurrentUser && hasRoomConnection
    const canFetchFromCatalyst = !isCurrentUserGuest

    const profile = yield call(fetchWithBestStrategy, userId, {
      canFetchFromComms,
      canFetchFromCatalyst,
      roomConnection,
      minimumVersion
    })

    const avatar: Avatar = ensureAvatarCompatibilityFormat(profile)
    avatar.userId = userId

    if (loadingCurrentUser) {
      avatar.hasConnectedWeb3 = true
    }

    yield put(profileSuccess(avatar))
    return avatar
  } catch (error: any) {
    debugger
    trackEvent('error', {
      context: 'kernel#saga',
      message: `Error requesting profile for ${userId}: ${error}`,
      stack: error.stack || ''
    })
    yield put(profileFailure(userId, `${error}`))
  }
}

function* fetchWithBestStrategy(
  userId: string,
  options: {
    canFetchFromComms: boolean
    canFetchFromCatalyst: boolean
    roomConnection: RoomConnection | undefined
    minimumVersion: number | undefined
  }
) {
  const version = +(options.minimumVersion || 0)

  const fetches: { [k: string]: Generator<CallEffect<Avatar | null>> } = {}
  if (options.canFetchFromComms && !!options.roomConnection) {
    fetches.comms = fetchFromComms(options.roomConnection, userId, version)
  }
  if (options.canFetchFromCatalyst) {
    fetches.catalyst = fetchFromCatalyst(userId, version)
  }

  // If any of the fetching options is available, race against them and return the one that retrieves the profile quicker and with the correct content
  if (Object.keys(fetches).length) {
    const { comms, catalyst } = yield race(fetches)
    if (comms) {
      return comms
    } else if (catalyst) {
      return catalyst
    }
  }

  // If no other choice is available, generate a random user profile
  return generateRandomUserProfile(userId)
}

function* fetchFromComms(roomConnection: RoomConnection, userId: string, version: number) {
  // Retry three times
  for (let i = 0; i < FETCH_REMOTE_PROFILE_RETRIES; i++) {
    const remote = yield call(fetchPeerProfile, roomConnection, userId, version)
    if (remote) {
      return remote
    }
  }
  return null
}

function* fetchFromCatalyst(userId: string, version: number) {
  const catalyst: Avatar | null = yield call(fetchCatalystProfile, userId, version)
  if (catalyst) {
    if (catalyst.version >= version) {
      return catalyst
    }

    defaultLogger.warn(`expected profile min version ${version} from catalyst but got ${catalyst.version}`)
  }
  return null
}

export function getInformationToFetchProfileFromStore(state: RootState, action: ProfileRequestAction) {
  const { userId, minimumVersion } = action.payload
  const roomConnection: RoomConnection | undefined = getCommsRoom(state)
  const hasRoomConnection = !!roomConnection
  const loadingCurrentUser: boolean = isCurrentUserId(state, userId)
  const isGuestLogin = isGuestLoginSelector(state)

  const [_, existingProfile]: [ProfileStatus?, Avatar?] = getProfileStatusAndData(state, userId)
  const existingProfileWithCorrectVersion =
    existingProfile && (!minimumVersion || existingProfile.version >= minimumVersion)
  return {
    roomConnection,
    loadingCurrentUser,
    hasRoomConnection,
    existingProfile,
    isGuestLogin,
    existingProfileWithCorrectVersion
  }
}
