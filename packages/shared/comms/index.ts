import 'webrtc-adapter'

import {
  parcelLimits,
  ETHEREUM_NETWORK,
  commConfigurations,
  playerConfigurations,
  getServerConfigurations,
  USE_LOCAL_COMMS
} from 'config'

import { saveToLocalStorage } from 'atomicHelpers/localStorage'
import { positionObservable } from 'shared/world/positionThings'
import { CommunicationArea, squareDistance, Position, position2parcel, sameParcel } from './utils'
import { Stats } from './debug'
import { Auth } from 'decentraland-auth'

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
  getUserProfile,
  getPeer
} from './peers'

import { ChatData, PositionData, ProfileData } from './proto/comms'
import { chatObservable, ChatEvent } from './chat'
import { WorldInstanceConnection } from './worldInstanceConnection'
import { BrokerConnection } from './BrokerConnection'
import { ReadOnlyVector3, ReadOnlyQuaternion } from 'decentraland-ecs/src'
import { UserInformation, Pose } from './types'
import { CommunicationsController } from 'shared/apis/CommunicationsController'
import { CliBrokerConnection } from './CliBrokerConnection'
import { log } from 'engine/logger'
import { MessageEntry } from 'shared/types'
import { IBrokerConnection } from './IBrokerConnection'

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
  public readonly stats: Stats = new Stats(this)
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
const scenesSubscribedToCommsEvents = new Set<CommunicationsController>()

/**
 * Returns a list of CIDs that must receive scene messages from comms
 */
function getParcelSceneSubscriptions(): string[] {
  let ids: string[] = []

  scenesSubscribedToCommsEvents.forEach($ => {
    ids.push($.cid)
  })

  return ids
}

export function sendPublicChatMessage(messageId: string, text: string) {
  if (context && context.currentPosition && context.worldInstanceConnection) {
    context.worldInstanceConnection.sendChatMessage(context.currentPosition, messageId, text)
  }
}

export function sendParcelSceneCommsMessage(cid: string, message: string) {
  if (context && context.currentPosition && context.worldInstanceConnection) {
    context.worldInstanceConnection.sendParcelSceneCommsMessage(cid, message)
  }
}

export function subscribeParcelSceneToCommsMessages(controller: CommunicationsController) {
  scenesSubscribedToCommsEvents.add(controller)
}

export function unsubscribeParcelSceneToCommsMessages(controller: CommunicationsController) {
  scenesSubscribedToCommsEvents.delete(controller)
}

// TODO: Change ChatData to the new class once it is added to the .proto
export function processParcelSceneCommsMessage(context: Context, fromAlias: string, data: ChatData) {
  const cid = data.getMessageId()
  const text = data.getText()

  const peer = getPeer(fromAlias)

  if (peer) {
    scenesSubscribedToCommsEvents.forEach($ => {
      if ($.cid === cid) {
        $.receiveCommsMessage(text, peer)
      }
    })
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
      const entry: MessageEntry = {
        id: msgId,
        sender: displayName || user.publicKey || 'unknown',
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

let currentParcelTopics = ''
let previousTopics = ''

export function onPositionUpdate(context: Context, p: Position) {
  const worldConnection = context.worldInstanceConnection

  if (!worldConnection || !worldConnection.connection.isAuthenticated) {
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

    let rawTopics: string[] = []
    for (let x = xMin; x <= xMax; x += 4) {
      for (let z = zMin; z <= zMax; z += 4) {
        const hash = `${x >> 2}:${z >> 2}`
        if (!rawTopics.includes(hash)) {
          rawTopics.push(hash)
        }
      }
    }

    currentParcelTopics = rawTopics.join(' ')
  }

  const parcelSceneSubscriptions = getParcelSceneSubscriptions()

  const parcelSceneCommsTopics = parcelSceneSubscriptions.join(' ')

  const topics = currentParcelTopics + (parcelSceneCommsTopics.length ? ' ' + parcelSceneCommsTopics : '')

  if (topics !== previousTopics) {
    worldConnection.updateSubscriptions(topics)
    previousTopics = topics
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

export async function connect(userId: string, network: ETHEREUM_NETWORK, auth: Auth, ethAddress?: string) {
  setLocalProfile(userId, {
    ...getUserProfile(),
    publicKey: ethAddress || null
  })

  const user = getCurrentUser()
  if (!user) {
    return
  }

  const userProfile = {
    displayName: user.displayName,
    publicKey: user.publicKey,
    avatarType: user.avatarType
  }

  let commsBroker: IBrokerConnection

  if (USE_LOCAL_COMMS) {
    commsBroker = new CliBrokerConnection(document.location.toString().replace(/^http/, 'ws'))
  } else {
    const coordinatorURL = getServerConfigurations().worldInstanceUrl
    const body = `GET:${coordinatorURL}`
    const credentials = await auth.getMessageCredentials(body)

    const qs = new URLSearchParams({
      signature: credentials['x-signature'],
      identity: credentials['x-identity'],
      timestamp: credentials['x-timestamp'],
      'access-token': credentials['x-access-token']
    })
    const url = new URL(coordinatorURL)
    url.search = qs.toString()
    commsBroker = new BrokerConnection(auth, url.toString())
  }

  const connection = new WorldInstanceConnection(commsBroker)

  await connection.connection.isConnected

  connection.positionHandler = (alias: string, data: PositionData) => {
    processPositionMessage(context!, alias, data)
  }
  connection.profileHandler = (alias: string, data: ProfileData) => {
    processProfileMessage(context!, alias, data)
  }
  connection.chatHandler = (alias: string, data: ChatData) => {
    processChatMessage(context!, alias, data)
  }
  connection.sceneMessageHandler = (alias: string, data: ChatData) => {
    processParcelSceneCommsMessage(context!, alias, data)
  }

  context = new Context(userProfile, network)
  context.worldInstanceConnection = connection

  if (commConfigurations.debug) {
    connection.stats = context.stats
    commsBroker.stats = context.stats
  }

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

declare var global: any

global['printCommsInformation'] = function() {
  if (context) {
    log('Communication topics: ' + previousTopics)
    context.stats.printDebugInformation()
  }
}
