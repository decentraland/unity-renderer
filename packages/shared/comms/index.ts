import 'webrtc-adapter'

import { commConfigurations as config, ETHEREUM_NETWORK, commConfigurations, networkConfigurations } from 'config'
import { saveToLocalStorage } from 'atomicHelpers/localStorage'
import { positionObserver } from 'shared/world/positionThings'
import { CommunicationArea, squareDistance, Position, position2parcel, sameParcel } from './utils'
import { Stats, NetworkStats, PkgStats } from './debug'

import {
  getCurrentPeer,
  localProfileUUID,
  getUser,
  removeById,
  setLocalProfile,
  getCurrentUser,
  receiveUserData,
  receiveUserVisible,
  receiveUserPose,
  getUserProfile
} from './peers'

import { ChatData, PositionData, ProfileData } from './commproto_pb'
import { chatObservable, ChatEvent } from './chat'
import { WorldInstanceConnection } from './worldInstanceConnection'
import { ReadOnlyVector3, ReadOnlyQuaternion } from 'decentraland-ecs/src'
import { UserInformation, Pose } from './types'

type Timestamp = number
type PeerAlias = string

export class PeerTrackingInfo {
  public position: Position | null = null
  public profile: UserInformation | null = null
  public lastPositionUpdate: Timestamp = 0
  public lastProfileUpdate: Timestamp = 0
  public receivedPublicChatMessages = new Set<string>()
}

export class Context {
  public stats: Stats | null = null
  public commRadius: number

  public peerData = new Map<PeerAlias, PeerTrackingInfo>()
  public userProfile: UserInformation

  public positionTopics = new Set<string>()
  public profileTopics = new Set<string>()
  public chatTopics = new Set<string>()

  public currentPosition: Position | null = null

  public network: ETHEREUM_NETWORK | null

  public worldInstanceConnection: WorldInstanceConnection | null

  constructor(userProfile: UserInformation, network?: ETHEREUM_NETWORK) {
    this.userProfile = userProfile
    this.network = network || null

    this.stats = config.debug ? new Stats(this) : null
    this.commRadius = commConfigurations.commRadius
  }
}

let context: Context | null

export function sendPublicChatMessage(messageId: string, text: string) {
  if (context.currentPosition) {
    context.worldInstanceConnection.sendChatMessage(context.currentPosition, messageId, text)
  }
}

export function persistCurrentUser(changes: Partial<UserInformation>): Readonly<UserInformation> {
  const peer = getCurrentPeer()

  Object.assign(peer.user, changes)

  saveToLocalStorage('dcl-profile', peer.user)

  receiveUserData(localProfileUUID, peer.user)

  const user = peer.user
  if (!context) {
    throw new Error('persistCurrentUser before initialization')
  }
  if (user) {
    context.userProfile = user
  }

  return peer.user
}

function ensurePeerTrackingInfo(context: Context, alias: string): PeerTrackingInfo {
  let peerTrackingInfo = context.peerData.get(alias)

  if (!peerTrackingInfo) {
    peerTrackingInfo = new PeerTrackingInfo()
    context.peerData.set(alias, peerTrackingInfo)
  }
  return peerTrackingInfo
}

export function processChatMessage(
  context: Context,
  conn: WorldInstanceConnection,
  fromAlias: string,
  data: Uint8Array
): PkgStats {
  const chatData = ChatData.deserializeBinary(data)
  const msgId = chatData.getMessageId()

  const peerTrackingInfo = ensurePeerTrackingInfo(context, fromAlias)
  if (!peerTrackingInfo.receivedPublicChatMessages.has(msgId)) {
    const text = chatData.getText()
    peerTrackingInfo.receivedPublicChatMessages.add(msgId)

    const user = getUser(fromAlias)

    if (user) {
      const { displayName } = user
      const entry = {
        id: msgId,
        sender: displayName,
        message: text,
        isCommand: false
      }
      chatObservable.notifyObservers({ type: ChatEvent.MESSAGE_RECEIVED, messageEntry: entry })
    }
  }

  return conn.stats ? conn.stats.chat : null
}

export function processProfileMessage(
  context: Context,
  conn: WorldInstanceConnection,
  fromAlias: string,
  data: Uint8Array
): PkgStats {
  const profileData = ProfileData.deserializeBinary(data)
  const msgTimestamp = profileData.getTime()

  const peerTrackingInfo = ensurePeerTrackingInfo(context, fromAlias)

  if (msgTimestamp > peerTrackingInfo.lastProfileUpdate) {
    const publicKey = profileData.getPublicKey()
    const avatarType = profileData.getAvatarType()
    const displayName = profileData.getDisplayName()

    peerTrackingInfo.profile = {
      displayName,
      publicKey,
      avatarType
    }

    peerTrackingInfo.lastProfileUpdate = msgTimestamp
  }
  return conn.stats ? conn.stats.profile : null
}

export function processPositionMessage(
  context: Context,
  conn: WorldInstanceConnection,
  fromAlias: string,
  data: Uint8Array
): PkgStats {
  const positionData = PositionData.deserializeBinary(data)
  const msgTimestamp = positionData.getTime()

  const peerTrackingInfo = ensurePeerTrackingInfo(context, fromAlias)
  if (msgTimestamp > peerTrackingInfo.lastPositionUpdate) {
    const p = [
      positionData.getPositionX(),
      positionData.getPositionY(),
      positionData.getPositionZ(),
      positionData.getRotationX(),
      positionData.getRotationY(),
      positionData.getRotationZ(),
      positionData.getRotationW()
    ] as Position

    peerTrackingInfo.position = p
    peerTrackingInfo.lastPositionUpdate = msgTimestamp
  }

  return conn.stats ? conn.stats.position : null
}

type ProcessingPeerInfo = {
  alias: PeerAlias
  profile: UserInformation
  squareDistance: number
  position: Position
}

function updateTopics(
  conn: WorldInstanceConnection,
  oldTopics: Set<string>,
  newTopics: Set<string>,
  handler: (fromAlias: string, data: Uint8Array) => PkgStats
) {
  for (let topic of newTopics) {
    if (!oldTopics.has(topic)) {
      conn.addTopic(topic, handler)
    } else {
      oldTopics.delete(topic)
    }
  }

  for (let topic of oldTopics) {
    conn.removeTopic(topic)
  }

  return newTopics
}

export function onPositionUpdate(context: Context, p: Position) {
  if (!context.worldInstanceConnection.unreliableDataChannel || !context.worldInstanceConnection.reliableDataChannel) {
    return
  }

  const oldParcel = context.currentPosition ? position2parcel(context.currentPosition) : null
  const newParcel = position2parcel(p)

  if (!sameParcel(oldParcel, newParcel)) {
    const newPositionTopics = new Set<string>()
    const newProfileTopics = new Set<string>()
    const newChatTopics = new Set<string>()

    const commArea = new CommunicationArea(newParcel, context.commRadius)
    for (let x = commArea.vMin.x; x <= commArea.vMax.x; ++x) {
      for (let z = commArea.vMin.z; z <= commArea.vMax.z; ++z) {
        newPositionTopics.add(`position:${x}:${z}`)
        newProfileTopics.add(`profile:${x}:${z}`)
        newChatTopics.add(`chat:${x}:${z}`)
      }
    }

    context.positionTopics = updateTopics(
      context.worldInstanceConnection,
      context.positionTopics,
      newPositionTopics,
      (fromAlias: string, data: Uint8Array) =>
        processPositionMessage(context, context.worldInstanceConnection, fromAlias, data)
    )

    context.profileTopics = updateTopics(
      context.worldInstanceConnection,
      context.profileTopics,
      newProfileTopics,
      (fromAlias: string, data: Uint8Array) =>
        processProfileMessage(context, context.worldInstanceConnection, fromAlias, data)
    )

    context.chatTopics = updateTopics(
      context.worldInstanceConnection,
      context.chatTopics,
      newChatTopics,
      (fromAlias: string, data: Uint8Array) =>
        processChatMessage(context, context.worldInstanceConnection, fromAlias, data)
    )
  }

  context.currentPosition = p
  context.worldInstanceConnection.sendPositionMessage(p)
}

function collectInfo(context: Context) {
  if (context.stats) {
    context.stats.collectInfoDuration.start()
  }

  if (!context.currentPosition) {
    return
  }

  const now = Date.now()
  const visiblePeers: ProcessingPeerInfo[] = []
  const commArea = new CommunicationArea(position2parcel(context.currentPosition), commConfigurations.commRadius)
  for (let [peerAlias, trackingInfo] of context.peerData) {
    if (!trackingInfo.position || !trackingInfo.profile) {
      continue
    }

    if (!commArea.contains(trackingInfo.position)) {
      receiveUserVisible(peerAlias, false)
      continue
    }

    const msSinceLastUpdate = now - Math.max(trackingInfo.lastPositionUpdate, trackingInfo.lastProfileUpdate)

    if (msSinceLastUpdate > config.peerTtlMs) {
      context.peerData.delete(peerAlias)
      removeById(peerAlias)
      continue
    }

    visiblePeers.push({
      position: trackingInfo.position,
      profile: trackingInfo.profile,
      squareDistance: squareDistance(context.currentPosition, trackingInfo.position),
      alias: peerAlias
    })
  }

  if (visiblePeers.length <= config.maxVisiblePeers) {
    for (let peerInfo of visiblePeers) {
      const alias = peerInfo.alias
      receiveUserVisible(alias, true)
      receiveUserPose(alias, peerInfo.position as Pose)
      receiveUserData(alias, peerInfo.profile)
    }
  } else {
    const sortedBySqDistanceVisiblePeers = visiblePeers.sort((p1, p2) => p1.squareDistance - p2.squareDistance)
    for (let i = 0; i < sortedBySqDistanceVisiblePeers.length; ++i) {
      const peer = sortedBySqDistanceVisiblePeers[i]
      const alias = peer.alias

      if (i < config.maxVisiblePeers) {
        receiveUserVisible(alias, true)
        receiveUserPose(alias, peer.position as Pose)
        receiveUserData(alias, peer.profile)
      } else {
        receiveUserVisible(alias, false)
      }
    }
  }

  if (context.stats) {
    context.stats.visiblePeersCount = visiblePeers.length
    context.stats.collectInfoDuration.stop()
  }
}

export async function connect(ethAddress: string, network?: ETHEREUM_NETWORK) {
  const peerId = ethAddress

  setLocalProfile(peerId, {
    ...getUserProfile(),
    publicKey: ethAddress
  })

  const user = getCurrentUser()
  if (user) {
    const userProfile = {
      displayName: user.displayName,
      publicKey: user.publicKey,
      avatarType: user.avatarType
    }
    context = new Context(userProfile, network)
    context.worldInstanceConnection = new WorldInstanceConnection(networkConfigurations[network].worldInstanceUrl)

    if (context.stats) {
      context.worldInstanceConnection.stats = new NetworkStats(context.worldInstanceConnection)
      context.stats.primaryNetworkStats = context.worldInstanceConnection.stats
    }

    context.worldInstanceConnection.connect()

    setInterval(() => {
      if (context.currentPosition) {
        context.worldInstanceConnection.sendProfileMessage(context.currentPosition, context.userProfile)
      }
    }, 1000)

    positionObserver.add(
      (
        obj: Readonly<{
          position: ReadOnlyVector3
          rotation: ReadOnlyVector3
          quaternion: ReadOnlyQuaternion
        }>
      ) => {
        const p = [
          obj.position.x,
          obj.position.y,
          obj.position.z,
          obj.quaternion.x,
          obj.quaternion.y,
          obj.quaternion.z,
          obj.quaternion.w
        ] as Position

        onPositionUpdate(context, p)
      }
    )

    setInterval(() => collectInfo(context), 100)
  }
}
