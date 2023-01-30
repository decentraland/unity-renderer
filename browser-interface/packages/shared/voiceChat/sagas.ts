import { call, select, takeEvery, takeLatest, put, apply, fork, take } from 'redux-saga/effects'
import { receiveUserTalking } from 'shared/comms/peers'
import { VOICE_CHAT_SAMPLE_RATE } from 'voice-chat-codec/constants'
import { VoiceHandler } from './VoiceHandler'
import {
  SET_VOICE_CHAT_MUTE,
  SET_VOICE_CHAT_VOLUME,
  VOICE_PLAYING_UPDATE,
  REQUEST_VOICE_CHAT_RECORDING,
  REQUEST_TOGGLE_VOICE_CHAT_RECORDING,
  VoicePlayingUpdateAction,
  SetVoiceChatVolumeAction,
  SetVoiceChatMuteAction,
  setVoiceChatHandler,
  voiceRecordingUpdate,
  voicePlayingUpdate,
  setVoiceChatError,
  leaveVoiceChat,
  SetVoiceChatErrorAction,
  SET_VOICE_CHAT_ERROR,
  SetVoiceChatMediaAction,
  setVoiceChatMedia,
  SET_VOICE_CHAT_MEDIA,
  clearVoiceChatError,
  SET_AUDIO_DEVICE,
  SetAudioDevice,
  SET_VOICE_CHAT_HANDLER,
  LEAVE_VOICE_CHAT,
  JOIN_VOICE_CHAT
} from './actions'
import { voiceChatLogger } from './context'
import { store } from 'shared/store/isolatedStore'
import {
  getVoiceHandler,
  isVoiceChatAllowedByCurrentScene,
  isRequestedVoiceChatRecording,
  getVoiceChatState,
  hasJoinedVoiceChat
} from './selectors'
import { positionObservable, PositionReport } from 'shared/world/positionThings'
import { positionReportToCommsPositionRfc4 } from 'shared/comms/interface/utils'
import { trackEvent } from 'shared/analytics'
import { VoiceChatState } from './types'

import { SET_ROOM_CONNECTION } from 'shared/comms/actions'
import { waitForMetaConfigurationInitialization } from 'shared/meta/sagas'
import { incrementCounter } from 'shared/occurences'
import { getCommsRoom } from 'shared/comms/selectors'
import { RoomConnection } from 'shared/comms/interface'

let audioRequestInitialized = false

export function* voiceChatSaga() {
  yield fork(reactToNewVoiceChatHandler)

  yield takeLatest(REQUEST_VOICE_CHAT_RECORDING, handleRecordingRequest)
  yield takeLatest(REQUEST_TOGGLE_VOICE_CHAT_RECORDING, handleRecordingRequest)

  yield takeEvery(VOICE_PLAYING_UPDATE, handleUserVoicePlaying)

  yield takeEvery(SET_VOICE_CHAT_VOLUME, handleVoiceChatVolume)
  yield takeEvery(SET_VOICE_CHAT_MUTE, handleVoiceChatMute)
  yield takeEvery(SET_VOICE_CHAT_MEDIA, handleVoiceChatMedia)
  yield takeEvery(SET_VOICE_CHAT_ERROR, handleVoiceChatError)

  yield fork(handleConnectVoiceChatToRoom)
  yield takeEvery(SET_AUDIO_DEVICE, setAudioDevices)
}

/**
 * This saga reconnects or disconnect the voiceChatHandler based on a setting or room change.
 */
function* handleConnectVoiceChatToRoom() {
  yield call(waitForMetaConfigurationInitialization)

  while (true) {
    const joined: boolean = yield select(hasJoinedVoiceChat)

    // if we are supposed to be joined, then ask the RoomConnection about the handler
    if (joined) {
      const room: RoomConnection = yield select(getCommsRoom)
      if (room) {
        try {
          const voiceHandler: VoiceHandler = yield apply(room, room.getVoiceHandler, [])
          yield put(setVoiceChatHandler(voiceHandler || null))
        } catch (err: any) {
          yield put(setVoiceChatError(err.toString()))
          yield put(setVoiceChatHandler(null))
        }
      }
    } else {
      yield put(setVoiceChatHandler(null))
    }

    // wait for next event to happen
    yield take([SET_ROOM_CONNECTION, JOIN_VOICE_CHAT, LEAVE_VOICE_CHAT])
  }
}

function* handleRecordingRequest() {
  const requestedRecording = yield select(isRequestedVoiceChatRecording)
  const voiceHandler: VoiceHandler | null = yield select(getVoiceHandler)

  if (voiceHandler) {
    const isAlowedByScene: boolean = yield select(isVoiceChatAllowedByCurrentScene)
    if (!isAlowedByScene || !requestedRecording) {
      voiceHandler.setRecording(false)
    } else {
      yield call(requestUserMediaIfNeeded)
      voiceHandler.setRecording(true)
    }
  }
}

function* reactToNewVoiceChatHandler() {
  let previousHandler: VoiceHandler | null = null

  positionObservable.add((obj: Readonly<PositionReport>) => {
    previousHandler?.reportPosition(positionReportToCommsPositionRfc4(obj))
  })

  while (true) {
    yield take(SET_VOICE_CHAT_HANDLER)

    const voiceChatState: VoiceChatState = yield select(getVoiceChatState)
    const voiceHandler = voiceChatState.voiceHandler

    if (voiceHandler === previousHandler) {
      continue
    }

    if (voiceHandler !== previousHandler && previousHandler) {
      yield previousHandler.destroy()
    }

    // set the state for the next round
    previousHandler = voiceHandler

    yield put(clearVoiceChatError())

    if (voiceHandler) {
      voiceChatLogger.log('Setting new voice handler', voiceHandler)

      voiceHandler.onRecording((recording) => {
        queueMicrotask(() => store.dispatch(voiceRecordingUpdate(recording)))
      })

      voiceHandler.onUserTalking((userId, talking) => {
        queueMicrotask(() => store.dispatch(voicePlayingUpdate(userId, talking)))
      })

      voiceHandler.onError((message) => {
        queueMicrotask(() => store.dispatch(setVoiceChatError(message)))
      })

      voiceHandler.setVolume(voiceChatState.volume)
      voiceHandler.setMute(voiceChatState.mute)

      if (voiceChatState.media) {
        yield put(setVoiceChatMedia(voiceChatState.media))
      }
    }
  }
}

function* handleVoiceChatError({ payload }: SetVoiceChatErrorAction) {
  if (payload.message) {
    trackEvent('error', {
      context: 'voice-chat',
      message: 'stream recording error: ' + payload.message,
      stack: ''
    })
    incrementCounter('voiceChatHandlerError')
    yield put(leaveVoiceChat())
  }
}

function* handleVoiceChatMedia({ payload }: SetVoiceChatMediaAction) {
  const voiceHandler: VoiceHandler | null = yield select(getVoiceHandler)
  if (voiceHandler && payload.media) {
    voiceChatLogger.info('Setting media stream', payload.media)
    yield voiceHandler.setInputStream(payload.media)
  }
}

function* requestUserMediaIfNeeded() {
  const voiceHandler: VoiceHandler | null = yield select(getVoiceHandler)
  if (voiceHandler) {
    if (!voiceHandler.hasInput()) {
      const media: MediaStream = yield call(requestMediaDevice)
      yield put(setVoiceChatMedia(media))
    }
  }
}

function* handleUserVoicePlaying(action: VoicePlayingUpdateAction) {
  const { userId, playing } = action.payload
  receiveUserTalking(userId, playing)
}

function* handleVoiceChatVolume(action: SetVoiceChatVolumeAction) {
  const voiceHandler: VoiceHandler | null = yield select(getVoiceHandler)
  voiceHandler?.setVolume(action.payload.volume)
}

function* handleVoiceChatMute(action: SetVoiceChatMuteAction) {
  const voiceHandler: VoiceHandler | null = yield select(getVoiceHandler)
  voiceHandler?.setMute(action.payload.mute)
}

function* setAudioDevices(action: SetAudioDevice) {
  if (!audioRequestInitialized && action.payload.devices.inputDeviceId) {
    const media = yield call(requestMediaDevice, action.payload.devices.inputDeviceId)
    yield put(setVoiceChatMedia(media))
  }
}

export async function requestMediaDevice(deviceId?: string) {
  if (!audioRequestInitialized) {
    audioRequestInitialized = true

    try {
      const media = await navigator.mediaDevices.getUserMedia({
        audio: {
          deviceId,
          channelCount: 1,
          sampleRate: VOICE_CHAT_SAMPLE_RATE,
          echoCancellation: true,
          noiseSuppression: true,
          autoGainControl: true,
          advanced: [{ echoCancellation: true }, { autoGainControl: true }, { noiseSuppression: true }] as any
        },
        video: false
      })

      return media
    } catch (e) {
      trackEvent('error', {
        context: 'voice-chat',
        message: 'Error requesting audio: ' + e,
        stack: ''
      })
      incrementCounter('voiceChatRequestMediaDeviceFail')
    } finally {
      audioRequestInitialized = false
    }
  }
}
