import { IEthereumProvider, LoginState } from '@dcl/kernel-interface'
import { RootSessionState } from './types'

// TODO use userId
export const isCurrentUserId = (store: RootSessionState, userId: string) =>
  store.session.identity?.address.toLowerCase() === userId.toLowerCase()
export const getCurrentUserId = (store: RootSessionState) => store.session.identity?.address
export const getCurrentIdentity = (store: RootSessionState) => store.session.identity
export const getCurrentNetwork = (store: RootSessionState) => store.session.network
export const hasWallet = (store: RootSessionState) => store.session.identity?.hasConnectedWeb3
export const isSignupInProgress = (state: RootSessionState): boolean => !!state.session.isSignUp
export const isGuestLogin = (state: RootSessionState): boolean => !!state.session.isGuestLogin
export const getProvider = (state: RootSessionState): IEthereumProvider | undefined => state.session.provider

export function isLoginCompleted(state: RootSessionState) {
  return (
    (state.session.identity && state.session.provider && state.session.loginState === LoginState.COMPLETED) || false
  )
}
