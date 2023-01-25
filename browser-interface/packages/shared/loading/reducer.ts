import { AnyAction } from 'redux'
import { PENDING_SCENES, InformPendingScenes, UPDATE_STATUS_MESSAGE } from './actions'
import { FATAL_ERROR, ExecutionLifecycleEvent, EXPERIENCE_STARTED, NOT_STARTED, TELEPORT_TRIGGERED } from './types'
import {
  RENDERING_ACTIVATED,
  RENDERING_BACKGROUND,
  RENDERING_DEACTIVATED,
  RENDERING_FOREGROUND
} from '../loadingScreen/types'

export type LoadingState = {
  status: ExecutionLifecycleEvent
  totalScenes: number
  pendingScenes: number
  /** @deprecated #3642 Message of the loading state will be moved to Renderer */
  message: string
  /** @deprecated #3642 Not used */
  renderingActivated: boolean
  /** @deprecated #3642 Will be moved to Renderer */
  // true if the rendering was activated at least once
  renderingWasActivated: boolean
  /** @deprecated #3642 Not used */
  isForeground: boolean
  /** @deprecated #3642 Will be moved to Renderer */
  initialLoad: boolean

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
      message: '',
      lastUpdate: 0,
      renderingActivated: false,
      renderingWasActivated: false,
      isForeground: true,
      initialLoad: true,
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
  if (action.type === RENDERING_ACTIVATED) {
    return { ...state, renderingActivated: true, renderingWasActivated: true }
  }
  if (action.type === RENDERING_DEACTIVATED) {
    return { ...state, renderingActivated: false }
  }
  if (action.type === RENDERING_FOREGROUND) {
    return { ...state, isForeground: true }
  }
  if (action.type === RENDERING_BACKGROUND) {
    return { ...state, isForeground: false }
  }
  if (action.type === EXPERIENCE_STARTED) {
    return { ...state, status: action.type, initialLoad: false }
  }
  if (action.type === TELEPORT_TRIGGERED) {
    return { ...state, message: action.payload }
  }
  if (action.type === UPDATE_STATUS_MESSAGE) {
    return { ...state, message: action.payload.message, lastUpdate: action.payload.lastUpdate }
  }
  if (action.type === FATAL_ERROR) {
    return { ...state, error: action.payload.type }
  }
  return state
}
