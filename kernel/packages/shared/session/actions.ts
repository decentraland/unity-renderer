import { action } from 'typesafe-actions'

import { ETHEREUM_NETWORK } from 'config'

import { ExplorerIdentity } from './types'
import { Profile } from '../profiles/types'

export const INIT_SESSION = '[Session] initializing'
export const initSession = () => action(INIT_SESSION)
export type InitSession = ReturnType<typeof initSession>

export const AUTHENTICATE = '[Authenticate]'
export const authenticate = (provider: string) => action(AUTHENTICATE, { provider })
export type AuthenticateAction = ReturnType<typeof authenticate>

export const SIGNUP = '[SIGNUP]'
export const signUp = () => action(SIGNUP)

export const USER_AUTHENTIFIED = '[Success] User authentified'
export const userAuthentified = (userId: string, identity: ExplorerIdentity, network: ETHEREUM_NETWORK) =>
  action(USER_AUTHENTIFIED, { userId, identity, network })
export type UserAuthentified = ReturnType<typeof userAuthentified>

export const LOGIN_COMPLETED = '[Success] Login'
export const loginCompleted = () => action(LOGIN_COMPLETED)

export const LOGOUT = '[Request] Logout'
export const logout = () => action(LOGOUT)
export type Logout = ReturnType<typeof logout>

export const REDIRECT_TO_SIGN_UP = '[Request] Redirect to SignUp'
export const redirectToSignUp = () => action(REDIRECT_TO_SIGN_UP)
export type RedirectToSignUp = ReturnType<typeof redirectToSignUp>

export const UPDATE_TOS = '[UPDATE_TOS]'
export const updateTOS = (agreed: boolean) => action(UPDATE_TOS, agreed)

export const SIGNUP_FORM = '[SIGNUP_FORM]'
export const signupForm = (name: string, email: string) => action(SIGNUP_FORM, { name, email })
export type SignUpFormAction = ReturnType<typeof signupForm>

export const SIGNUP_STAGE = '[SIGNUP_STAGE]'
export const changeSignUpStage = (stage: string) => action(SIGNUP_STAGE, { stage })
export type ChangeSignUpStageAction = ReturnType<typeof changeSignUpStage>

export const CHANGE_LOGIN_STAGE = '[LOGIN_STAGE] change login stage'
export const changeLoginStage = (stage: string) => action(CHANGE_LOGIN_STAGE, { stage })

export const TOGGLE_WALLET_PROMPT = '[WALLET_PROMPT] show wallet prompt'
export const toggleWalletPrompt = (show: boolean) => action(TOGGLE_WALLET_PROMPT, { show })

export const SIGNUP_SET_PROFILE = '[SIGN-UP] signup set profile'
export const signUpSetProfile = (profile: Partial<Profile>) => action(SIGNUP_SET_PROFILE, profile)
export type SignUpSetProfileAction = ReturnType<typeof signUpSetProfile>

export const SIGNUP_SET_IDENTITY = '[SIGN-UP] set identity'
export const signUpSetIdentity = (userId: string, identity: ExplorerIdentity) =>
  action(SIGNUP_SET_IDENTITY, { userId, identity })
export type SignUpSetIdentityAction = ReturnType<typeof signUpSetIdentity>

export const SIGNUP_CANCEL = '[SIGN-UP-CANCEL]'
export const signUpCancel = () => action(SIGNUP_CANCEL)

export const SIGNUP_CLEAR_DATA = '[SIGN-UP] clear data'
export const signUpClearData = () => action(SIGNUP_CLEAR_DATA)

export const SIGNIN_CURRENT_PROVIDER = '[SIGN-IN] set initial wallet'
export const signInSetCurrentProvider = (provider: string) => action(SIGNIN_CURRENT_PROVIDER, { provider })

export const SIGNUP_COME_BACK_TO_AVATAR_EDITOR = '[SIGNUP_COME_BACK_TO_AVATAR_EDITOR]'
export const signUpComeBackToAvatarEditor = () => action(SIGNUP_COME_BACK_TO_AVATAR_EDITOR)

export const SIGNIN_SET_SIGNING = '[SIGN-IN] start authentication'
export const signInSigning = (signing: boolean) => action(SIGNIN_SET_SIGNING, { signing })

export const SIGNUP_SET_IS_SIGNUP = '[SIGN-UP] mark session as new user'
export const signUpSetIsSignUp = (isSignUp: boolean) => action(SIGNUP_SET_IS_SIGNUP, { isSignUp })
