import {
  JOIN_VOICE_CHAT,
  LEAVE_VOICE_CHAT,
  REQUEST_VOICE_CHAT_RECORDING,
  SET_AUDIO_DEVICE,
  SET_VOICE_CHAT_ERROR,
  SET_VOICE_CHAT_HANDLER,
  SET_VOICE_CHAT_MEDIA,
  SET_VOICE_CHAT_MUTE,
  SET_VOICE_CHAT_POLICY,
  SET_VOICE_CHAT_VOLUME,
  VoiceChatActions,
  VOICE_RECORDING_UPDATE
} from './actions'
import { VoiceChatState, VoicePolicy } from './types'

const INITIAL_STATE: VoiceChatState = {
  joined: false,
  voiceHandler: null,
  recording: false,
  requestRecording: false,
  policy: VoicePolicy.ALLOW_ALL,
  volume: 1.0,
  mute: false,
  error: null,
  inputDeviceId: null,
  outputDeviceId: null
}

export function voiceChatReducer(state?: VoiceChatState, action?: VoiceChatActions): VoiceChatState {
  if (!state) {
    return INITIAL_STATE
  }

  if (!action) {
    return state
  }

  switch (action.type) {
    case SET_VOICE_CHAT_HANDLER: {
      const { payload } = action
      return { ...state, voiceHandler: payload.voiceHandler }
    }
    case SET_VOICE_CHAT_POLICY: {
      const { payload } = action
      return { ...state, policy: payload.policy }
    }
    case REQUEST_VOICE_CHAT_RECORDING: {
      const { payload } = action
      return { ...state, requestRecording: payload.recording }
    }
    case SET_VOICE_CHAT_VOLUME: {
      const { payload } = action
      return { ...state, volume: payload.volume }
    }
    case SET_VOICE_CHAT_MUTE: {
      const { payload } = action
      return { ...state, mute: payload.mute }
    }
    case VOICE_RECORDING_UPDATE: {
      const { payload } = action
      return { ...state, recording: payload.recording }
    }
    case SET_VOICE_CHAT_MEDIA: {
      const { payload } = action
      return { ...state, media: payload.media }
    }
    case SET_VOICE_CHAT_ERROR: {
      const { payload } = action
      return { ...state, error: payload.message }
    }
    case JOIN_VOICE_CHAT: {
      return { ...state, joined: true }
    }
    case LEAVE_VOICE_CHAT: {
      return { ...state, joined: false }
    }
    case SET_AUDIO_DEVICE: {
      const { payload } = action
      return {
        ...state,
        inputDeviceId: payload.devices.inputDeviceId ?? state.inputDeviceId,
        outputDeviceId: payload.devices.outputDeviceId ?? state.outputDeviceId
      }
    }
  }

  return state
}
