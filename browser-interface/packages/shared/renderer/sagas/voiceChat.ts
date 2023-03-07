import { isSceneFeatureToggleEnabled } from 'lib/decentraland/sceneJson/isSceneFeatureToggleEnabled'
import { call, fork, select, take, takeEvery, takeLatest } from 'redux-saga/effects'
import { NotificationType, VOICE_CHAT_FEATURE_TOGGLE } from 'shared/types'
import {
  SetVoiceChatErrorAction,
  SET_VOICE_CHAT_ERROR,
  SET_VOICE_CHAT_HANDLER,
  VoicePlayingUpdateAction,
  VoiceRecordingUpdateAction,
  VOICE_PLAYING_UPDATE,
  VOICE_RECORDING_UPDATE
} from 'shared/voiceChat/actions'
import { getVoiceHandler } from 'shared/voiceChat/selectors'
import { VoiceHandler } from 'shared/voiceChat/VoiceHandler'
import { SetCurrentScene, SET_CURRENT_SCENE } from 'shared/world/actions'
import { getSceneWorkerBySceneID } from 'shared/world/parcelSceneManager'
import { SceneWorker } from 'shared/world/SceneWorker'
import { getUnityInterface } from 'unity-interface/IUnityInterface'
import { waitForRendererInstance } from '../sagas-helper'

export function* voiceChatRendererSagas() {
  yield call(waitForRendererInstance)
  yield takeEvery(VOICE_PLAYING_UPDATE, updateUserVoicePlayingRenderer)
  yield takeEvery(VOICE_RECORDING_UPDATE, updatePlayerVoiceRecordingRenderer)
  yield takeEvery(SET_VOICE_CHAT_ERROR, handleVoiceChatError)
  yield takeLatest(SET_CURRENT_SCENE, listenToWhetherSceneSupportsVoiceChat)
  yield fork(updateChangeVoiceChatHandlerProcess)
}

function* updateUserVoicePlayingRenderer(action: VoicePlayingUpdateAction) {
  const { playing, userId } = action.payload
  yield call(waitForRendererInstance)
  getUnityInterface().SetUserTalking(userId, playing)
}

function* updatePlayerVoiceRecordingRenderer(action: VoiceRecordingUpdateAction) {
  yield call(waitForRendererInstance)
  getUnityInterface().SetPlayerTalking(action.payload.recording)
}

function* updateChangeVoiceChatHandlerProcess() {
  let prevHandler: VoiceHandler | undefined = undefined
  while (true) {
    // wait for a new VoiceHandler
    yield take(SET_VOICE_CHAT_HANDLER)

    const handler: VoiceHandler | undefined = yield select(getVoiceHandler)

    if (handler !== prevHandler) {
      if (prevHandler) {
        yield prevHandler.destroy()
      }
      prevHandler = handler
    }

    yield call(waitForRendererInstance)

    if (handler) {
      getUnityInterface().SetVoiceChatStatus({ isConnected: true })
    } else {
      getUnityInterface().SetVoiceChatStatus({ isConnected: false })
    }
  }
}

function* handleVoiceChatError(action: SetVoiceChatErrorAction) {
  const message = action.payload.message
  yield call(waitForRendererInstance)
  if (message) {
    getUnityInterface().ShowNotification({
      type: NotificationType.GENERIC,
      message,
      buttonMessage: 'OK',
      timer: 5
    })
  }
}

function* listenToWhetherSceneSupportsVoiceChat(data: SetCurrentScene) {
  const currentScene: SceneWorker | undefined = data.payload.currentScene
    ? yield call(getSceneWorkerBySceneID, data.payload.currentScene)
    : undefined

  const nowEnabled = currentScene
    ? isSceneFeatureToggleEnabled(VOICE_CHAT_FEATURE_TOGGLE, currentScene?.metadata)
    : isSceneFeatureToggleEnabled(VOICE_CHAT_FEATURE_TOGGLE)

  yield call(waitForRendererInstance)

  getUnityInterface().SetVoiceChatEnabledByScene(nowEnabled)
}
