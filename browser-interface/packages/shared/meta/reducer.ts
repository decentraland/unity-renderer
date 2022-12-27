import { AnyAction } from 'redux'
import { META_CONFIGURATION_INITIALIZED } from './actions'
import { MetaState } from './types'

const initialState: MetaState = {
  initialized: false,
  config: {}
}

export function metaReducer(state?: MetaState, action?: AnyAction): MetaState {
  if (!state) {
    return initialState
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case META_CONFIGURATION_INITIALIZED:
      return {
        ...state,
        initialized: true,
        config: action.payload
      }
    default:
      return state
  }
}
