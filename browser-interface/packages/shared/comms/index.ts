import { store } from 'shared/store/isolatedStore'
import { commsLogger } from './logger'
import { getCommsRoom } from './selectors'

export function sendPublicChatMessage(message: string) {
  const commsContext = getCommsRoom(store.getState())

  commsContext
    ?.sendChatMessage({
      message,
      timestamp: Date.now()
    })
    .catch((e) => commsLogger.warn(`error while sending message `, e))
}

export function sendParcelSceneCommsMessage(sceneId: string, data: Uint8Array) {
  const commsContext = getCommsRoom(store.getState())

  commsContext
    ?.sendParcelSceneMessage({
      data,
      sceneId
    })
    .catch((e) => commsLogger.warn(`error while sending message `, e))
}
