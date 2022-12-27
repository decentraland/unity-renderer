import { IFuture } from 'fp-future'
import { Position3D } from 'shared/comms/v3/types'

export type P2PLogConfig = {
  debugWebRtcEnabled: boolean
  debugUpdateNetwork: boolean
  debugIceCandidates: boolean
  debugMesh: boolean
}

export type ReceivedRelayData = {
  hops: number
  total: number
  discarded: number
}

export type PeerRelayData = {
  lastRelaySuspensionTimestamp?: number
  /**
   * This is data for relays received from this peer
   */
  receivedRelayData: Record<string, ReceivedRelayData>
  /**
   * This is suspension data for relays sent by this peer
   * Example: A requests suspension of P to B ==> B stores that suspension in ownSuspendedRelays in the connection data of their connection
   * key = source peer id. Value = suspension expiration
   * */
  ownSuspendedRelays: Record<string, number>

  /**
   * These are suspensions requested to this peer, tracked by the local peer
   * Example: A requests suspension of P to B ==> A stores that suspension in theirSuspendedRelays in the connection data of their connection
   * key = source peer id. Value = suspension expiration
   * */
  theirSuspendedRelays: Record<string, number>

  /**
   * Suspension requests to be sent to this peer by the local peer in the next window.
   * Is the list of the ids of the relayed peers for which to suspend relay
   */
  pendingSuspensionRequests: string[]
}

export type PingResult = {
  peerId: string
  latency: number
}

export type PacketSubtypeData = {
  lastTimestamp: number
  lastSequenceId: number
}

export type PeerRelay = { id: string; hops: number; timestamp: number }

export type KnownPeerData = {
  id: string
  lastUpdated?: number // Local timestamp used for registering if the peer is alive
  timestamp?: number // Their local timestamp used for handling packets
  subtypeData: Record<string, PacketSubtypeData>
  position?: Position3D
  latency?: number
  hops?: number
  reachableThrough: Record<string, PeerRelay>
}

export type MinPeerData = { id: string; position?: Position3D }

export type NetworkOperation = () => Promise<KnownPeerData[]>

export type ActivePing = {
  results: PingResult[]
  startTime?: number
  future: IFuture<PingResult[]>
}
