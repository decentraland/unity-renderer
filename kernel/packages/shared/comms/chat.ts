import { Observable } from 'decentraland-ecs/src'
import { MessageEntry } from 'shared/types'
import { uuid } from 'atomicHelpers/math'

export enum ChatEvent {
  MESSAGE_RECEIVED = 'MESSAGE_RECEIVED',
  MESSAGE_SENT = 'MESSAGE_SENT'
}

export const chatObservable = new Observable<{
  type: ChatEvent
  messageEntry: MessageEntry
}>()

export function notifyStatusThroughChat(status: string) {
  chatObservable.notifyObservers({
    type: ChatEvent.MESSAGE_RECEIVED,
    messageEntry: {
      id: uuid(),
      isCommand: true,
      sender: 'Decentraland',
      message: status
    }
  })
}
