import { action } from 'typesafe-actions'

import { ChatMessage, FriendshipAction } from 'shared/types'

import { FriendsState } from './types'
import { SocialAPI, SocialId } from 'dcl-social-client'
import { IFuture } from 'fp-future'
import { FriendshipErrorCode } from 'shared/protocol/decentraland/renderer/common/friend_request_common.gen'

export const UPDATE_FRIENDSHIP = 'Update friendship'
export const updateFriendship = (
  _action: FriendshipAction,
  userId: string,
  incoming: boolean,
  future: IFuture<{ userId: string; error: FriendshipErrorCode | null }>,
  messageBody?: string | undefined
) => action(UPDATE_FRIENDSHIP, { action: _action, userId, future, messageBody }, { incoming })
export type UpdateFriendship = ReturnType<typeof updateFriendship>

export const SET_MATRIX_CLIENT = '[CHAT] Set Matrix client'
export const setMatrixClient = (socialApi: SocialAPI) => action(SET_MATRIX_CLIENT, { socialApi })
export type SetMatrixClient = ReturnType<typeof setMatrixClient>

export const UPDATE_PRIVATE_MESSAGING = 'Update private messaging state'
export const updatePrivateMessagingState = (state: FriendsState) => action(UPDATE_PRIVATE_MESSAGING, state)
export type UpdatePrivateMessagingState = ReturnType<typeof updatePrivateMessagingState>

export const UPDATE_USER_DATA = 'Update user data'
export const updateUserData = (userId: string, socialId: string, conversationId?: string) =>
  action(UPDATE_USER_DATA, { userId, socialId, conversationId })
export type UpdateUserData = ReturnType<typeof updateUserData>

export const JOIN_OR_CREATE_CHANNEL = 'Join or create channel'
export const joinOrCreateChannel = (channelId: string, userIds: SocialId[]) =>
  action(JOIN_OR_CREATE_CHANNEL, { channelId, userIds })
export type JoinOrCreateChannel = ReturnType<typeof joinOrCreateChannel>

export const LEAVE_CHANNEL = 'Leave channel'
export const leaveChannel = (channelId: string) => action(LEAVE_CHANNEL, { channelId })
export type LeaveChannel = ReturnType<typeof leaveChannel>

export const SEND_CHANNEL_MESSAGE = '[Request] Send channel message'
export const sendChannelMessage = (channelId: string, message: ChatMessage) =>
  action(SEND_CHANNEL_MESSAGE, { channelId, message })
export type SendChannelMessage = ReturnType<typeof sendChannelMessage>
