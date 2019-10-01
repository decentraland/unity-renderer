import { put, select, takeEvery } from 'redux-saga/effects'
import { Context } from '../comms'
import { defaultLogger } from '../logger'
import { SaveAvatarSuccess, SAVE_AVATAR_SUCCESS } from '../passports/actions'
import { announceProfile, AnnounceProfileAction, ANNOUNCE_PROFILE } from './actions'
import { getCommsContext } from './selectors'

export function* rootProtocolSaga() {
  // Forwarding effects
  yield takeEvery(SAVE_AVATAR_SUCCESS, announceNewAvatar)

  // Handling of local actions
  yield takeEvery(ANNOUNCE_PROFILE, handleAnnounceProfile)
}

export function* announceNewAvatar(action: SaveAvatarSuccess) {
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
