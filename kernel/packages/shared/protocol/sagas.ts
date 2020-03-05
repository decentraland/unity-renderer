import { put, select, takeLatest } from 'redux-saga/effects'
import { Context } from '../comms'
import { defaultLogger } from '../logger'
import { SaveProfileSuccess, SAVE_PROFILE_SUCCESS } from '../profiles/actions'
import { announceProfile, AnnounceProfileAction, ANNOUNCE_PROFILE } from './actions'
import { getCommsContext } from './selectors'

export function* rootProtocolSaga() {
  // Forwarding effects
  yield takeLatest(SAVE_PROFILE_SUCCESS, announceNewAvatar)

  // Handling of local actions
  yield takeLatest(ANNOUNCE_PROFILE, handleAnnounceProfile)
}

export function* announceNewAvatar(action: SaveProfileSuccess) {
  yield put(announceProfile(action.payload.userId, action.payload.version))
}

export function* handleAnnounceProfile(action: AnnounceProfileAction) {
  const context = (yield select(getCommsContext)) as Context | undefined
  if (context === undefined) {
    defaultLogger.warn('Announce profile is impossible (no connection found)')
    return
  }
  if (context.userInfo) {
    context.userInfo.version = action.payload.version
  }
}
