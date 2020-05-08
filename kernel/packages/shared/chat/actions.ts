import { action } from 'typesafe-actions'
import { ChatMessage, FriendshipAction } from '../types'
import { ChatState } from './types'

export const MESSAGE_RECEIVED = 'Message received'
export const messageReceived = (message: ChatMessage) => action(MESSAGE_RECEIVED, message)
export type MessageReceived = ReturnType<typeof messageReceived>

export const SEND_MESSAGE = '[Request] Send message'
export const sendMessage = (message: ChatMessage) => action(SEND_MESSAGE, message)
export type SendMessage = ReturnType<typeof sendMessage>

export const SEND_PRIVATE_MESSAGE = '[Request] Send private message'
export const sendPrivateMessage = (userId: string, message: string) => action(SEND_PRIVATE_MESSAGE, { userId, message })
export type SendPrivateMessage = ReturnType<typeof sendPrivateMessage>

export const UPDATE_FRIENDSHIP = 'Update friendship'
export const updateFriendship = (_action: FriendshipAction, userId: string, incoming: boolean) =>
  action(UPDATE_FRIENDSHIP, { action: _action, userId }, { incoming })
export type UpdateFriendship = ReturnType<typeof updateFriendship>

export const UPDATE_PRIVATE_MESSAGING = 'Update private messaging state'
export const updatePrivateMessagingState = (state: ChatState['privateMessaging']) =>
  action(UPDATE_PRIVATE_MESSAGING, state)
export type UpdatePrivateMessagingState = ReturnType<typeof updatePrivateMessagingState>

export const UPDATE_USER_DATA = 'Update user data'
export const updateUserData = (userId: string, socialId: string, conversationId?: string) =>
  action(UPDATE_USER_DATA, { userId, socialId, conversationId })
export type UpdateUserData = ReturnType<typeof updateUserData>
