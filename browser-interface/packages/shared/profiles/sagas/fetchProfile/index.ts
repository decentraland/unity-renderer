import type { Avatar } from '@dcl/schemas'
import { FETCH_REMOTE_PROFILE_RETRIES } from 'config'
import { generateRandomUserProfile } from 'lib/decentraland/profiles/generateRandomUserProfile'
import { ensureAvatarCompatibilityFormat } from 'lib/decentraland/profiles/transformations/profileToServerFormat'
import { call, put, select } from 'redux-saga/effects'
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
  } = (yield select(getInformationToFetchProfileFromStore, action)) as ReturnType<
    typeof getInformationToFetchProfileFromStore
  >

  if (existingProfile && existingProfileWithCorrectVersion) {
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
  if (options.canFetchFromComms && !!options.roomConnection) {
    // Retry three times
    for (let i = 0; i < FETCH_REMOTE_PROFILE_RETRIES; i++) {
      const remote = yield call(fetchPeerProfile, options.roomConnection, userId, version)
      if (remote) {
        return remote
      }
    }
  }
  if (options.canFetchFromCatalyst) {
    const catalyst = yield call(fetchCatalystProfile, userId, version)
    if (catalyst) {
      return catalyst
    }
  }
  return generateRandomUserProfile(userId)
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
