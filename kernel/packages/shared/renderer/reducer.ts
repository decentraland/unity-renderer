import { AnyAction } from 'redux'
import { ENGINE_STARTED, RendererState, RENDERER_INITIALIZED } from './types'

const INITIAL_STATE: RendererState = {
  initialized: false,
  engineStarted: false
}

export function rendererReducer(state?: RendererState, action?: AnyAction): RendererState {
  if (!state) {
    return INITIAL_STATE
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case RENDERER_INITIALIZED:
      return {
        ...state,
        initialized: true
      }
    case ENGINE_STARTED:
      return {
        ...state,
        engineStarted: true
      }
    default:
      return state
  }
}
