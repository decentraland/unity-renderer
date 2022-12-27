export type TTLFunction = (index: number, type: PeerMessageType) => number
export type OptimisticFunction = (index: number, type: PeerMessageType) => boolean

export type PeerMessageType = {
  /**
   * Time to Live of the messages of this particular type. How many hops will the message do before being discarded.
   * It can be set to a number, or to an interleaving (to have variable TTL depending on the index of the message).
   *
   * NOTE: Interleaving is not implemented yet.
   */
  ttl?: number | TTLFunction
  /**
   * If the time since received the last message of the same type (calculated using the message timestamp) is greater than this value,
   * then the message is discarded. Set to 0 to discard al messages older than the last one.
   */

  discardOlderThan?: number
  /**
   * Time to preserve the messages in the list of known messages, in order to avoid processing them multiple times.
   * If a message is received with a timestamp which indicates that it is older than this expiration time (calculated using the timestamp of the known peer),
   * then the message is discarded directly.
   */

  expirationTime?: number

  /**
   * If a packet is optimistic, then the members of the received by list are set before sending the packet.
   * An optimistic package should have a lot less duplicates, but it could be less reliable if the peer connection is not healthy
   */
  optimistic: boolean | OptimisticFunction
  /**
   * The name of the type is used as a key for some data structures, so it should be unique
   */
  name: string
}

export const PeerMessageTypes = {
  reliable: (name: string) => ({
    name,
    ttl: 10,
    expirationTime: 20 * 1000,
    optimistic: false
  }),
  unreliable: (name: string) => ({
    name,
    ttl: 10,
    discardOlderThan: 0,
    expirationTime: 2000,
    optimistic: true
  })
}

export const PingMessageType: PeerMessageType = {
  name: 'ping',
  ttl: 10,
  optimistic: true,
  discardOlderThan: 0
}

export const PongMessageType: PeerMessageType = {
  ...PingMessageType,
  name: 'pong'
}

export const SuspendRelayType: PeerMessageType = {
  name: 'suspendRelay',
  ttl: 1,
  optimistic: true,
  discardOlderThan: 0
}
