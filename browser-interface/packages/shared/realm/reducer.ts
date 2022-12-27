import { AnyAction } from 'redux'

import { RealmState } from './types'
import { SET_REALM_ADAPTER } from './actions'

const INITIAL_COMMS: RealmState = {
  realmAdapter: undefined
}

export function realmReducer(state?: RealmState, action?: AnyAction): RealmState {
  if (!state) {
    return INITIAL_COMMS
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case SET_REALM_ADAPTER:
      if (state.realmAdapter === action.payload) {
        return state
      }
      return { ...state, realmAdapter: action.payload }
    default:
      return state
  }
}
