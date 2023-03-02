import { AnyAction } from 'redux'

import { SessionState } from './types'
import {
  CHANGE_LOGIN_STAGE,
  SIGNUP_SET_IS_SIGNUP,
  USER_AUTHENTICATED,
  UserAuthenticated,
  AUTHENTICATE,
  AuthenticateAction,
  ChangeLoginStateAction,
  SET_LOADING_SCREEN_OFF
} from './actions'
import { LoginState } from 'kernel-web-interface'

const INITIAL_STATE: SessionState = {
  identity: undefined,
  network: undefined,
  loginState: LoginState.LOADING,
  isSignUp: false,
  isLoadingScreenOff: false
}

export function sessionReducer(state?: SessionState, action?: AnyAction): SessionState {
  if (!state) {
    return INITIAL_STATE
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case USER_AUTHENTICATED: {
      return { ...state, ...(action as UserAuthenticated).payload }
    }
    case CHANGE_LOGIN_STAGE: {
      console.log("LOADINGSCREEN:  CHANGE_LOGIN_STATE")
      return { ...state, loginState: (action as ChangeLoginStateAction).payload.stage }
    }
    case AUTHENTICATE: {
      console.log("LOADINGSCREEN:  AUTHENTICATE")
      return {
        ...state,
        isGuestLogin: (action as AuthenticateAction).payload.isGuest,
        provider: (action as AuthenticateAction).payload.provider
      }
    }
    case SIGNUP_SET_IS_SIGNUP: {
      return {
        ...state,
        isSignUp: action.payload.isSignUp
      }
    }
    case SET_LOADING_SCREEN_OFF: {
      console.log("LOADINGSCREEN: Reducer called " + action.payload.isLoadingScreenOff)
      return {
        ...state,
        isLoadingScreenOff: action.payload.isLoadingScreenOff
      }
    }
  }
  return state
}
