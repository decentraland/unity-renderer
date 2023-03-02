import { RootLoadingState } from './reducer'

export const getFatalError = (state: RootLoadingState) => state.loading.error
