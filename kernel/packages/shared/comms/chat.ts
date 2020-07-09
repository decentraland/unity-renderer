import { MessageEntry, ChatMessageType, PresenceStatus, UpdateUserStatusMessage } from 'shared/types'
import { uuid } from 'atomicHelpers/math'
import { StoreContainer } from '../store/rootTypes'
import { messageReceived } from '../chat/actions'
import { getProfile } from 'shared/profiles/selectors'

declare const globalThis: StoreContainer
let friendStatus: Record<string, PresenceStatus> = {}

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

export function notifyFriendOnlineStatusThroughChat(userStatus: UpdateUserStatusMessage) {
  const friendName = getProfile(globalThis.globalStore.getState(), userStatus.userId)?.name

  if (friendName === undefined) {
    return
  }

  if (!friendStatus[friendName]) {
    friendStatus[friendName] = userStatus.presence
    return
  }

  if (!(userStatus.realm?.layer && userStatus.realm?.serverName)) {
    if (userStatus.presence !== PresenceStatus.ONLINE) {
      friendStatus[friendName] = userStatus.presence
    }
    return
  }

  if (userStatus.presence === PresenceStatus.ONLINE && friendStatus[friendName] === PresenceStatus.OFFLINE) {
    let message = `${friendName} joined ${userStatus.realm?.serverName}-${userStatus.realm?.layer}`

    if (userStatus.position) {
      message += ` ${userStatus.position.x}, ${userStatus.position.y}`
    }

    notifyStatusThroughChat(message)
  }

  friendStatus[friendName] = userStatus.presence
}
