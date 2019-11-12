import { RendererState, RENDERER_INITIALIZED } from './types'
import { AnyAction } from 'redux'

const INITIAL_STATE: RendererState = {
  initialized: false
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
        initialized: true
      }
    default:
      return state
  }
}
