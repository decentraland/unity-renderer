import { parcelLimits, commConfigurations as config, ETHEREUM_NETWORK } from 'config'
import { Adapter } from './adapter'
import { getHeaders, UserData as EphemeralKey } from 'ephemeralkey'
import {
  MessageType,
  FlowStatus,
  ChatMessage,
  PositionMessage,
  ClientDisconnectedFromServerMessage,
  GenericMessage,
  ProfileMessage,
  FlowStatusMessage,
  PingMessage,
  WebRtcOfferMessage,
  WebRtcAnswerMessage,
  WebRtcIceCandidateMessage
} from '../../shared/comms/worldcomm_pb'
import { V2, CommunicationArea, decodeMessageHeader } from './utils'

import { log, error as logError } from 'engine/logger'
import { NetworkStats } from './debug'

const ONE_HOUR_MS = 60 * 60 * 1000

export enum SocketReadyState {
  CONNECTING,
  OPEN,
  CLOSING,
  CLOSED
}

type Timestamp = number
export type ServerId = number
type PeerAlias = number
type PeerId = string
export type Position = [number, number, number, number, number, number, number]

type PositionData = {
  alias: PeerAlias
  time: Timestamp
  position: Position
}

export type ClientPositionData = PositionData & {
  serverId: ServerId
}

type SendResult = {
  bytesPerMessage: number
  sent: number
}

type PeerDistance = {
  peerId: PeerId
  peerIndex: number
  squareDistance: number
}

type CurrentPosition = {
  position: Position
  parcel: V2
  commArea: CommunicationArea
  antennas: V2[]
}

// NOTE this represents the client interface with the worker, I'm going to abstract the fact
// that the worker cannot open a webrtc connection here
class ClientAPI {
  public webRtcConnection: ServerConnection | null = null
  public constructor(public context: Context, public connector: Adapter) {}

  infoCollected(info: CollectedInfo) {
    this.connector.notify('infoCollected', info)
  }

  onNewEphemeralKeyRequired() {
    const network = this.context.network
    this.connector.notify('onNewEphemeralKeyRequired', { network })
  }

  onPublicChatReceived(peerId: string, msgId: string, text: string) {
    this.connector.notify('onPublicChatReceived', {
      peerId,
      msgId,
      text
    })
  }

  openWebRtcConnection(connection: ServerConnection) {
    const serverId = connection.serverId
    this.webRtcConnection = connection
    this.connector.notify('openWebRtcRequest', { serverId })
  }

  offerReceived(connection: ServerConnection, sdp: string) {
    const serverId = connection.serverId
    this.connector.notify('offerReceived', { sdp, serverId })
  }

  answerReceived(connection: ServerConnection, sdp: string) {
    const serverId = connection.serverId
    this.connector.notify('answerReceived', { sdp, serverId })
  }

  iceCandidateReceived(connection: ServerConnection, sdp: string) {
    const serverId = connection.serverId
    this.connector.notify('iceCandidateReceived', { sdp, serverId })
  }

  onConnectionClosed(connection: ServerConnection) {
    const serverId = connection.serverId
    if (this.webRtcConnection && this.webRtcConnection.serverId === serverId) {
      this.connector.notify('onCloseWebRtcConnectionRequested', { serverId })
    }
  }

  onWebRtcConnectionClosed(serverId: ServerId) {
    if (this.webRtcConnection && this.webRtcConnection.serverId === serverId) {
      if (this.webRtcConnection.flowStatus === FlowStatus.OPEN_WEBRTC_PREFERRED) {
        this.webRtcConnection.flowStatus = FlowStatus.OPEN
      }
      this.webRtcConnection = null
    }
  }

  closeWebRtcConnection(connection: ServerConnection) {
    const serverId = connection.serverId
    if (this.webRtcConnection && this.webRtcConnection.serverId === serverId) {
      this.connector.notify('closeWebRtcConnection', { serverId })
    }
  }

  onOfferGenerated(serverId: ServerId, sdp: string) {
    const msg = new WebRtcOfferMessage()
    msg.setType(MessageType.WEBRTC_OFFER)
    msg.setTime(Date.now())
    msg.setSdp(sdp)
    this.sendControlMessage(serverId, msg)
  }

  onAnswerGenerated(serverId: ServerId, sdp: string) {
    const msg = new WebRtcAnswerMessage()
    msg.setType(MessageType.WEBRTC_ANSWER)
    msg.setTime(Date.now())
    msg.setSdp(sdp)
    this.sendControlMessage(serverId, msg)
  }

  onIceCandidate(serverId: ServerId, sdp: string) {
    const msg = new WebRtcIceCandidateMessage()
    msg.setType(MessageType.WEBRTC_ICE_CANDIDATE)
    msg.setTime(Date.now())
    msg.setSdp(sdp)
    this.sendControlMessage(serverId, msg)
  }

  sendControlMessage(serverId: ServerId, msg: any) {
    if (!this.webRtcConnection || this.webRtcConnection.serverId !== serverId) {
      log('control message over a non existent webrtc connection, ignoring')
      return
    }

    const connection = this.webRtcConnection

    const ws = connection.ws
    if (!ws || ws.readyState !== SocketReadyState.OPEN) {
      throw new Error('try to send answer to a non ready ws')
    }

    const bytes = msg.serializeBinary()

    if (this.context.stats) {
      this.context.stats.webRtcSession.incrementSent(1, bytes.length)
    }

    ws.send(bytes)
  }
}

export function makeCurrentPosition(position: Position, maybeParcel?: V2): CurrentPosition {
  const parcel = maybeParcel ? maybeParcel : position2parcel(position)

  const area = new CommunicationArea(parcel, config.commRadius)
  const antennaRadius = config.antennaRadius
  const antennas = [
    new V2(
      Math.max(parcelLimits.minParcelX, parcel.x - antennaRadius),
      Math.max(parcelLimits.minParcelZ, parcel.z - antennaRadius)
    ),
    new V2(
      Math.max(parcelLimits.minParcelX, parcel.x - antennaRadius),
      Math.min(parcelLimits.maxParcelZ, parcel.z + antennaRadius)
    ),
    new V2(
      Math.min(parcelLimits.maxParcelX, parcel.x + antennaRadius),
      Math.max(parcelLimits.minParcelZ, parcel.z - antennaRadius)
    ),
    new V2(
      Math.min(parcelLimits.maxParcelX, parcel.x + antennaRadius),
      Math.min(parcelLimits.maxParcelZ, parcel.z + antennaRadius)
    )
  ]

  return {
    position,
    parcel,
    commArea: area,
    antennas
  }
}

function position2parcel(p: Position): V2 {
  const parcelSize = parcelLimits.parcelSize
  return new V2(Math.trunc(p[0] / parcelSize), Math.trunc(p[2] / parcelSize))
}

function position2v2(p: Position): V2 {
  return new V2(p[0], p[2])
}

export type UserProfile = {
  displayName: string
  publicKey: string
  avatarType: string
}

export class PeerTrackingInfo {
  public position: Position | null = null
  public profile: UserProfile | null = null
  public lastPositionUpdate: Timestamp = 0
  public lastProfileUpdate: Timestamp = 0
  public servers = new Map<ServerId, Timestamp>()
  public receivedPublicChatMessages = new Set<string>()
}

export class ServerConnection {
  public locations: V2[]
  public ws: WebSocket | null
  public flowStatus: FlowStatus = FlowStatus.CLOSE
  public pingInterval: any = null
  public ping: number = -1
  public alias2id = new Map<PeerAlias, PeerId>()

  constructor(public serverId: ServerId, public url: string, p: V2) {
    this.locations = [p]
    this.ws = null
  }

  public containsLocation(p: V2): boolean {
    for (let location of this.locations) {
      if (location.x === p.x && location.z === p.z) {
        return true
      }
    }

    return false
  }

  public containsAnyLocations(ps: V2[]): boolean {
    for (let location of this.locations) {
      for (let p of ps) {
        if (p.x === location.x && p.z === location.z) {
          return true
        }
      }
    }

    return false
  }

  public removeLocation(p: V2) {
    for (let i = 0; i < this.locations.length; ++i) {
      const location = this.locations[i]
      if (location.x === p.x && location.z === p.z) {
        this.locations.splice(i, 1)
        return
      }
    }
  }

  public addLocation(p: V2) {
    this.locations.push(p)
  }

  public minSquareDistance(origin: V2): number {
    if (this.locations.length === 0) {
      throw new Error('try to get distance from an empty connection')
    }

    const distTo = (p: V2): number => {
      return new V2(p.x - origin.x, p.z - origin.z).squareLenght()
    }

    let min = distTo(this.locations[0])
    for (let i = 1; i < this.locations.length; ++i) {
      let d = distTo(this.locations[i])
      if (d < min) {
        min = d
      }
    }

    return min
  }
}

export type CommParcelDataPacked = {
  x: number
  y: number
  commServerUrl: string | null
}

export type CommParcelData = {
  commServerUrl: string | null
}

export class Context {
  public knownServers = new Map<string, ServerId>()

  public commData = new Map<string, CommParcelData>()

  public peerId: PeerId

  public connections: ServerConnection[] = []
  public negotiatingConnection = new Set<WebSocket>()

  public peerData = new Map<PeerId, PeerTrackingInfo>()
  public userProfile: UserProfile

  public currentPosition: CurrentPosition | null = null

  public stats: NetworkStats | null = null

  public clientApi: ClientAPI

  public connectionsInterval: any
  public profileInterval: any

  public network: ETHEREUM_NETWORK | null
  public ephemeralKey: EphemeralKey | null

  constructor(
    connector: Adapter,
    peerId: PeerId,
    userProfile: UserProfile,
    network?: ETHEREUM_NETWORK,
    ephemeralKey?: EphemeralKey
  ) {
    this.clientApi = new ClientAPI(this, connector)
    this.peerId = peerId
    this.userProfile = userProfile
    this.network = network || null
    this.ephemeralKey = ephemeralKey || null
  }
}

export function init(
  peerId: PeerId,
  connector: Adapter,
  userProfile: UserProfile,
  network?: ETHEREUM_NETWORK,
  ephemeralKey?: EphemeralKey
): Context {
  const context = new Context(connector, peerId, userProfile, network, ephemeralKey)

  if (config.debug) {
    context.stats = new NetworkStats(context)
  }

  context.connectionsInterval = setInterval(() => processConnections(context), config.processConnectionsIntervalMs)
  context.profileInterval = setInterval(() => sendProfileMessage(context), 1000)

  return context
}

export function close(context: Context) {
  if (context.connectionsInterval) {
    clearInterval(context.connectionsInterval)
  }
  if (context.profileInterval) {
    clearInterval(context.profileInterval)
  }

  if (context.stats) {
    context.stats.close()
  }
  for (let connection of context.connections) {
    const ws = connection.ws
    if (ws) {
      closeWs(ws)
    }
  }
}

type PeerInfo = {
  peerId: string
  position: Position | null
  profile: UserProfile | null
}

type CollectedInfo = {
  peers: PeerInfo[]
}

export function onInfoRequested(context: Context, data: ClientPositionData[]) {
  if (context.stats) {
    context.stats.positionWebRtcPackages.incrementRecv(data.length)
  }

  if (context.clientApi.webRtcConnection) {
    const connection = context.clientApi.webRtcConnection
    for (let msg of data) {
      if (msg.serverId === connection.serverId) {
        processPositionMessage(context, connection, msg)
      }
    }
  }

  const info = collectInfo(context)
  context.clientApi.infoCollected(info)
}

export function onCommDataLoaded(context: Context, data: CommParcelDataPacked[]) {
  for (let parcelData of data) {
    const { x, y } = parcelData
    const key = `${x},${y}`

    const commServerUrl = parcelData.commServerUrl || config.defaultCommServerUrl

    const cacheData = context.commData.get(key)
    const cachedServerUrl = (cacheData && cacheData.commServerUrl) || config.defaultCommServerUrl

    if (cachedServerUrl !== commServerUrl) {
      // IMPORTANT: this is pretty tricky because we connect to a default comm server url
      // if no parcel data is available, and then we replace one by one if needed when we adquire the data.
      // This could be a problem for example if we need to connect to n parcels for which we have no info,
      // and then they all point to differents servers. That been said, the most extreme cases for
      // this problem are once you connect for first time and when you teleport, because
      // nothing will be preloaded at those points.
      // However, since I image the problem of lacking of a prefetch may manifest itself in
      // differents parts of the game, not just comms, if this ever becomes an issue, we could solve
      // it once for all by prefeching before teleporting.
      const p = new V2(x, y)

      const connection = findConnectionByUrl(context, cachedServerUrl)
      if (connection) {
        log(`a new comm server url has been loaded for parcel ${key}, replacing old one`)
        connection.removeLocation(p)
        ensureConnection(context, commServerUrl, p)
      }
    }
    context.commData.set(key, { commServerUrl })
  }
}

export function collectInfo(context: Context): CollectedInfo {
  const now = Date.now()
  if (context.stats) {
    context.stats.collectInfoDuration.start()
  }
  const peers = [] as PeerInfo[]
  const info = { peers }
  if (!context.currentPosition) {
    return info
  }

  const { position, commArea } = context.currentPosition
  const p = position2v2(position)
  let visiblePeers: PeerDistance[] = []
  for (let [peerId, trackingInfo] of context.peerData) {
    const peerIndex = peers.length
    const peerInfo = { peerId, position: null, profile: null } as PeerInfo
    peers.push(peerInfo)

    if (trackingInfo.servers.size === 0) {
      const lastUpdate = Math.max(trackingInfo.lastPositionUpdate, trackingInfo.lastProfileUpdate)

      if (now - lastUpdate >= config.peerTtlMs) {
        context.peerData.delete(peerId)
      }

      continue
    }

    if (!trackingInfo.position || !trackingInfo.profile) {
      continue
    }

    if (!commArea.contains(position2parcel(trackingInfo.position))) {
      continue
    }

    const peerPosition = position2v2(trackingInfo.position)
    visiblePeers.push({
      peerId,
      squareDistance: p.minus(peerPosition).squareLenght(),
      peerIndex
    })

    peerInfo.position = trackingInfo.position
    peerInfo.profile = trackingInfo.profile
  }

  if (visiblePeers.length > config.maxVisiblePeers) {
    const sortedBySqDistanceVisiblePeers = visiblePeers.sort((p1, p2) => p1.squareDistance - p2.squareDistance)
    for (let i = config.maxVisiblePeers; i < visiblePeers.length; ++i) {
      const peer = sortedBySqDistanceVisiblePeers[i]
      const peerIndex = peer.peerIndex
      // NOTE: this peer is far from the user, let's hide it
      peers[peerIndex].position = null
      peers[peerIndex].profile = null
    }
  }

  if (context.stats) {
    context.stats.collectInfoDuration.stop()
  }
  return info
}

export function sendPublicChatMessage(context: Context, messageId: string, text: string, time?: number) {
  const { currentPosition, stats } = context
  if (currentPosition) {
    const { x, z } = currentPosition.parcel
    const m = new ChatMessage()
    m.setType(MessageType.CHAT)
    m.setTime(time ? time : Date.now())
    m.setMessageId(messageId)
    m.setPositionX(x)
    m.setPositionZ(z)
    m.setText(text)

    const result = sendMessage(context, m)
    if (stats) {
      stats.chat.incrementSent(result.sent, result.sent * result.bytesPerMessage)
    }
  }
}

export function onProfileUpdate(context: Context, profile: UserProfile) {
  context.userProfile = profile
}

export function onPositionUpdate(context: Context, position: Position): void {
  const newParcel = position2parcel(position)

  let { currentPosition } = context
  if (!currentPosition || currentPosition.parcel.x !== newParcel.x || currentPosition.parcel.z !== newParcel.z) {
    const newPosition = makeCurrentPosition(position, newParcel)
    const area = newPosition.commArea

    for (let connection of context.connections) {
      for (let p of connection.locations) {
        if (!area.contains(p)) {
          // NOTE: we don't actually disconnect here since the next loop may add the domain again,
          // processConnections interval will take care of actually closing connections
          connection.removeLocation(p)
        }
      }
    }

    for (let x = area.vMin.x; x <= area.vMax.x; ++x) {
      for (let z = area.vMin.z; z <= area.vMax.z; ++z) {
        const p = new V2(x, z)

        const commServerUrl = findParcelCommServerUrl(context, p)
        ensureConnection(context, commServerUrl, p)
      }
    }

    context.currentPosition = newPosition
  } else {
    currentPosition.position = position
  }

  sendPositionMessage(context, position)
}

export function setEphemeralKey(context: Context, key: EphemeralKey) {
  context.ephemeralKey = key
}

export function processConnections(context: Context) {
  const { currentPosition } = context
  if (!currentPosition) {
    return
  }

  const { parcel, antennas } = currentPosition

  if (context.stats) {
    context.stats.processConnectionDuration.start()
  }
  const untrackedServers = new Set<ServerId>()

  // NOTE: first, we disconnect the ones we don't need anymore
  for (let i = 0; i < context.connections.length; ++i) {
    const connection = context.connections[i]
    if (connection.locations.length === 0) {
      context.connections.splice(i, 1)
      --i

      untrackedServers.add(connection.serverId)

      if (connection.ws) {
        context.negotiatingConnection.delete(connection.ws)
        closeWs(connection.ws)
      }

      context.clientApi.closeWebRtcConnection(connection)

      if (connection.pingInterval) {
        clearInterval(connection.pingInterval)
      }
    } else if (connection.ws && connection.ws.readyState === SocketReadyState.CLOSED) {
      untrackedServers.add(connection.serverId)
    }
  }

  // NOTE: then, ensure the list on negotiatingConnection is correct
  for (let ws of context.negotiatingConnection) {
    if (ws.readyState !== SocketReadyState.CLOSING) {
      context.negotiatingConnection.delete(ws)
    }
  }

  context.connections.sort((a: ServerConnection, b: ServerConnection) => {
    return a.minSquareDistance(parcel) - b.minSquareDistance(parcel)
  })

  // NOTE: then, we open new connections up to config.maxConcurrentConnectionRequests
  for (let connection of context.connections) {
    if (context.negotiatingConnection.size >= config.maxConcurrentConnectionRequests) {
      return
    }
    if (!connection.ws) {
      connection.ws = connectWs(context, connection.url, connection)
      context.negotiatingConnection.add(connection.ws)
    } else if (connection.ws.readyState === SocketReadyState.CLOSED) {
      connection.ws.onmessage = null
      connection.ws.onerror = null
      connection.ws.onclose = null
      connection.flowStatus = FlowStatus.CLOSE
      connection.ws = connectWs(context, connection.url, connection)
      context.negotiatingConnection.add(connection.ws)
    }
  }

  // NOTE: then, we ensure the proper flow status is set
  const infoPoints = antennas.concat(parcel)
  let infoPointsReady = 0
  for (let connection of context.connections) {
    if (connection.ws && connection.ws.readyState === SocketReadyState.OPEN) {
      if (connection.containsLocation(parcel)) {
        if (openParcelFlow(context, connection)) {
          infoPointsReady++
        }
      } else if (connection.containsAnyLocations(antennas)) {
        if (openAntennaFlow(context, connection)) {
          infoPointsReady++
        }
      }
    }

    if (infoPointsReady === infoPoints.length) {
      // NOTE since at least one connection in the recv area is ready, we can
      // close the flow in the other ones
      for (let connection of context.connections) {
        if (
          connection.ws &&
          connection.ws.readyState === SocketReadyState.OPEN &&
          !connection.containsAnyLocations(antennas.concat(parcel)) &&
          connection.flowStatus === FlowStatus.OPEN
        ) {
          untrackedServers.add(connection.serverId)
          closeFlow(context, connection)
        }
      }
    }
  }

  // NOTE finally let's remove the server tracking for servers that had been
  // disconnected or we are no longer receiving information from
  if (untrackedServers.size > 0) {
    for (let trackingInfo of context.peerData.values()) {
      for (let serverId of untrackedServers) {
        trackingInfo.servers.delete(serverId)
      }
    }
  }

  if (context.stats) {
    if (context.stats) {
      context.stats.processConnectionDuration.stop()
    }
  }
}

function connectWs(context: Context, url: string, connection: ServerConnection) {
  let wsUrl = url
  if (context.ephemeralKey) {
    const now = Date.now()

    const aboutToExpire = now >= context.ephemeralKey.expiresAt - ONE_HOUR_MS
    if (aboutToExpire) {
      context.clientApi.onNewEphemeralKeyRequired()
    }

    const headers = getHeaders(context.ephemeralKey, {
      method: 'GET',
      url: url,
      timestamp: now,
      body: Buffer.alloc(0)
    })

    const qs = new URLSearchParams(headers).toString()
    wsUrl = `${url}?${qs}`
  }

  const serverId = connection.serverId
  const ws = new WebSocket(wsUrl)
  ws.binaryType = 'arraybuffer'
  ws.onopen = () => {
    context.negotiatingConnection.delete(ws)
  }
  ws.onmessage = event => {
    const data = event.data
    const msg = new Uint8Array(data)
    const msgSize = msg.length
    const [msgType, msgTimestamp] = decodeMessageHeader(msg)

    switch (msgType) {
      case MessageType.UNKNOWN_MESSAGE_TYPE: {
        if (context.stats) {
          context.stats.others.incrementRecv(msgSize)
        }
        log('unsopported message')
        break
      }
      case MessageType.CHAT: {
        if (context.stats) {
          context.stats.chat.incrementRecv(msgSize)
        }
        let message
        try {
          message = ChatMessage.deserializeBinary(msg)
        } catch (e) {
          logError('cannot deserialize chat message', msg)
          break
        }

        const msgId = message.getMessageId()
        const p = new V2(message.getPositionX(), message.getPositionZ())
        const alias = message.getAlias()
        const peerId = connection.alias2id.get(alias)

        const currentPosition = context.currentPosition
        if (peerId && currentPosition && currentPosition.commArea.contains(p)) {
          const peerTrackingInfo = ensurePeerTrackingInfo(context, peerId)
          if (!peerTrackingInfo.receivedPublicChatMessages.has(msgId)) {
            const text = message.getText()
            peerTrackingInfo.receivedPublicChatMessages.add(msgId)

            context.clientApi.onPublicChatReceived(peerId, msgId, text)
          }
        }

        break
      }
      case MessageType.POSITION: {
        if (context.stats) {
          context.stats.position.incrementRecv(msgSize)
        }
        let message
        try {
          message = PositionMessage.deserializeBinary(msg)
        } catch (e) {
          logError('cannot deserialize position message', e, msg)
          break
        }

        const alias = message.getAlias()

        const parcelSize = parcelLimits.parcelSize
        const position = [
          message.getPositionX() * parcelSize,
          message.getPositionY(),
          message.getPositionZ() * parcelSize,
          message.getRotationX(),
          message.getRotationY(),
          message.getRotationZ(),
          message.getRotationW()
        ] as Position

        processPositionMessage(context, connection, { time: msgTimestamp, position, alias })
        break
      }
      case MessageType.PROFILE: {
        if (context.stats) {
          context.stats.profile.incrementRecv(msgSize)
        }
        let message
        try {
          message = ProfileMessage.deserializeBinary(msg)
        } catch (e) {
          logError('cannot deserialize position message', e, msg)
          break
        }

        const alias = message.getAlias()
        const peerId = message.getPeerId()
        connection.alias2id.set(alias, peerId)

        const peerTrackingInfo = ensurePeerTrackingInfo(context, peerId)
        const serversTrackingInfo = peerTrackingInfo.servers

        const trackedTs = serversTrackingInfo.get(serverId)
        if (!trackedTs || msgTimestamp > trackedTs) {
          serversTrackingInfo.set(serverId, msgTimestamp)
        }

        if (msgTimestamp > peerTrackingInfo.lastProfileUpdate) {
          const publicKey = message.getPublicKey()
          const avatarType = message.getAvatarType()
          const displayName = message.getDisplayName()

          peerTrackingInfo.profile = {
            displayName,
            publicKey,
            avatarType
          }

          peerTrackingInfo.lastProfileUpdate = msgTimestamp
        }
        break
      }
      case MessageType.CLIENT_DISCONNECTED_FROM_SERVER: {
        let message
        try {
          message = ClientDisconnectedFromServerMessage.deserializeBinary(msg)
        } catch (e) {
          logError('cannot deserialize client disconnected message', e, msg)
          break
        }

        const alias = message.getAlias()
        const peerId = connection.alias2id.get(alias)
        if (peerId) {
          const peerTrackingInfo = context.peerData.get(peerId)
          if (peerTrackingInfo) {
            peerTrackingInfo.servers.delete(serverId)
          }
        }

        if (context.stats) {
          context.stats.others.incrementRecv(msgSize)
        }
        break
      }
      case MessageType.PING: {
        let message
        try {
          message = PingMessage.deserializeBinary(msg)
        } catch (e) {
          logError('cannot deserialize ping message', e, msg)
          break
        }

        if (context.stats) {
          context.stats.ping.incrementRecv(msgSize)
        }

        connection.ping = Date.now() - message.getTime()

        break
      }
      case MessageType.CLOCK_SKEW_DETECTED: {
        if (context.stats) {
          context.stats.others.incrementRecv(msgSize)
        }
        break
      }
      case MessageType.WEBRTC_ICE_CANDIDATE: {
        if (context.stats) {
          context.stats.webRtcSession.incrementRecv(msgSize)
        }

        let message
        try {
          message = WebRtcIceCandidateMessage.deserializeBinary(msg)
        } catch (e) {
          logError('cannot deserialize webrtc ice candidate message', e, msg)
          break
        }
        const sdp = message.getSdp()
        context.clientApi.iceCandidateReceived(connection, sdp)
        break
      }
      case MessageType.WEBRTC_SUPPORTED: {
        if (context.stats) {
          context.stats.webRtcSession.incrementRecv(msgSize)
        }

        context.clientApi.openWebRtcConnection(connection)
        break
      }
      case MessageType.WEBRTC_OFFER: {
        if (context.stats) {
          context.stats.webRtcSession.incrementRecv(msgSize)
        }

        let message
        try {
          message = WebRtcOfferMessage.deserializeBinary(msg)
        } catch (e) {
          logError('cannot deserialize webrtc offer message', e, msg)
          break
        }
        const sdp = message.getSdp()
        context.clientApi.offerReceived(connection, sdp)

        break
      }
      case MessageType.WEBRTC_ANSWER: {
        if (context.stats) {
          context.stats.webRtcSession.incrementRecv(msgSize)
        }

        let message
        try {
          message = WebRtcOfferMessage.deserializeBinary(msg)
        } catch (e) {
          logError('cannot deserialize webrtc answer message', e, msg)
          break
        }
        const sdp = message.getSdp()
        context.clientApi.answerReceived(connection, sdp)

        break
      }
      case MessageType.WEBRTC_ICE_CANDIDATE: {
        if (context.stats) {
          context.stats.webRtcSession.incrementRecv(msgSize)
        }

        let message
        try {
          message = WebRtcIceCandidateMessage.deserializeBinary(msg)
        } catch (e) {
          logError('cannot deserialize webrtc ice candidate message', e, msg)
          break
        }
        const sdp = message.getSdp()
        context.clientApi.iceCandidateReceived(connection, sdp)

        break
      }
      default: {
        if (context.stats) {
          context.stats.others.incrementRecv(msgSize)
        }
        log('ignoring message with type', msgType)
        break
      }
    }
  }

  ws.onerror = event => {
    logError('socket error', event)
  }

  return ws
}

function ensurePeerTrackingInfo(context: Context, peerId: string): PeerTrackingInfo {
  let peerTrackingInfo = context.peerData.get(peerId)

  if (!peerTrackingInfo) {
    peerTrackingInfo = new PeerTrackingInfo()
    context.peerData.set(peerId, peerTrackingInfo)
  }
  return peerTrackingInfo
}

function processPositionMessage(context: Context, connection: ServerConnection, data: PositionData) {
  const msgTimestamp = data.time
  const p = data.position
  const alias = data.alias

  const serverId = connection.serverId

  const peerId = connection.alias2id.get(alias)

  if (peerId) {
    const peerTrackingInfo = ensurePeerTrackingInfo(context, peerId)
    const serversTrackingInfo = peerTrackingInfo.servers

    const trackedTs = serversTrackingInfo.get(serverId)
    if (!trackedTs || msgTimestamp > trackedTs) {
      serversTrackingInfo.set(serverId, msgTimestamp)
    }

    if (msgTimestamp > peerTrackingInfo.lastPositionUpdate) {
      peerTrackingInfo.position = p
      peerTrackingInfo.lastPositionUpdate = msgTimestamp
    }
  }
}

function sendProfileMessage(context: Context, time?: number) {
  const userProfile = context.userProfile
  if (context.currentPosition) {
    const { x, z } = context.currentPosition.parcel
    const m = new ProfileMessage()
    m.setType(MessageType.PROFILE)
    m.setTime(time ? time : Date.now())
    m.setPeerId(context.peerId)
    m.setPositionX(x)
    m.setPositionZ(z)
    m.setAvatarType(userProfile.avatarType)
    m.setDisplayName(userProfile.displayName)
    m.setPublicKey(userProfile.publicKey)
    const result = sendMessage(context, m)
    if (context.stats) {
      context.stats.profile.incrementSent(result.sent, result.sent * result.bytesPerMessage)
    }
  }
}

function sendPositionMessage(context: Context, p: Position, time?: number) {
  const parcelSize = parcelLimits.parcelSize
  const m = new PositionMessage()
  m.setType(MessageType.POSITION)
  m.setTime(time ? time : Date.now())
  m.setPositionX(p[0] / parcelSize)
  m.setPositionY(p[1])
  m.setPositionZ(p[2] / parcelSize)
  m.setRotationX(p[3])
  m.setRotationY(p[4])
  m.setRotationZ(p[5])
  m.setRotationW(p[6])

  const result = sendMessage(context, m)
  if (context.stats) {
    context.stats.position.incrementSent(result.sent, result.sent * result.bytesPerMessage)
  }
}

function ensureConnection(context: Context, url: string, p: V2) {
  let connection = findConnectionByUrl(context, url)
  if (!connection) {
    const serverId = ensureServer(context, url)
    connection = new ServerConnection(serverId, url, p)

    if (config.debug) {
      connection.pingInterval = setInterval(() => {
        const msg = new PingMessage()
        msg.setType(MessageType.PING)
        msg.setTime(Date.now())
        const bytes = msg.serializeBinary()

        if (connection) {
          const ws = connection.ws
          if (ws && ws.readyState === SocketReadyState.OPEN) {
            ws.send(bytes)
          }
        }
      }, 10000)
    }
    context.connections.push(connection)
  } else if (!connection.containsLocation(p)) {
    connection.addLocation(p)
  }

  return connection
}

function findConnectionByUrl(context: Context, url: string): ServerConnection | null {
  for (let connection of context.connections) {
    if (connection.url === url) {
      return connection
    }
  }

  return null
}

// NOTE: returns true if flow already open in the right state
function openParcelFlow(context: Context, connection: ServerConnection): boolean {
  const status = config.webrtcSupportEnabled ? FlowStatus.OPEN_WEBRTC_PREFERRED : FlowStatus.OPEN
  return changeFlowStatus(context, connection, status)
}

// NOTE: returns true if flow already open in the right state
function openAntennaFlow(context: Context, connection: ServerConnection): boolean {
  return changeFlowStatus(context, connection, FlowStatus.OPEN)
}

function closeFlow(context: Context, connection: ServerConnection) {
  changeFlowStatus(context, connection, FlowStatus.CLOSE)
}

function changeFlowStatus(context: Context, connection: ServerConnection, status: FlowStatus): boolean {
  if (connection.flowStatus === status) {
    return true
  }
  sendFlowStatusMessage(context, connection, status)
  connection.flowStatus = status
  return false
}

function sendFlowStatusMessage(context: Context, connection: ServerConnection, status: FlowStatus) {
  const ws = connection.ws
  if (!ws || ws.readyState !== SocketReadyState.OPEN) {
    throw new Error('try to send flow status to a non ready ws')
  }
  const msg = new FlowStatusMessage()
  msg.setType(MessageType.FLOW_STATUS)
  msg.setTime(Date.now())
  msg.setFlowStatus(status)
  const bytes = msg.serializeBinary()
  ws.send(bytes)

  if (context.stats) {
    context.stats.others.incrementSent(1, bytes.length)
  }
}

function sendMessage(context: Context, msg: any): SendResult {
  const genericMessage = msg as GenericMessage
  if (genericMessage.getType() === MessageType.UNKNOWN_MESSAGE_TYPE) {
    throw Error('cannot send a message without a type')
  }

  const bytes = msg.serializeBinary()
  const result = { bytesPerMessage: bytes.length, sent: 0 } as SendResult

  for (let connection of context.connections) {
    const ws = connection.ws
    if (ws && ws.readyState === SocketReadyState.OPEN) {
      ws.send(bytes)
      result.sent++
    }
  }

  return result
}

function closeWs(ws: WebSocket) {
  ws.onmessage = null
  ws.onerror = null
  ws.onclose = null
  ws.close()
}

function ensureServer({ knownServers }: Context, url: string): ServerId {
  let serverId = knownServers.get(url)
  if (typeof serverId === 'undefined') {
    serverId = knownServers.size
  }
  knownServers.set(url, serverId)

  return serverId
}

function findParcelCommServerUrl({ commData }: Context, p: V2) {
  const key = `${p.x},${p.z}`
  const parcelData = commData.get(key)
  if (parcelData && parcelData.commServerUrl) {
    return parcelData.commServerUrl
  } else {
    return config.defaultCommServerUrl
  }
}
