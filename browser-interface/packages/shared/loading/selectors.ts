import { RootLoadingState } from './reducer'

export const getFatalError = (state: RootLoadingState) => state.loading.error
export const getLoadingState = (state: RootLoadingState) => state.loading
