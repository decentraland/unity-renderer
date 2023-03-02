import { RootState } from 'shared/store/rootTypes'
import { RootLoadingState } from './reducer'

export const getFatalError = (state: RootLoadingState) => state.loading.error
export const getLoadingState = (state: RootLoadingState) => state.loading
export const getLastUpdateTime = (state: RootLoadingState) => state.loading.lastUpdate

export function shouldWaitForScenes(state: RootState) {
  if (!state.renderer.parcelLoadingStarted) {
    return true
  }

  // in the initial load, we should wait until we have *some* scene to load
  if (state.loading.initialLoad) {
    if (state.loading.pendingScenes !== 0 || state.loading.totalScenes === 0) {
      return true
    }
  }

  // otherwise only wait until pendingScenes == 0
  return state.loading.pendingScenes !== 0
}
