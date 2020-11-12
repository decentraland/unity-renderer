import { RootLoadingState } from './reducer'

export const isInitialLoading = (state: RootLoadingState) => state.loading.initialLoad
export const isWaitingTutorial = (state: RootLoadingState) => state.loading.waitingTutorial
