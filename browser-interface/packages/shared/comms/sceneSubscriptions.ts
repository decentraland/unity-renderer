import { PeerInformation } from './interface/types'

export interface ICommunicationsController {
  cid: string
  receiveCommsMessage(message: Uint8Array, sender: PeerInformation): void
}

export const scenesSubscribedToCommsEvents = new Set<ICommunicationsController>()

export function subscribeParcelSceneToCommsMessages(controller: ICommunicationsController) {
  scenesSubscribedToCommsEvents.add(controller)
}

export function unsubscribeParcelSceneToCommsMessages(controller: ICommunicationsController) {
  scenesSubscribedToCommsEvents.delete(controller)
}

/**
 * Returns a list of CIDs that must receive scene messages from comms
 */
export function getParcelSceneSubscriptions(): string[] {
  const ids: string[] = []

  scenesSubscribedToCommsEvents.forEach(($) => {
    ids.push($.cid)
  })

  return ids
}
