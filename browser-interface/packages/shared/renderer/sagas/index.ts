import { call, fork, take, takeEvery } from 'redux-saga/effects'
import { SEND_PROFILE_TO_RENDERER_REQUEST } from 'shared/profiles/actions'
import { RENDERER_INITIALIZE } from '../types'
import { initializeRenderer } from './initializeRenderer'
import { handleSubmitProfileToRenderer } from './profiles'
import { reportRealmChangeToRenderer } from './realms'
import { voiceChatRendererSagas } from './voiceChat'

export function* rendererSaga() {
  // Initialization
  const action = yield take(RENDERER_INITIALIZE)
  yield call(initializeRenderer, action)

  // Profile management
  yield takeEvery(SEND_PROFILE_TO_RENDERER_REQUEST, handleSubmitProfileToRenderer)

  // Realm and content server management
  yield fork(reportRealmChangeToRenderer)
  yield fork(voiceChatRendererSagas)
}
