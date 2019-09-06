import {
  AuthRequestAction,
  AuthSuccessAction,
  AuthFailureAction,
  AUTH_REQUEST,
  AUTH_SUCCESS,
  AUTH_FAILURE
} from './actions'
import { AuthState } from './types'

export const INITIAL_STATE: AuthState = {
  loading: [],
  data: null,
  error: null
}

function removeLast(actions: any, comparator: any) {
  const last = actions.filter(comparator).pop()
  return actions.filter(function(action: any) {
    return action !== last
  })
}
const getType = function(action: any) {
  return action.type.slice(10)
}
const getStatus = function(action: any) {
  return action.type.slice(1, 8).toUpperCase()
}

function loadingReducer(initialState: any, action: any) {
  const state = initialState ? initialState : INITIAL_STATE
  const type = getType(action)
  const status = getStatus(action)
  switch (status) {
    case 'REQUEST': {
      return state.concat([action])
    }
    case 'FAILURE':
    case 'SUCCESS': {
      return removeLast(state, function(actionItem: any) {
        return getType(actionItem) === type
      })
    }
    default:
      return state
  }
}

export type AuthReducerAction = AuthRequestAction | AuthSuccessAction | AuthFailureAction

export function authReducer(state: AuthState = INITIAL_STATE, action: AuthReducerAction) {
  switch (action.type) {
    case AUTH_REQUEST: {
      return {
        ...state,
        loading: loadingReducer(state.loading, action)
      }
    }
    case AUTH_SUCCESS: {
      return {
        ...state,
        data: action.payload.data,
        loading: loadingReducer(state.loading, action),
        error: null
      }
    }
    case AUTH_FAILURE: {
      return {
        ...state,
        data: null,
        loading: loadingReducer(state.loading, action),
        error: action.payload.error
      }
    }
    default: {
      return state
    }
  }
}
