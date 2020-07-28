import { AnyAction } from 'redux'

import { RendererState, RENDERER_INITIALIZED } from './types'
import { RENDERER_ENABLED, RendererEnabled } from './actions'

const INITIAL_STATE: RendererState = {
  initialized: false,
  instancedJS: undefined
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
    case RENDERER_ENABLED:
      return {
        ...state,
        instancedJS: (action as RendererEnabled).payload.instancedJS
      }
    default:
      return state
  }
}
