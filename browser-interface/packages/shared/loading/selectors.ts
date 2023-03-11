import { RootRendererState } from 'shared/renderer/types'
import { RootLoadingState } from './reducer'

export const getFatalError = (state: RootLoadingState) => state.loading.error

export function scenesLoaded(state: RootRendererState & RootLoadingState) {
  if (!state.renderer.parcelLoadingStarted) {
    return true
  }
  if (!state.loading.totalScenes) {
    return true
  }
  const { pendingScenes } = state.loading
  return pendingScenes > 0
}
