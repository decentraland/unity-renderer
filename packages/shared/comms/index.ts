import 'webrtc-adapter'

import { parcelLimits, ETHEREUM_NETWORK, commConfigurations, getNetworkConfigurations, playerConfigurations } from 'config'

import { saveToLocalStorage } from 'atomicHelpers/localStorage'
import { positionObservable } from 'shared/world/positionThings'
import { CommunicationArea, squareDistance, Position, position2parcel, sameParcel } from './utils'
import { Stats } from './debug'

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
  public lastUpdate: Timestamp = 0
  public receivedPublicChatMessages = new Set<string>()
}

export class Context {
  public stats: Stats | null = null
  public commRadius: number

  public peerData = new Map<PeerAlias, PeerTrackingInfo>()
  public userProfile: UserInformation

  public currentPosition: Position | null = null

  public network: ETHEREUM_NETWORK | null

  public worldInstanceConnection: WorldInstanceConnection | null = null

  constructor(userProfile: UserInformation, network?: ETHEREUM_NETWORK) {
    this.userProfile = userProfile
    this.network = network || null

    this.commRadius = commConfigurations.commRadius
  }
}

let context: Context | null = null

export function sendPublicChatMessage(messageId: string, text: string) {
  if (context && context.currentPosition && context.worldInstanceConnection) {
    context.worldInstanceConnection.sendChatMessage(context.currentPosition, messageId, text)
  }
}

export function persistCurrentUser(changes: Partial<UserInformation>): Readonly<UserInformation> {
  const peer = getCurrentPeer()

  if (!peer || !localProfileUUID) throw new Error('cannotGetCurrentPeer')
  if (!peer.user) throw new Error('cannotGetCurrentPeer.user')

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

export function processChatMessage(context: Context, fromAlias: string, data: ChatData) {
  const msgId = data.getMessageId()

  const peerTrackingInfo = ensurePeerTrackingInfo(context, fromAlias)
  if (!peerTrackingInfo.receivedPublicChatMessages.has(msgId)) {
    const text = data.getText()
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
}

export function processProfileMessage(context: Context, fromAlias: string, data: ProfileData) {
  const msgTimestamp = data.getTime()

  const peerTrackingInfo = ensurePeerTrackingInfo(context, fromAlias)

  if (msgTimestamp > peerTrackingInfo.lastProfileUpdate) {
    const publicKey = data.getPublicKey()
    const avatarType = data.getAvatarType()
    const displayName = data.getDisplayName()

    peerTrackingInfo.profile = {
      displayName,
      publicKey,
      avatarType
    }

    peerTrackingInfo.lastProfileUpdate = msgTimestamp
    peerTrackingInfo.lastUpdate = Date.now()
  }
}

export function processPositionMessage(context: Context, fromAlias: string, positionData: PositionData) {
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
    peerTrackingInfo.lastUpdate = Date.now()
  }
}

type ProcessingPeerInfo = {
  alias: PeerAlias
  profile: UserInformation
  squareDistance: number
  position: Position
}

export function onPositionUpdate(context: Context, p: Position) {
  const worldConnection = context.worldInstanceConnection

  if (!worldConnection || !worldConnection.unreliableDataChannel || !worldConnection.reliableDataChannel) {
    return
  }

  const oldParcel = context.currentPosition ? position2parcel(context.currentPosition) : null
  const newParcel = position2parcel(p)

  if (!sameParcel(oldParcel, newParcel)) {
    const commArea = new CommunicationArea(newParcel, context.commRadius)

    const xMin = ((commArea.vMin.x + parcelLimits.maxParcelX) >> 2) << 2
    const xMax = ((commArea.vMax.x + parcelLimits.maxParcelX) >> 2) << 2
    const zMin = ((commArea.vMin.z + parcelLimits.maxParcelZ) >> 2) << 2
    const zMax = ((commArea.vMax.z + parcelLimits.maxParcelZ) >> 2) << 2

    let rawTopics = ''
    for (let x = xMin; x <= xMax; x += 4) {
      for (let z = zMin; z <= zMax; z += 4) {
        const hash = `${x >> 2}:${z >> 2}`
        let topic = `position:${hash}`
        rawTopics += topic + ' '

        topic = `profile:${hash}`
        rawTopics += topic + ' '

        topic = `chat:${hash}`
        rawTopics += topic

        if (x !== xMax || z !== zMax) {
          rawTopics += ' '
        }
      }
    }

    worldConnection.updateSubscriptions(rawTopics)
  }

  context.currentPosition = p
  worldConnection.sendPositionMessage(p)
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
    const msSinceLastUpdate = now - trackingInfo.lastUpdate

    if (msSinceLastUpdate > commConfigurations.peerTtlMs) {
      context.peerData.delete(peerAlias)
      removeById(peerAlias)

      if (context.stats) {
        context.stats.onPeerRemoved(peerAlias)
      }

      continue
    }

    if (!trackingInfo.position || !trackingInfo.profile) {
      continue
    }

    if (!commArea.contains(trackingInfo.position)) {
      receiveUserVisible(peerAlias, false)
      continue
    }

    visiblePeers.push({
      position: trackingInfo.position,
      profile: trackingInfo.profile,
      squareDistance: squareDistance(context.currentPosition, trackingInfo.position),
      alias: peerAlias
    })
  }

  if (visiblePeers.length <= commConfigurations.maxVisiblePeers) {
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

      if (i < commConfigurations.maxVisiblePeers) {
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
    context.stats.trackingPeersCount = context.peerData.size
    context.stats.collectInfoDuration.stop()
  }
}

export async function connect(ethAddress: string, network: ETHEREUM_NETWORK) {
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

    const connection = new WorldInstanceConnection(getNetworkConfigurations(network).worldInstanceUrl)
    connection.positionHandler = (alias, data) => processPositionMessage(context!, alias, data)
    connection.profileHandler = (alias, data) => processProfileMessage(context!, alias, data)
    connection.chatHandler = (alias, data) => processChatMessage(context!, alias, data)
    context = new Context(userProfile, network)
    context.worldInstanceConnection = connection

    if (commConfigurations.debug) {
      context.stats = new Stats(context)
      context.worldInstanceConnection.stats = context.stats
    }

    context.worldInstanceConnection.connect()

    setInterval(() => {
      if (context && context.currentPosition && context.worldInstanceConnection) {
        context.worldInstanceConnection.sendProfileMessage(context.currentPosition, context.userProfile)
      }
    }, 1000)

    positionObservable.add(
      (
        obj: Readonly<{
          position: ReadOnlyVector3
          rotation: ReadOnlyVector3
          quaternion: ReadOnlyQuaternion
        }>
      ) => {
        const p = [
          obj.position.x,
          obj.position.y - playerConfigurations.height,
          obj.position.z,
          obj.quaternion.x,
          obj.quaternion.y,
          obj.quaternion.z,
          obj.quaternion.w
        ] as Position

        if (context) {
          onPositionUpdate(context, p)
        }
      }
    )

    setInterval(() => {
      if (context) {
        collectInfo(context)
      }
    }, 100)
  }
}
