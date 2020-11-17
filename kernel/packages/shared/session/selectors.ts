import { LoginStage, RootSessionState } from './types'

// TODO use userId
export const getUserId = (state: RootSessionState) => state.session.userId
export const getCurrentUserId = (store: RootSessionState) => store.session.identity?.address
export const getCurrentIdentity = (store: RootSessionState) => store.session.identity
export const getCurrentNetwork = (store: RootSessionState) => store.session.network
export const hasWallet = (store: RootSessionState) => store.session.identity?.hasConnectedWeb3
export const getSignUpProfile = (store: RootSessionState) => store.session.signup.profile
export const getSignUpIdentity = (store: RootSessionState) => ({
  userId: store.session.signup.userId,
  identity: store.session.signup.identity
})
export const isSignUp = (state: RootSessionState) => state.session.isSignUp
export const isLoginStageCompleted = (state: RootSessionState) => state.session.loginStage === LoginStage.COMPLETED
