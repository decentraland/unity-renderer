import { uuid } from 'lib/javascript/uuid'
import { messageReceived } from 'shared/chat/actions'
import { store } from 'shared/store/isolatedStore'
import { ChatMessageType } from 'shared/types'

export function notifyStatusThroughChat(status: string) {
  store.dispatch(
    messageReceived({
      messageId: uuid(),
      messageType: ChatMessageType.SYSTEM,
      timestamp: Date.now(),
      body: status
    })
  )
}
