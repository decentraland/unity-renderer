import { AnyAction } from 'redux'
import { SET_CATALYST_CANDIDATES, SELECT_NETWORK, SelectNetworkAction, SET_LAST_CONNECTED_CANDIDATES } from './actions'
import { DaoState } from './types'

export function daoReducer(state?: DaoState, action?: AnyAction): DaoState {
  if (!state) {
    return {
      network: null,
      candidates: [],
      catalystCandidatesReceived: false,
      lastConnectedCandidates: new Map()
    }
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case SELECT_NETWORK:
      return {
        ...state,
        network: (action as SelectNetworkAction).payload
      }
    case SET_CATALYST_CANDIDATES:
      return {
        ...state,
        catalystCandidatesReceived: true,
        candidates: action.payload
      }
    case SET_LAST_CONNECTED_CANDIDATES:
      return {
        ...state,
        lastConnectedCandidates: action.payload
      }
  }
  return state
}
