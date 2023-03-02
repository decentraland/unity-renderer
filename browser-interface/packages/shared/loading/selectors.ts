import { RootLoadingState } from './reducer'

export const getFatalError = (state: RootLoadingState) => state.loading.error
export const getLastUpdateTime = (state: RootLoadingState) => state.loading.lastUpdate
