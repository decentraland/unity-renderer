import { IRealmAdapter } from 'shared/realm/types'
import { listenPeerMessage } from '../../logic/subscription-adapter'
import { ILogger } from 'shared/logger'
import { P2PLogConfig } from './types'
import { PeerTopicSubscriptionResultElem } from '@dcl/protocol/out-ts/decentraland/bff/topics_service.gen'

export const defaultIceServers = [
  { urls: 'stun:stun.l.google.com:19302' },
  {
    urls: 'turn:coturn-raw.decentraland.services:3478',
    credential: 'passworddcl',
    username: 'usernamedcl'
  }
]

type Config = {
  logger: ILogger
  packetHandler: (data: Uint8Array, peerId: string) => void
  shouldAcceptOffer(peerId: string): boolean
  logConfig: P2PLogConfig
}

type Connection = {
  instance: RTCPeerConnection
  createTimestamp: number
  dc?: RTCDataChannel
}

const PEER_CONNECT_TIMEOUT = 3500

export class Mesh {
  private disposed = false
  private logger: ILogger
  private packetHandler: (data: Uint8Array, peerId: string) => void
  private shouldAcceptOffer: (peerId: string) => boolean
  private initiatedConnections = new Map<string, Connection>()
  private receivedConnections = new Map<string, Connection>()
  private logConfig: P2PLogConfig
  private encoder = new TextEncoder()
  private decoder = new TextDecoder()

  private listeners: { close(): void }[] = []

  constructor(
    private realmAdapter: IRealmAdapter,
    private peerId: string,
    { logger, packetHandler, shouldAcceptOffer, logConfig }: Config
  ) {
    this.logger = logger
    this.packetHandler = packetHandler
    this.shouldAcceptOffer = shouldAcceptOffer
    this.logConfig = logConfig

    this.listeners.push(
      listenPeerMessage(realmAdapter.services.comms, `${this.peerId}.candidate`, this.onCandidateMessage.bind(this)),
      listenPeerMessage(realmAdapter.services.comms, `${this.peerId}.offer`, this.onOfferMessage.bind(this)),
      listenPeerMessage(realmAdapter.services.comms, `${this.peerId}.answer`, this.onAnswerListener.bind(this))
    )
  }

  public async connectTo(peerId: string, reason: string): Promise<void> {
    if (this.initiatedConnections.has(peerId) || this.receivedConnections.has(peerId)) {
      return
    }

    this.debugWebRtc(`Connecting to ${peerId}. ${reason}`)

    const instance = this.createConnection(peerId, this.peerId)
    const conn: Connection = { instance, createTimestamp: Date.now() }
    instance.addEventListener('connectionstatechange', (_) => {
      switch (instance.connectionState) {
        case 'new':
          conn.createTimestamp = Date.now()
          break
        case 'closed':
          this.receivedConnections.delete(peerId)
          break
        case 'failed':
          this.receivedConnections.delete(peerId)
          break
        default:
          break
      }
    })

    this.debugWebRtc(`Opening dc for ${peerId}`)
    const dc = instance.createDataChannel('data')
    dc.binaryType = 'arraybuffer'
    dc.addEventListener('open', () => {
      conn.dc = dc
    })
    dc.addEventListener('message', async (event) => {
      const data = new Uint8Array(event.data)

      this.packetHandler(data, peerId)
    })

    const offer = await instance.createOffer({
      offerToReceiveAudio: true,
      offerToReceiveVideo: false
    })
    await instance.setLocalDescription(offer)
    this.debugWebRtc(`Set local description for ${peerId}`)
    this.debugWebRtc(`Sending offer to ${peerId}`)
    await this.realmAdapter.services.comms.publishToTopic({
      topic: `${peerId}.offer`,
      payload: this.encoder.encode(JSON.stringify(offer))
    })

    this.initiatedConnections.set(peerId, conn)
  }

  public connectedCount(): number {
    let count = 0
    this.initiatedConnections.forEach(({ instance }: Connection) => {
      if (instance.connectionState === 'connected') {
        count++
      }
    })
    this.receivedConnections.forEach(({ instance }: Connection) => {
      if (instance.connectionState === 'connected') {
        count++
      }
    })
    return count
  }

  public connectionsCount(): number {
    return this.initiatedConnections.size + this.receivedConnections.size
  }

  public disconnectFrom(peerId: string): void {
    this.debugWebRtc(`Disconnecting from ${peerId}`)
    let conn = this.initiatedConnections.get(peerId)
    if (conn) {
      conn.instance.close()
      this.initiatedConnections.delete(peerId)
    }

    conn = this.receivedConnections.get(peerId)
    if (conn) {
      conn.instance.close()
      this.receivedConnections.delete(peerId)
    }
  }

  public hasConnectionsFor(peerId: string): boolean {
    return !!(this.initiatedConnections.get(peerId) || this.receivedConnections.get(peerId))
  }

  public isConnectedTo(peerId: string): boolean {
    let conn = this.initiatedConnections.get(peerId)
    if (conn && conn.instance.connectionState === 'connected') {
      return true
    }
    conn = this.receivedConnections.get(peerId)
    if (conn && conn.instance.connectionState === 'connected') {
      return true
    }

    return false
  }

  public connectedPeerIds(): string[] {
    const peerIds = new Set(this.initiatedConnections.keys())
    this.receivedConnections.forEach((_, peerId) => peerIds.add(peerId))
    return Array.from(peerIds)
  }

  public fullyConnectedPeerIds(): string[] {
    const peers: string[] = []

    this.initiatedConnections.forEach(({ instance }: Connection, peerId: string) => {
      if (instance.connectionState === 'connected') {
        peers.push(peerId)
      }
    })

    this.receivedConnections.forEach(({ instance }: Connection, peerId: string) => {
      if (instance.connectionState === 'connected') {
        peers.push(peerId)
      }
    })

    return peers
  }

  public checkConnectionsSanity(): void {
    this.initiatedConnections.forEach((conn: Connection, peerId: string) => {
      const state = conn.instance.connectionState
      if (state !== 'connected' && Date.now() - conn.createTimestamp > PEER_CONNECT_TIMEOUT) {
        this.debugWebRtc(`The connection ->${peerId} is not in a sane state ${state}. Discarding it.`)
        conn.instance.close()
        this.initiatedConnections.delete(peerId)
      }
    })
    this.receivedConnections.forEach((conn: Connection, peerId: string) => {
      const state = conn.instance.connectionState
      if (state !== 'connected' && Date.now() - conn.createTimestamp > PEER_CONNECT_TIMEOUT) {
        this.debugWebRtc(`The connection <-${peerId} is not in a sane state ${state}. Discarding it.`)
        conn.instance.close()
        this.receivedConnections.delete(peerId)
      }
    })
  }

  public sendPacketToPeer(peerId: string, data: Uint8Array): boolean {
    let conn = this.initiatedConnections.get(peerId)
    if (conn && conn.dc && conn.dc.readyState === 'open') {
      conn.dc.send(data)
      return true
    }
    conn = this.receivedConnections.get(peerId)
    if (conn && conn.dc && conn.dc.readyState === 'open') {
      conn.dc.send(data)
      return true
    }
    return false
  }

  async dispose(): Promise<void> {
    if (this.disposed) return
    this.disposed = true

    for (const listener of this.listeners) {
      await listener.close()
    }

    this.initiatedConnections.forEach(({ instance }: Connection) => {
      instance.close()
    })
    this.receivedConnections.forEach(({ instance }: Connection) => {
      instance.close()
    })

    this.initiatedConnections.clear()
    this.receivedConnections.clear()
  }

  private createConnection(peerId: string, initiator: string) {
    const instance = new RTCPeerConnection({
      iceServers: defaultIceServers
    })

    instance.addEventListener('icecandidate', async (event) => {
      if (event.candidate) {
        try {
          const msg = { candidate: event.candidate, initiator }
          await this.realmAdapter.services.comms.publishToTopic({
            topic: `${peerId}.candidate`,
            payload: this.encoder.encode(JSON.stringify(msg))
          })
        } catch (err: any) {
          this.logger.error(`cannot publish ice candidate: ${err.toString()}`)
        }
      }
    })

    instance.addEventListener('iceconnectionstatechange', () => {
      this.debugWebRtc(`Connection with ${peerId}, ice status changed: ${instance.iceConnectionState}`)
    })
    return instance
  }

  private async onCandidateMessage(message: PeerTopicSubscriptionResultElem) {
    if (this.disposed) return

    if (this.logConfig.debugIceCandidates) {
      this.logger.info(`ICE candidate received from ${message.sender}`)
    }

    const { candidate, initiator } = JSON.parse(this.decoder.decode(message.payload))

    try {
      const conn = (initiator === this.peerId ? this.initiatedConnections : this.receivedConnections).get(
        message.sender
      )
      if (!conn) {
        if (this.logConfig.debugWebRtcEnabled) {
          this.logger.info(
            `ICE candidate received from ${message.sender}, but there is no connection. (initiator: ${
              initiator === this.peerId ? 'us' : 'them'
            })`
          )
        }
        return
      }

      const state = conn.instance.connectionState
      if (state !== 'connecting' && state !== 'new' && state !== 'connected') {
        this.debugWebRtc(`No setting ice candidate for ${message.sender}, connection is in state ${state}`)
        return
      }

      await conn.instance.addIceCandidate(candidate)
    } catch (e: any) {
      this.logger.error(
        `Failed to add ice candidate: ${e.toString()} (initiator: ${initiator === this.peerId ? 'us' : 'them'})`
      )
    }
  }

  private async onOfferMessage(message: PeerTopicSubscriptionResultElem) {
    if (this.disposed) return
    const peerId = message.sender
    if (!this.shouldAcceptOffer(peerId)) {
      return
    }

    this.debugWebRtc(`Got offer message from ${peerId}`)

    const existentConnection = this.initiatedConnections.get(peerId)
    if (existentConnection) {
      if (this.peerId < peerId) {
        this.debugWebRtc(`Both peers try to establish connection with each other ${peerId}, closing old connection`)
        existentConnection.instance.close()
        this.initiatedConnections.delete(peerId)
        return
      }
      this.debugWebRtc(`Both peers try to establish connection with each other ${peerId}, keeping this offer`)
    }

    const offer = JSON.parse(this.decoder.decode(message.payload))
    const instance = this.createConnection(peerId, peerId)
    const conn: Connection = { instance, createTimestamp: Date.now() }
    this.receivedConnections.set(peerId, conn)

    instance.addEventListener('connectionstatechange', () => {
      switch (instance.connectionState) {
        case 'new':
          conn.createTimestamp = Date.now()
          break
        case 'closed':
          this.receivedConnections.delete(peerId)
          break
        case 'failed':
          this.receivedConnections.delete(peerId)
          break
        default:
          break
      }
    })
    instance.addEventListener('datachannel', (event) => {
      this.debugWebRtc(`Got data channel from ${peerId}`)
      const dc = event.channel
      dc.binaryType = 'arraybuffer'
      dc.addEventListener('open', () => {
        conn.dc = dc
      })

      dc.addEventListener('message', (event) => {
        const data = new Uint8Array(event.data)
        this.packetHandler(data, peerId)
      })
    })

    try {
      this.debugWebRtc(`Setting remote description for ${peerId}`)
      await instance.setRemoteDescription(offer)

      this.debugWebRtc(`Creating answer for ${peerId}`)
      const answer = await instance.createAnswer()

      this.debugWebRtc(`Setting local description for ${peerId}`)
      await instance.setLocalDescription(answer)

      this.debugWebRtc(`Sending answer to ${peerId}`)
      await this.realmAdapter.services.comms.publishToTopic({
        topic: `${peerId}.answer`,
        payload: this.encoder.encode(JSON.stringify(answer))
      })
    } catch (e: any) {
      this.logger.error(`Failed to create answer: ${e.toString()}`)
    }
  }

  private async onAnswerListener(message: PeerTopicSubscriptionResultElem) {
    if (this.disposed) return
    this.debugWebRtc(`Got answer message from ${message.sender}`)
    const conn = this.initiatedConnections.get(message.sender)
    if (!conn) {
      return
    }

    const state = conn.instance.connectionState
    if (state !== 'connecting' && state !== 'new') {
      this.debugWebRtc(`No setting remote description for ${message.sender} connection is in state ${state}`)
      return
    }

    try {
      const answer = JSON.parse(this.decoder.decode(message.payload))
      this.debugWebRtc(`Setting remote description for ${message.sender}`)
      await conn.instance.setRemoteDescription(answer)
    } catch (e: any) {
      this.logger.error(`Failed to set remote description: ${e.toString()}`)
    }
  }

  private debugWebRtc(message: string) {
    if (this.logConfig.debugWebRtcEnabled) {
      this.logger.log(message)
    }
  }
}
