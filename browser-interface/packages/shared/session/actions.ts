import { action } from 'typesafe-actions'

import { ETHEREUM_NETWORK } from 'config'

import { ExplorerIdentity } from './types'
import { IEthereumProvider, LoginState } from '@dcl/kernel-interface'

export const INIT_SESSION = '[SESSION] Initializing'
export const initSession = () => action(INIT_SESSION)
export type InitSession = ReturnType<typeof initSession>

export const AUTHENTICATE = '[SESSION] Authenticate'
export const authenticate = (provider: IEthereumProvider, isGuest: boolean) =>
  action(AUTHENTICATE, { provider, isGuest })
export type AuthenticateAction = ReturnType<typeof authenticate>

export const SIGNUP = '[SESSION] SignUp'
export const signUp = (email: string, name: string) => action(SIGNUP, { email, name })
export type SignUpAction = ReturnType<typeof signUp>

export const USER_AUTHENTICATED = '[SESSION] User authenticated'
export const userAuthenticated = (identity: ExplorerIdentity, network: ETHEREUM_NETWORK, isGuest: boolean) =>
  action(USER_AUTHENTICATED, { identity, network, isGuest })
export type UserAuthenticated = ReturnType<typeof userAuthenticated>

export const LOGOUT = '[SESSION] Logout'
export const logout = () => action(LOGOUT)
export type Logout = ReturnType<typeof logout>

export const REDIRECT_TO_SIGN_UP = '[Request] Redirect to SignUp'
export const redirectToSignUp = () => action(REDIRECT_TO_SIGN_UP)
export type RedirectToSignUp = ReturnType<typeof redirectToSignUp>

export const UPDATE_TOS = '[SESSION] UPDATE_TOS'
export const updateTOS = (agreed: boolean) => action(UPDATE_TOS, agreed)

export const CHANGE_LOGIN_STAGE = '[SESSION] change login stage'
export const changeLoginState = (stage: LoginState) => action(CHANGE_LOGIN_STAGE, { stage })
export type ChangeLoginStateAction = ReturnType<typeof changeLoginState>

export const SIGNUP_CANCEL = '[SESSION] Cancel signup'
export const signUpCancel = () => action(SIGNUP_CANCEL)

export const SIGNUP_CLEAR_DATA = '[SESSION] Clear signup data'
export const signUpClearData = () => action(SIGNUP_CLEAR_DATA)

export const SIGNUP_SET_IS_SIGNUP = '[SESSION] mark session as new user'
export const signUpSetIsSignUp = (isSignUp: boolean) => action(SIGNUP_SET_IS_SIGNUP, { isSignUp })
export type SignUpSetIsSignUp = ReturnType<typeof signUpSetIsSignUp>
