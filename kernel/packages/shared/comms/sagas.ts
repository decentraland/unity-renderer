import { put, takeEvery, select, call, takeLatest } from 'redux-saga/effects'

import { STATIC_WORLD } from 'config'

import { establishingComms, FATAL_ERROR } from 'shared/loading/types'
import { USER_AUTHENTIFIED } from 'shared/session/actions'
import { getCurrentIdentity } from 'shared/session/selectors'
import { setWorldContext } from 'shared/protocol/actions'
import { ensureRealmInitialized, selectRealm } from 'shared/dao/sagas'
import { getRealm } from 'shared/dao/selectors'
import { CATALYST_REALMS_SCAN_SUCCESS, setCatalystRealm } from 'shared/dao/actions'
import { Realm } from 'shared/dao/types'
import { realmToString } from 'shared/dao/utils/realmToString'
import { createLogger } from 'shared/logger'

import {
  connect,
  disconnect,
  updatePeerVoicePlaying,
  updateVoiceCommunicatorMute,
  updateVoiceCommunicatorVolume,
  updateVoiceRecordingStatus
} from '.'
import {
  SetVoiceMute,
  SetVoiceVolume,
  SET_VOICE_CHAT_RECORDING,
  SET_VOICE_MUTE,
  SET_VOICE_VOLUME,
  TOGGLE_VOICE_CHAT_RECORDING,
  VoicePlayingUpdate,
  VoiceRecordingUpdate,
  VOICE_PLAYING_UPDATE,
  VOICE_RECORDING_UPDATE
} from './actions'

import { isVoiceChatRecording } from './selectors'
import { unityInterface } from 'unity-interface/UnityInterface'
import { ensureMetaConfigurationInitialized } from 'shared/meta'
import { isVoiceChatEnabledFor } from 'shared/meta/selectors'
import { userAuthentified } from 'shared/session'
import { sceneObservable } from 'shared/world/sceneState'
import { SceneFeatureToggles } from 'shared/types'
import { isFeatureToggleEnabled } from 'shared/selectors'

const DEBUG = false
const logger = createLogger('comms: ')

export function* commsSaga() {
  yield takeEvery(USER_AUTHENTIFIED, establishCommunications)
  yield takeLatest(CATALYST_REALMS_SCAN_SUCCESS, changeRealm)
  yield ensureMetaConfigurationInitialized()
  yield userAuthentified()

  const identity = yield select(getCurrentIdentity)

  if (yield select(isVoiceChatEnabledFor, identity.address)) {
    yield takeEvery(SET_VOICE_CHAT_RECORDING, updateVoiceChatRecordingStatus)
    yield takeEvery(TOGGLE_VOICE_CHAT_RECORDING, updateVoiceChatRecordingStatus)
    yield takeEvery(VOICE_PLAYING_UPDATE, updateUserVoicePlaying)
    yield takeEvery(VOICE_RECORDING_UPDATE, updatePlayerVoiceRecording)
    yield takeEvery(SET_VOICE_VOLUME, updateVoiceChatVolume)
    yield takeEvery(SET_VOICE_MUTE, updateVoiceChatMute)
    yield listenToWhetherSceneSupportsVoiceChat()
  }

  yield takeEvery(FATAL_ERROR, bringDownComms)
}

function* bringDownComms() {
  disconnect()
}

function* listenToWhetherSceneSupportsVoiceChat() {
  sceneObservable.add(({ previousScene, newScene }) => {
    const previouslyEnabled = previousScene
      ? isFeatureToggleEnabled(SceneFeatureToggles.VOICE_CHAT, previousScene.sceneJsonData)
      : undefined
    const nowEnabled = isFeatureToggleEnabled(SceneFeatureToggles.VOICE_CHAT, newScene.sceneJsonData)
    if (previouslyEnabled !== nowEnabled) {
      unityInterface.SetVoiceChatEnabledByScene(nowEnabled)
      if (!nowEnabled) {
        // We want to stop any potential recordings when a user enters a new scene
        updateVoiceRecordingStatus(false)
      }
    }
  })
}

function* establishCommunications() {
  if (STATIC_WORLD) {
    return
  }

  yield call(ensureRealmInitialized)

  const identity = yield select(getCurrentIdentity)

  yield put(establishingComms())
  const context = yield connect(identity.address)
  if (context !== undefined) {
    yield put(setWorldContext(context))
  }
}

function* updateVoiceChatRecordingStatus() {
  const recording = yield select(isVoiceChatRecording)
  updateVoiceRecordingStatus(recording)
}

function* updateUserVoicePlaying(action: VoicePlayingUpdate) {
  updatePeerVoicePlaying(action.payload.userId, action.payload.playing)
}

function* updateVoiceChatVolume(action: SetVoiceVolume) {
  updateVoiceCommunicatorVolume(action.payload.volume)
}

function* updateVoiceChatMute(action: SetVoiceMute) {
  updateVoiceCommunicatorMute(action.payload.mute)
}

function* updatePlayerVoiceRecording(action: VoiceRecordingUpdate) {
  unityInterface.SetPlayerTalking(action.payload.recording)
}

function* changeRealm() {
  const currentRealm: ReturnType<typeof getRealm> = yield select(getRealm)
  if (!currentRealm) {
    DEBUG && logger.info(`No realm set, wait for actual DAO initialization`)
    // if not realm is set => wait for actual dao initialization
    return
  }

  const otherRealm = yield call(selectRealm)

  if (!sameRealm(currentRealm, otherRealm)) {
    logger.info(`Changing realm from ${realmToString(currentRealm)} to ${realmToString(otherRealm)}`)
    yield put(setCatalystRealm(otherRealm))
  } else {
    DEBUG && logger.info(`Realm already set ${realmToString(currentRealm)}`)
  }
}

function sameRealm(realm1: Realm, realm2: Realm) {
  return realm1.catalystName === realm2.catalystName && realm1.layer === realm2.layer
}
