import { action } from 'typesafe-actions'

import { ChatMessage } from 'shared/types'

export const MESSAGE_RECEIVED = 'Message received'
export const messageReceived = (message: ChatMessage) => action(MESSAGE_RECEIVED, message)
export type MessageReceived = ReturnType<typeof messageReceived>

export const SEND_MESSAGE = '[Request] Send message'
export const sendMessage = (message: ChatMessage) => action(SEND_MESSAGE, message)
export type SendMessage = ReturnType<typeof sendMessage>

export const SEND_PRIVATE_MESSAGE = '[Request] Send private message'
export const sendPrivateMessage = (userId: string, message: ChatMessage) =>
  action(SEND_PRIVATE_MESSAGE, { userId, message })
export type SendPrivateMessage = ReturnType<typeof sendPrivateMessage>
