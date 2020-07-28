import { AnyAction } from 'redux'

import { COMMS_ESTABLISHED } from 'shared/loading/types'

import { CommsState } from './types'

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
