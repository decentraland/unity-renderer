import { AnyAction } from 'redux'
import {
  SET_CATALYST_CANDIDATES,
  CATALYST_REALMS_SCAN_FINISHED,
  SetCatalystCandidates,
  CatalystRealmsScanFinished
} from './actions'
import { CatalystSelectionState } from './types'

export function catalystSelectionReducer(state?: CatalystSelectionState, action?: AnyAction): CatalystSelectionState {
  if (!state) {
    return {
      network: null,
      candidates: [],
      catalystCandidatesReceived: false,
      currentCatalyst: null
    }
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case SET_CATALYST_CANDIDATES:
      return {
        ...state,
        catalystCandidatesReceived: true,
        candidates: (action as SetCatalystCandidates).payload
      }
    case CATALYST_REALMS_SCAN_FINISHED:
      return {
        ...state,
        currentCatalyst: (action as CatalystRealmsScanFinished).payload
      }
  }
  return state
}
