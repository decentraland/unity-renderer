import { AnyAction } from 'redux'

import { LoginStage, SessionState } from './types'
import {
  CHANGE_LOGIN_STAGE,
  ChangeSignUpStageAction,
  SIGNIN_CURRENT_PROVIDER,
  SIGNIN_SET_SIGNING,
  SIGNUP_CLEAR_DATA,
  SIGNUP_FORM,
  SIGNUP_SET_IDENTITY,
  SIGNUP_SET_IS_SIGNUP,
  SIGNUP_SET_PROFILE,
  SIGNUP_STAGE,
  SignUpFormAction,
  SignUpSetIdentityAction,
  SignUpSetProfileAction,
  TOGGLE_WALLET_PROMPT,
  UPDATE_TOS,
  USER_AUTHENTIFIED,
  UserAuthentified
} from './actions'

const SIGNUP_INITIAL_STATE = {
  stage: '',
  profile: {},
  userId: undefined,
  identity: undefined
}

const INITIAL_STATE: SessionState = {
  initialized: false,
  identity: undefined,
  userId: undefined,
  network: undefined,
  loginStage: LoginStage.LOADING,
  tos: true,
  showWalletPrompt: false,
  signing: false,
  currentProvider: null,
  isSignUp: false,
  signup: SIGNUP_INITIAL_STATE
}

export function sessionReducer(state?: SessionState, action?: AnyAction) {
  if (!state) {
    return INITIAL_STATE
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case USER_AUTHENTIFIED: {
      return { ...state, initialized: true, ...(action as UserAuthentified).payload }
    }
    case UPDATE_TOS: {
      return { ...state, tos: action.payload }
    }
    case CHANGE_LOGIN_STAGE: {
      return { ...state, loginStage: action.payload.stage }
    }
    case TOGGLE_WALLET_PROMPT:
      return { ...state, showWalletPrompt: action.payload.show }
    case SIGNUP_STAGE:
      return {
        ...state,
        signup: {
          ...state.signup,
          ...(action as ChangeSignUpStageAction).payload
        }
      }
    case SIGNUP_FORM:
      const { name, email } = (action as SignUpFormAction).payload
      return {
        ...state,
        signup: {
          ...state.signup,
          profile: {
            ...state.signup.profile,
            unclaimedName: name,
            email
          }
        }
      }
    case SIGNUP_SET_PROFILE: {
      const { name, email, ...values } = (action as SignUpSetProfileAction).payload
      return {
        ...state,
        signup: {
          ...state.signup,
          profile: {
            ...state.signup.profile,
            ...values
          }
        }
      }
    }
    case SIGNIN_SET_SIGNING: {
      return {
        ...state,
        ...action.payload
      }
    }
    case SIGNUP_SET_IDENTITY: {
      return {
        ...state,
        signup: {
          ...state.signup,
          ...(action as SignUpSetIdentityAction).payload
        }
      }
    }
    case SIGNUP_CLEAR_DATA: {
      return {
        ...state,
        signup: SIGNUP_INITIAL_STATE
      }
    }
    case SIGNUP_SET_IS_SIGNUP: {
      return {
        ...state,
        isSignUp: action.payload.isSignUp
      }
    }
    case SIGNIN_CURRENT_PROVIDER: {
      return {
        ...state,
        currentProvider: action.payload.provider
      }
    }
  }
  return state
}
