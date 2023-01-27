import { AnyAction } from 'redux'
import { SET_CATALYST_CANDIDATES, SELECT_NETWORK, SelectNetworkAction } from './actions'
import { DaoState } from './types'

export function daoReducer(state?: DaoState, action?: AnyAction): DaoState {
  if (!state) {
    return {
      network: null,
      candidates: [],
      catalystCandidatesReceived: false
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
  }
  return state
}
