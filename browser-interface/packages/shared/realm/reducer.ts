import { AnyAction } from 'redux'

import { OnboardingState, RealmState } from './types'
import { SET_ONBOARDING_STATE, SET_REALM_ADAPTER } from './actions'

const INITIAL_COMMS: RealmState = {
  realmAdapter: undefined,
  previousAdapter: undefined
}

const INITIAL_ONBOARDING_STATE: OnboardingState = {
  isInOnboarding: false,
  onboardingRealm: undefined
}

export function realmReducer(state?: RealmState, action?: AnyAction): RealmState {
  if (!state) {
    return INITIAL_COMMS
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case SET_REALM_ADAPTER:
      if (state.realmAdapter === action.payload) {
        return state
      }
      return { ...state, realmAdapter: action.payload, previousAdapter: state.realmAdapter }
    default:
      return state
  }
}

export function onboardingReducer(state: OnboardingState, action: AnyAction): OnboardingState {
  if (!state) {
    return INITIAL_ONBOARDING_STATE
  }
  switch (action.type) {
    case SET_ONBOARDING_STATE:
      return { isInOnboarding: action.payload.isInOnboarding, onboardingRealm: action.payload.onboardingRealm }
    default:
      return state
  }
}
