import { action } from 'typesafe-actions'

import { FriendshipAction } from 'shared/types'

import { FriendsState } from './types'

export const UPDATE_FRIENDSHIP = 'Update friendship'
export const updateFriendship = (_action: FriendshipAction, userId: string, incoming: boolean) =>
  action(UPDATE_FRIENDSHIP, { action: _action, userId }, { incoming })
export type UpdateFriendship = ReturnType<typeof updateFriendship>

export const UPDATE_PRIVATE_MESSAGING = 'Update private messaging state'
export const updatePrivateMessagingState = (state: FriendsState) => action(UPDATE_PRIVATE_MESSAGING, state)
export type UpdatePrivateMessagingState = ReturnType<typeof updatePrivateMessagingState>

export const UPDATE_USER_DATA = 'Update user data'
export const updateUserData = (userId: string, socialId: string, conversationId?: string) =>
  action(UPDATE_USER_DATA, { userId, socialId, conversationId })
export type UpdateUserData = ReturnType<typeof updateUserData>
