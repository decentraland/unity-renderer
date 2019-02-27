import { Observable } from 'decentraland-ecs/src'

export const ChatEvent = {
  MESSAGE_RECEIVED: 'MESSAGE_RECEIVED',
  MESSAGE_SENT: 'MESSAGE_SENT'
}

export const chatObservable = new Observable()
