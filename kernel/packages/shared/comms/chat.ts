import { Observable } from 'decentraland-ecs/src'
import { MessageEntry, ChatMessageType } from 'shared/types'
import { uuid } from 'atomicHelpers/math'
import { StoreContainer } from '../store/rootTypes'
import { messageReceived } from '../chat/actions'
import { USE_NEW_CHAT } from '../../config/index'

declare const globalThis: StoreContainer

export enum ChatEventType {
  MESSAGE_RECEIVED = 'MESSAGE_RECEIVED',
  MESSAGE_SENT = 'MESSAGE_SENT'
}

export type ChatEvent = {
  type: ChatEventType
  messageEntry: MessageEntry
}

export const chatObservable = new Observable<ChatEvent>()

export function notifyStatusThroughChat(status: string) {
  if (USE_NEW_CHAT) {
    globalThis.globalStore.dispatch(
      messageReceived({
        messageId: uuid(),
        messageType: ChatMessageType.SYSTEM,
        timestamp: Date.now(),
        body: status
      })
    )
  } else {
    chatObservable.notifyObservers({
      type: ChatEventType.MESSAGE_RECEIVED,
      messageEntry: {
        id: uuid(),
        sender: 'Decentraland',
        isCommand: true,
        message: status,
        timestamp: Date.now()
      }
    })
  }
}
