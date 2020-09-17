import { AnyAction } from 'redux'

import { COMMS_ESTABLISHED } from 'shared/loading/types'

import { CommsState } from './types'
import { SET_VOICE_CHAT_RECORDING } from './actions'

const INITIAL_COMMS = {
  initialized: false,
  voiceChatRecording: false
}

export function commsReducer(state?: CommsState, action?: AnyAction): CommsState {
  if (!state) {
    return INITIAL_COMMS
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case COMMS_ESTABLISHED:
      return { ...state, initialized: true }
    case SET_VOICE_CHAT_RECORDING:
      return { ...state, voiceChatRecording: action.payload.recording }
    default:
      return state
  }
}
