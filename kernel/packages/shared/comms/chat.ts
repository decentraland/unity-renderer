import { Observable } from 'decentraland-ecs/src'
import { MessageEntry } from 'shared/types'

export enum ChatEvent {
  MESSAGE_RECEIVED = 'MESSAGE_RECEIVED',
  MESSAGE_SENT = 'MESSAGE_SENT'
}

export const chatObservable = new Observable<{
  type: ChatEvent
  messageEntry: MessageEntry
}>()
