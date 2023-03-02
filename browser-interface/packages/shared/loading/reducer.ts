import { AnyAction } from 'redux'
import { InformPendingScenes, PENDING_SCENES } from './actions'
import { ExecutionLifecycleEvent, EXPERIENCE_STARTED, FATAL_ERROR, NOT_STARTED } from './types'

type LoadingState = {
  status: ExecutionLifecycleEvent
  totalScenes: number
  pendingScenes: number

  lastUpdate: number | null
  error: string | null
  tldError: {
    tld: string
    web3Net: string
    tldNet: string
  } | null
}

export type RootLoadingState = {
  loading: LoadingState
}

export function loadingReducer(state?: LoadingState, action?: AnyAction): LoadingState {
  if (!state) {
    return {
      status: NOT_STARTED,
      totalScenes: 0,
      pendingScenes: 0,
      lastUpdate: 0,
      error: null,
      tldError: null
    }
  }
  if (!action) {
    return state
  }
  if (action.type === PENDING_SCENES) {
    return {
      ...state,
      pendingScenes: (action as InformPendingScenes).payload.pendingScenes,
      totalScenes: (action as InformPendingScenes).payload.totalScenes
    }
  }
  if (action.type === EXPERIENCE_STARTED) {
    return { ...state, status: action.type }
  }
  if (action.type === FATAL_ERROR) {
    return { ...state, error: action.payload.type }
  }
  return state
}
