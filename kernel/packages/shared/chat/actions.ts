import { action } from 'typesafe-actions'
import { ChatMessage } from '../types'

export const MESSAGE_RECEIVED = 'Message received'
export const messageReceived = (message: ChatMessage) => action(MESSAGE_RECEIVED, message)
export type MessageReceived = ReturnType<typeof messageReceived>

export const SEND_MESSAGE = '[Request] Send message'
export const sendMessage = (message: ChatMessage) => action(SEND_MESSAGE, message)
export type SendMessage = ReturnType<typeof sendMessage>
