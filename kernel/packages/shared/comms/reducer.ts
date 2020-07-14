import { COMMS_ESTABLISHED } from '../loading/types'
import { AnyAction } from 'redux'

export type CommsState = {
  initialized: boolean
}

const INITIAL_COMMS = {
  initialized: false
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
      return { initialized: true }
    default:
      return state
  }
}
