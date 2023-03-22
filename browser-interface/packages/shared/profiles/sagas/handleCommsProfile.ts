import type { Avatar } from '@dcl/schemas'
import type { EventChannel } from 'redux-saga'
import { call, put, select, spawn, take } from 'redux-saga/effects'
import { createReceiveProfileOverCommsChannel, createVersionUpdateOverCommsChannel } from 'shared/comms/handlers'
import { getCommsRoom } from 'shared/comms/selectors'
import { profileSuccess } from '../actions'
import { getProfileFromStore } from '../selectors'
import type { ProfileUserInfo } from '../types'
import { fetchPeerProfile } from './comms'

/**
 * Handle comms profiles. If we have the profile then it calls a profileSuccess to
 * store it and forward it to the renderer.
 */
export function* handleCommsProfile() {
  const channelEvent: EventChannel<Avatar> = yield call(createReceiveProfileOverCommsChannel)

  while (true) {
    // TODO: Add signatures and verifications to this profile-over-comms mechanism
    // It's not that critical as long as `shared/comms` only sends events after validating userId === peer.address
    const receivedProfile: Avatar = yield take(channelEvent)
    const existingProfile: ProfileUserInfo | null = yield select(getProfileFromStore, receivedProfile.userId)

    if (!existingProfile || existingProfile.data?.version <= receivedProfile.version) {
      // TEMP:
      receivedProfile.hasConnectedWeb3 = receivedProfile.hasConnectedWeb3 || false

      // store profile locally and forward to renderer
      yield put(profileSuccess(receivedProfile))
    }
  }
}

/**
 * Watch for updated profiles
 */
export function* handleCommsVersionUpdates() {
  const channelEvent: EventChannel<Avatar> = yield call(createVersionUpdateOverCommsChannel)

  while (true) {
    const { userId, version } = yield take(channelEvent)
    const existingProfile: ProfileUserInfo | null = yield select(getProfileFromStore, userId)

    if (!existingProfile || existingProfile.data?.version <= version) {
      const roomConnection = yield select(getCommsRoom)
      yield spawn(fetchPeerProfile, roomConnection, userId, version)
    }
  }
}
