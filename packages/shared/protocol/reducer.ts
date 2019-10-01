import { AnyAction } from 'redux'
import { ProtocolState } from './types'
import { SET_WORLD_CONTEXT } from './actions'
import { Context } from '../comms'

export function protocolReducer(state?: ProtocolState, action?: AnyAction): ProtocolState {
  if (!state) {
    return { context: undefined }
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case SET_WORLD_CONTEXT:
      return { ...state, context: action.payload as Context | undefined }
  }
  return state
}
