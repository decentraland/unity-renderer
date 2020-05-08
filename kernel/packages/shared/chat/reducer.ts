import { AnyAction } from 'redux'
import { ChatState, SocialData } from './types'
import { UPDATE_PRIVATE_MESSAGING, UpdatePrivateMessagingState, UPDATE_USER_DATA, UpdateUserData } from './actions'

const CHAT_INITIAL_STATE: ChatState = {
  privateMessaging: {
    client: null,
    socialInfo: {},
    friends: [],
    toFriendRequests: [],
    fromFriendRequests: []
  }
}

export function chatReducer(state?: ChatState, action?: AnyAction) {
  if (!state) {
    return CHAT_INITIAL_STATE
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

function reducePrivateMessaging(state: ChatState, action: UpdatePrivateMessagingState) {
  return { ...state, privateMessaging: action.payload }
}

function reduceUpdateUserData(state: ChatState, action: UpdateUserData) {
  const socialData = state.privateMessaging.socialInfo[action.payload.socialId]
  if (socialData && deepEquals(socialData, action.payload)) {
    // return state as is if user data exists and is equal
    return state
  }

  return {
    ...state,
    privateMessaging: {
      ...state.privateMessaging,
      socialInfo: {
        ...state.privateMessaging.socialInfo,
        [action.payload.socialId]: action.payload
      }
    }
  }
}

function deepEquals(a: SocialData, b: SocialData) {
  return a.userId === b.userId && a.socialId === b.socialId && a.conversationId === b.conversationId
}
