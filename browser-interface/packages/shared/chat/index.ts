import { uuid } from 'atomicHelpers/math'

import { ChatMessageType } from 'shared/types'
import { messageReceived } from 'shared/chat/actions'
import { store } from 'shared/store/isolatedStore'

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
