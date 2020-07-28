import { AnyAction } from 'redux'

import { SessionState } from './types'
import { USER_AUTHENTIFIED, UserAuthentified } from './actions'

const INITIAL_STATE: SessionState = {
  initialized: false,
  identity: undefined,
  userId: undefined,
  network: undefined
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
  }
  return state
}
