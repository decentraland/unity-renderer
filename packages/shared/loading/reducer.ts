import { AnyAction } from 'redux'
import { Events, EventsList, NOT_STARTED } from './types'

export type LoadingState = {
  status: Events
}
export function loadingReducer(state: LoadingState, action: AnyAction) {
  if (!state) {
    return { status: NOT_STARTED }
  }
  if (!action) {
    return state
  }
  if (action.type in EventsList) {
    return { status: action.type }
  }
  return state
}
