import { RootRendererState } from './types'

export function isInitialized(state: RootRendererState) {
  return state && state.renderer && state.renderer.initialized
}
