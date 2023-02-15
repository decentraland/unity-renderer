import type { Avatar } from '@dcl/schemas'
import { call, put, select } from 'redux-saga/effects'
import { trackEvent } from 'shared/analytics'
import type { RoomConnection } from 'shared/comms/interface'
import { getCommsRoom } from 'shared/comms/selectors'
import type { ProfileRequestAction } from 'shared/profiles/actions'
import { profileFailure, profileSuccess } from 'shared/profiles/actions'
import { generateRandomUserProfile } from 'lib/decentraland/profiles/generateRandomUserProfile'
import { isCurrentUserId, isGuestLogin } from 'shared/session/selectors'
import { getProfileStatusAndData } from '../../selectors'
import { ensureAvatarCompatibilityFormat } from '../../../../lib/decentraland/profiles/transformations/profileToServerFormat'
import type { ProfileStatus } from '../../types'
import { fetchPeerProfile } from '../comms'
import { fetchCatalystProfile } from '../content'
import { FETCH_REMOTE_PROFILE_RETRIES } from 'config'

export function* fetchProfile(action: ProfileRequestAction): any {
  const { userId, minimumVersion } = action.payload

  const roomConnection: RoomConnection | undefined = yield select(getCommsRoom)
  const hasRoomConnection = !!roomConnection
  const loadingCurrentUser: boolean = yield select(isCurrentUserId, userId)

  const [_, existingProfile]: [ProfileStatus?, Avatar?] = yield select(getProfileStatusAndData, userId)
  const existingProfileWithCorrectVersion =
    existingProfile && (!minimumVersion || existingProfile.version >= minimumVersion)

  if (existingProfileWithCorrectVersion) {
    yield put(profileSuccess(existingProfile))
    return existingProfile
  }

  try {
    const isCurrentUserGuest = loadingCurrentUser && (yield select(isGuestLogin))

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
