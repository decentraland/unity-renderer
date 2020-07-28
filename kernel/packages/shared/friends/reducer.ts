import { AnyAction } from 'redux'

import { FriendsState, SocialData } from './types'
import { UPDATE_PRIVATE_MESSAGING, UpdatePrivateMessagingState, UPDATE_USER_DATA, UpdateUserData } from './actions'

const FRIENDS_INITIAL_STATE: FriendsState = {
  client: null,
  socialInfo: {},
  friends: [],
  toFriendRequests: [],
  fromFriendRequests: []
}

type State = FriendsState

export function friendsReducer(state?: State, action?: AnyAction): State {
  if (!state) {
    return FRIENDS_INITIAL_STATE
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case UPDATE_PRIVATE_MESSAGING: {
      return reducePrivateMessaging(state, action as UpdatePrivateMessagingState)
    }
    case UPDATE_USER_DATA: {
      return reduceUpdateUserData(state, action as UpdateUserData)
    }
  }
  return state
}

function reducePrivateMessaging(state: State, action: UpdatePrivateMessagingState) {
  return { ...state, ...action.payload }
}

function reduceUpdateUserData(state: State, action: UpdateUserData) {
  const socialData = state.socialInfo[action.payload.socialId]
  if (socialData && deepEquals(socialData, action.payload)) {
    // return state as is if user data exists and is equal
    return state
  }

  return {
    ...state,
    socialInfo: {
      ...state.socialInfo,
      [action.payload.socialId]: action.payload
    }
  }
}

function deepEquals(a: SocialData, b: SocialData) {
  return a.userId === b.userId && a.socialId === b.socialId && a.conversationId === b.conversationId
}
