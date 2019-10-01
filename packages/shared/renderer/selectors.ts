import { RootRendererState } from './types'

export function isInitialized(state: RootRendererState) {
  return state.renderer.initialized
}
