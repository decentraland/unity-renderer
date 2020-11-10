import { RootLoadingState } from './reducer'

export const isInitialLoading = (state: RootLoadingState) => state.loading.initialLoad
