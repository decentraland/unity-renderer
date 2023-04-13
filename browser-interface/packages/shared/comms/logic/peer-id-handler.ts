import { Emitter } from 'mitt'
import * as rfc4 from 'shared/protocol/decentraland/kernel/comms/rfc4/comms.gen'
import { PeerDisconnectedEvent } from '../adapters/types'
import { CommsEvents } from '../interface'

export type CachedPeer = {
  address?: string
  position?: rfc4.Position
  profileResponse?: rfc4.ProfileResponse
  profileAnnounce?: rfc4.AnnounceProfileVersion
}

// This component abstracts the opaque peerId from transports into something that
// works for RFC4, replacing the peerId for the address whenever it is suitable
// Transports that support stateful address handles for peers may not use
// this component
export function peerIdHandler(options: { events: Emitter<CommsEvents> }) {
  const peers = new Map<string, CachedPeer>()

  function getPeer(sender: string): CachedPeer {
    if (!peers.has(sender)) {
      peers.set(sender, {})
    }
    return peers.get(sender)!
  }

  function disconnectPeer(id: string) {
    const peer = getPeer(id)
    if (peer.address) {
      options.events.emit('PEER_DISCONNECTED', { address: peer.address })
    }
  }

  return {
    disconnectPeer,
    removeAllBut(peerIds: string[]) {
      for (const [id] of peers) {
        if (!peerIds.includes(id)) {
          disconnectPeer(id)
        }
      }
    },
    identifyPeer(id: string, address: string) {
      const peer = getPeer(id)

      if (peer.address && peer.address !== address) {
        disconnectPeer(id)
        peer.position = undefined
        peer.profileAnnounce = undefined
        peer.profileResponse = undefined
        peer.address = undefined
      }

      if (!peer.address) {
        peer.address = address
        if (peer.position) {
          options.events.emit('position', { address, data: peer.position })
        }
        if (peer.profileResponse) {
          options.events.emit('profileResponse', { address, data: peer.profileResponse })
        }
        if (peer.profileAnnounce) {
          options.events.emit('profileMessage', { address, data: peer.profileAnnounce })
        }
      }
    },
    handleMessage<T extends keyof CommsEvents, X extends CommsEvents[T]>(message: T, packet: X) {
      if (message === 'PEER_DISCONNECTED') {
        const p = packet as PeerDisconnectedEvent
        disconnectPeer(p.address)
        return
      }

      if ('address' in packet) {
        const peer = getPeer(packet.address)

        if (peer.address) {
          options.events.emit(message, { ...packet, address: peer.address })
        }

        if ('data' in packet) {
          if (message === 'position') {
            const p = packet.data as rfc4.Position
            if (!peer.position || p.index >= peer.position.index) peer.position = p
          } else if (message === 'profileResponse') {
            const p = packet.data as rfc4.ProfileResponse
            if (!peer.profileResponse) peer.profileResponse = p
          } else if (message === 'profileMessage') {
            const p = packet.data as rfc4.AnnounceProfileVersion
            if (!peer.profileAnnounce || peer.profileAnnounce.profileVersion < p.profileVersion)
              peer.profileAnnounce = p
          }
        }
      }
    }
  }
}
