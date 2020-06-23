import { MessageEntry, ChatMessageType } from 'shared/types'
import { uuid } from 'atomicHelpers/math'
import { StoreContainer } from '../store/rootTypes'
import { messageReceived } from '../chat/actions'

declare const globalThis: StoreContainer

export enum ChatEventType {
  MESSAGE_RECEIVED = 'MESSAGE_RECEIVED',
  MESSAGE_SENT = 'MESSAGE_SENT'
}

export type ChatEvent = {
  type: ChatEventType
  messageEntry: MessageEntry
}

export function notifyStatusThroughChat(status: string) {
  globalThis.globalStore.dispatch(
    messageReceived({
      messageId: uuid(),
      messageType: ChatMessageType.SYSTEM,
      timestamp: Date.now(),
      body: status
    })
  )
}
