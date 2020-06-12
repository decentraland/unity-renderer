import { saveToLocalStorage } from 'atomicHelpers/localStorage'
import { commConfigurations, parcelLimits, COMMS, AUTO_CHANGE_REALM, USE_NEW_CHAT } from 'config'
import { CommunicationsController } from 'shared/apis/CommunicationsController'
import { defaultLogger } from 'shared/logger'
import { ChatMessage as InternalChatMessage, ChatMessageType, MessageEntry } from 'shared/types'
import { positionObservable, PositionReport } from 'shared/world/positionThings'
import { ProfileAsPromise } from '../profiles/ProfileAsPromise'
import { notifyStatusThroughChat, chatObservable, ChatEventType } from './chat'
import { CliBrokerConnection } from './CliBrokerConnection'
import { Stats } from './debug'
import { IBrokerConnection } from '../comms/v1/IBrokerConnection'
import {
  getCurrentPeer,
  getCurrentUser,
  getPeer,
  getUser,
  localProfileUUID,
  receiveUserData,
  receiveUserPose,
  receiveUserVisible,
  removeById,
  avatarMessageObservable
} from './peers'
import {
  Pose,
  UserInformation,
  Package,
  ChatMessage,
  ProfileVersion,
  BusMessage,
  AvatarMessageType,
  ConnectionEstablishmentError,
  IdTakenError,
  UnknownCommsModeError
} from './interface/types'
import {
  CommunicationArea,
  Position,
  position2parcel,
  sameParcel,
  squareDistance,
  ParcelArray
} from './interface/utils'
import { BrokerWorldInstanceConnection } from '../comms/v1/brokerWorldInstanceConnection'
import { profileToRendererFormat } from 'shared/profiles/transformations/profileToRendererFormat'
import { ProfileForRenderer } from 'decentraland-ecs/src'
import { worldRunningObservable, isWorldRunning } from '../world/worldState'
import { WorldInstanceConnection } from './interface/index'

import { LighthouseWorldInstanceConnection } from './v2/LighthouseWorldInstanceConnection'

import { identity } from '../index'
import { Authenticator } from 'dcl-crypto'
import { getCommsServer, getRealm, getAllCatalystCandidates } from '../dao/selectors'
import { Realm, LayerUserInfo } from 'shared/dao/types'
import { Store } from 'redux'
import { RootState, StoreContainer } from 'shared/store/rootTypes'
import { store } from 'shared/store/store'
import {
  setCatalystRealmCommsStatus,
  setCatalystRealm,
  markCatalystRealmFull,
  markCatalystRealmConnectionError
} from 'shared/dao/actions'
import { observeRealmChange, pickCatalystRealm, changeToCrowdedRealm } from 'shared/dao'
import { getProfile } from 'shared/profiles/selectors'
import { Profile } from 'shared/profiles/types'
import { realmToString } from '../dao/utils/realmToString'
import { queueTrackingEvent } from 'shared/analytics'
import { messageReceived } from '../chat/actions'

export type CommsVersion = 'v1' | 'v2'
export type CommsMode = CommsV1Mode | CommsV2Mode
export type CommsV1Mode = 'local' | 'remote'
export type CommsV2Mode = 'p2p' | 'server'

type Timestamp = number
type PeerAlias = string

export const MORDOR_POSITION: Position = [
  1000 * parcelLimits.parcelSize,
  1000,
  1000 * parcelLimits.parcelSize,
  0,
  0,
  0,
  0
]

type CommsContainer = {
  printCommsInformation: () => void
}

declare const globalThis: StoreContainer & CommsContainer

export class PeerTrackingInfo {
  public position: Position | null = null
  public identity: string | null = null
  public userInfo: UserInformation | null = null
  public lastPositionUpdate: Timestamp = 0
  public lastProfileUpdate: Timestamp = 0
  public lastUpdate: Timestamp = 0
  public receivedPublicChatMessages = new Set<string>()

  profilePromise: { promise: Promise<ProfileForRenderer | void>; version: number | null } = {
    promise: Promise.resolve(),
    version: null
  }

  public loadProfileIfNecessary(profileVersion: number) {
    if (this.identity && profileVersion !== this.profilePromise.version) {
      if (!this.userInfo || !this.userInfo.userId) {
        this.userInfo = {
          ...(this.userInfo || {}),
          userId: this.identity
        }
      }
      this.profilePromise = {
        promise: ProfileAsPromise(this.identity, profileVersion)
          .then(profile => {
            const forRenderer = profileToRendererFormat(profile)
            this.lastProfileUpdate = new Date().getTime()
            const userInfo = this.userInfo || {}
            userInfo.profile = forRenderer
            userInfo.version = profile.version
            this.userInfo = userInfo
            return forRenderer
          })
          .catch(error => {
            defaultLogger.error('Error fetching profile!', error)
          }),
        version: profileVersion
      }
    }
  }
}

export class Context {
  public readonly stats: Stats = new Stats(this)
  public commRadius: number

  public peerData = new Map<PeerAlias, PeerTrackingInfo>()
  public userInfo: UserInformation

  public currentPosition: Position | null = null

  public worldInstanceConnection: WorldInstanceConnection | null = null

  profileInterval?: NodeJS.Timer
  positionObserver: any
  worldRunningObserver: any
  infoCollecterInterval?: NodeJS.Timer
  analyticsInterval?: NodeJS.Timer

  timeToChangeRealm: number = Date.now() + commConfigurations.autoChangeRealmInterval

  positionUpdatesPaused: boolean = false

  constructor(userInfo: UserInformation) {
    this.userInfo = userInfo

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
    context.worldInstanceConnection
      .sendChatMessage(context.currentPosition, messageId, text)
      .catch(e => defaultLogger.warn(`error while sending message `, e))
  }
}

export function sendParcelSceneCommsMessage(cid: string, message: string) {
  if (context && context.currentPosition && context.worldInstanceConnection) {
    context.worldInstanceConnection
      .sendParcelSceneCommsMessage(cid, message)
      .catch(e => defaultLogger.warn(`error while sending message `, e))
  }
}

export function subscribeParcelSceneToCommsMessages(controller: CommunicationsController) {
  scenesSubscribedToCommsEvents.add(controller)
}

export function unsubscribeParcelSceneToCommsMessages(controller: CommunicationsController) {
  scenesSubscribedToCommsEvents.delete(controller)
}

async function changeConnectionRealm(realm: Realm, url: string) {
  defaultLogger.log('Changing connection realm to ', JSON.stringify(realm), { url })
  if (context && context.worldInstanceConnection) {
    context.positionUpdatesPaused = true
    try {
      removeAllPeers(context)
      await sendToMordorAsync()
      await context.worldInstanceConnection.changeRealm(realm, url)
    } finally {
      context.positionUpdatesPaused = false
    }
  }
}

// TODO: Change ChatData to the new class once it is added to the .proto
export function processParcelSceneCommsMessage(context: Context, fromAlias: string, message: Package<ChatMessage>) {
  const { id: cid, text } = message.data

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
  if (context) {
    if (user) {
      context.userInfo = user
    }
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

export function processChatMessage(context: Context, fromAlias: string, message: Package<ChatMessage>) {
  const msgId = message.data.id
  const profile = getProfile(globalThis.globalStore.getState(), identity.address)

  const peerTrackingInfo = ensurePeerTrackingInfo(context, fromAlias)
  if (!peerTrackingInfo.receivedPublicChatMessages.has(msgId)) {
    const text = message.data.text
    peerTrackingInfo.receivedPublicChatMessages.add(msgId)

    const user = getUser(fromAlias)
    if (user) {
      const displayName = user.profile && user.profile.name

      if (text.startsWith('␐')) {
        const [id, timestamp] = text.split(' ')
        avatarMessageObservable.notifyObservers({
          type: AvatarMessageType.USER_EXPRESSION,
          uuid: fromAlias,
          expressionId: id.slice(1),
          timestamp: parseInt(timestamp, 10)
        })
      } else {
        if (profile && user.userId && !isBlocked(profile, user.userId)) {
          if (USE_NEW_CHAT) {
            const messageEntry: InternalChatMessage = {
              messageType: ChatMessageType.PUBLIC,
              messageId: msgId,
              sender: displayName || 'unknown',
              body: text,
              timestamp: message.time
            }
            globalThis.globalStore.dispatch(messageReceived(messageEntry))
          } else {
            const messageEntry: MessageEntry = {
              id: msgId,
              sender: displayName || 'unknown',
              isCommand: false,
              message: text,
              timestamp: message.time
            }
            chatObservable.notifyObservers({ type: ChatEventType.MESSAGE_RECEIVED, messageEntry })
          }
        }
      }
    }
  }
}

function isBlocked(profile: Profile, userId: string): boolean {
  return profile.blocked && profile.blocked.includes(userId)
}

export function processProfileMessage(
  context: Context,
  fromAlias: string,
  peerIdentity: string,
  message: Package<ProfileVersion>
) {
  const msgTimestamp = message.time

  const peerTrackingInfo = ensurePeerTrackingInfo(context, fromAlias)

  if (msgTimestamp > peerTrackingInfo.lastProfileUpdate) {
    peerTrackingInfo.lastProfileUpdate = msgTimestamp
    peerTrackingInfo.identity = peerIdentity
    peerTrackingInfo.lastUpdate = Date.now()

    if (ensureTrackingUniqueAndLatest(context, fromAlias, peerIdentity, msgTimestamp)) {
      const profileVersion = message.data.version
      peerTrackingInfo.loadProfileIfNecessary(profileVersion ? parseInt(profileVersion, 10) : 0)
    }
  }
}

/**
 * Ensures that there is only one peer tracking info for this identity.
 * Returns true if this is the latest update and the one that remains
 */
function ensureTrackingUniqueAndLatest(context: Context, fromAlias: string, peerIdentity: string, thisUpdateTimestamp: Timestamp) {
  let currentLastProfileAlias = fromAlias
  let currentLastProfileUpdate = thisUpdateTimestamp

  context.peerData.forEach((info, key) => {
    if (info.identity === peerIdentity) {
      if (info.lastProfileUpdate < currentLastProfileUpdate) {
        removePeer(context, key)
      } else if (info.lastProfileUpdate > currentLastProfileUpdate) {
        removePeer(context, currentLastProfileAlias)
        currentLastProfileAlias = key
        currentLastProfileUpdate = info.lastProfileUpdate
      }
    }
  })

  return currentLastProfileAlias === fromAlias
}

export function processPositionMessage(context: Context, fromAlias: string, message: Package<Position>) {
  const msgTimestamp = message.time

  const peerTrackingInfo = ensurePeerTrackingInfo(context, fromAlias)
  if (msgTimestamp > peerTrackingInfo.lastPositionUpdate) {
    const p = message.data

    peerTrackingInfo.position = p
    peerTrackingInfo.lastPositionUpdate = msgTimestamp
    peerTrackingInfo.lastUpdate = Date.now()
  }
}

type ProcessingPeerInfo = {
  alias: PeerAlias
  userInfo: UserInformation
  squareDistance: number
  position: Position
}

let currentParcelTopics = ''
let previousTopics = ''

let lastNetworkUpdatePosition = new Date().getTime()

export function onPositionUpdate(context: Context, p: Position) {
  const worldConnection = context.worldInstanceConnection

  if (!worldConnection || !worldConnection.isAuthenticated) {
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
    if (context.currentPosition && !context.positionUpdatesPaused) {
      worldConnection
        .sendParcelUpdateMessage(context.currentPosition, p)
        .catch(e => defaultLogger.warn(`error while sending message `, e))
    }
  }

  const parcelSceneSubscriptions = getParcelSceneSubscriptions()

  const parcelSceneCommsTopics = parcelSceneSubscriptions.join(' ')

  const topics =
    (context.userInfo.userId ? context.userInfo.userId + ' ' : '') +
    currentParcelTopics +
    (parcelSceneCommsTopics.length ? ' ' + parcelSceneCommsTopics : '')

  if (topics !== previousTopics) {
    worldConnection
      .updateSubscriptions(topics.split(' '))
      .catch(e => defaultLogger.warn(`error while updating subscriptions`, e))
    previousTopics = topics
  }

  context.currentPosition = p
  const now = new Date().getTime()
  if (now - lastNetworkUpdatePosition > 100 && !context.positionUpdatesPaused) {
    lastNetworkUpdatePosition = now
    worldConnection.sendPositionMessage(p).catch(e => defaultLogger.warn(`error while sending message `, e))
  }
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
      removePeer(context, peerAlias)

      continue
    }

    if (trackingInfo.identity === identity.address) {
      // If we are tracking a peer that is ourselves, we remove it
      removePeer(context, peerAlias)
      continue
    }

    if (!trackingInfo.position || !trackingInfo.userInfo) {
      continue
    }

    if (!commArea.contains(trackingInfo.position)) {
      receiveUserVisible(peerAlias, false)
      continue
    }

    visiblePeers.push({
      position: trackingInfo.position,
      userInfo: trackingInfo.userInfo,
      squareDistance: squareDistance(context.currentPosition, trackingInfo.position),
      alias: peerAlias
    })
  }

  if (visiblePeers.length <= commConfigurations.maxVisiblePeers) {
    for (let peerInfo of visiblePeers) {
      const alias = peerInfo.alias
      receiveUserVisible(alias, true)
      receiveUserPose(alias, peerInfo.position as Pose)
      receiveUserData(alias, peerInfo.userInfo)
    }
  } else {
    const sortedBySqDistanceVisiblePeers = visiblePeers.sort((p1, p2) => p1.squareDistance - p2.squareDistance)
    for (let i = 0; i < sortedBySqDistanceVisiblePeers.length; ++i) {
      const peer = sortedBySqDistanceVisiblePeers[i]
      const alias = peer.alias

      if (i < commConfigurations.maxVisiblePeers) {
        receiveUserVisible(alias, true)
        receiveUserPose(alias, peer.position as Pose)
        receiveUserData(alias, peer.userInfo)
      } else {
        receiveUserVisible(alias, false)
      }
    }
  }

  checkAutochangeRealm(visiblePeers, context, now)

  if (context.stats) {
    context.stats.visiblePeerIds = visiblePeers.map(it => it.alias)
    context.stats.trackingPeersCount = context.peerData.size
    context.stats.collectInfoDuration.stop()
  }
}

function checkAutochangeRealm(visiblePeers: ProcessingPeerInfo[], context: Context, now: number) {
  if (AUTO_CHANGE_REALM) {
    if (visiblePeers.length > 0) {
      context.timeToChangeRealm = now + commConfigurations.autoChangeRealmInterval
    } else if (now > context.timeToChangeRealm) {
      context.timeToChangeRealm = now + commConfigurations.autoChangeRealmInterval
      defaultLogger.log('Changing to crowded realm because there is no people around')
      changeToCrowdedRealm().then(
        ([changed, realm]) => {
          if (changed) {
            defaultLogger.log('Successfully changed to realm', realm)
          } else {
            defaultLogger.log('No crowded realm found')
          }
        },
        error => defaultLogger.warn('Error trying to change realm', error)
      )
    }
  }
}

function removeAllPeers(context: Context) {
  for (const alias of context.peerData.keys()) {
    removePeer(context, alias)
  }
}

function removePeer(context: Context, peerAlias: string) {
  context.peerData.delete(peerAlias)
  removeById(peerAlias)
  if (context.stats) {
    context.stats.onPeerRemoved(peerAlias)
  }
}

function parseCommsMode(modeString: string) {
  const segments = modeString.split('-')
  return segments as [CommsVersion, CommsMode]
}

const NOOP = () => {
  // do nothing
}
function subscribeToRealmChange(store: Store<RootState>) {
  observeRealmChange(store, (previousRealm, currentRealm) =>
    changeConnectionRealm(currentRealm, getCommsServer(store.getState())).then(NOOP, defaultLogger.error)
  )
}

export async function connect(userId: string) {
  try {
    const user = getCurrentUser()
    if (!user) {
      return undefined
    }

    const userInfo = {
      ...user
    }

    let connection: WorldInstanceConnection

    const [version, mode] = parseCommsMode(COMMS)
    switch (version) {
      case 'v1': {
        let commsBroker: IBrokerConnection

        switch (mode) {
          case 'local': {
            let location = document.location.toString()
            if (location.indexOf('#') > -1) {
              location = location.substring(0, location.indexOf('#')) // drop fragment identifier
            }
            const commsUrl = location.replace(/^http/, 'ws') // change protocol to ws

            const url = new URL(commsUrl)
            const qs = new URLSearchParams({
              identity: btoa(userId)
            })
            url.search = qs.toString()

            defaultLogger.log('Using WebSocket comms: ' + url.href)
            commsBroker = new CliBrokerConnection(url.href)
            break
          }
          default: {
            throw new UnknownCommsModeError(`unrecognized mode for comms v1 "${mode}"`)
          }
        }

        const instance = new BrokerWorldInstanceConnection(commsBroker)
        await instance.isConnected

        connection = instance
        break
      }
      case 'v2': {
        const store: Store<RootState> = globalThis.globalStore
        const lighthouseUrl = getCommsServer(store.getState())
        const realm = getRealm(store.getState())

        const peerConfig = {
          connectionConfig: {
            iceServers: commConfigurations.iceServers
          },
          authHandler: async (msg: string) => {
            try {
              return Authenticator.signPayload(identity, msg)
            } catch (e) {
              defaultLogger.info(`error while trying to sign message from lighthouse '${msg}'`)
            }
            // if any error occurs
            return identity
          },
          logLevel: 'NONE',
          positionConfig: {
            selfPosition: () => {
              if (context && context.currentPosition) {
                return context.currentPosition.slice(0, 3)
              }
            }
          }
        }

        defaultLogger.log('Using Remote lighthouse service: ', lighthouseUrl)

        connection = new LighthouseWorldInstanceConnection(
          identity.address,
          realm!,
          lighthouseUrl,
          peerConfig,
          status => {
            store.dispatch(setCatalystRealmCommsStatus(status))
            switch (status.status) {
              case 'realm-full': {
                handleFullLayer()
                break
              }
              case 'reconnection-error': {
                handleReconnectionError()
                break
              }
            }
          }
        )

        break
      }
      default: {
        throw new Error(`unrecognized comms mode "${COMMS}"`)
      }
    }

    subscribeToRealmChange(store)

    context = new Context(userInfo)
    context.worldInstanceConnection = connection

    try {
      await connection.connectPeer()
    } catch (e) {
      // Do nothing if layer is full. This will be handled by status handler
      if (!(e.responseJson && e.responseJson.status === 'layer_is_full')) {
        throw e
      }
    }

    connection.positionHandler = (alias: string, data: Package<Position>) => {
      processPositionMessage(context!, alias, data)
    }
    connection.profileHandler = (alias: string, identity: string, data: Package<ProfileVersion>) => {
      processProfileMessage(context!, alias, identity, data)
    }
    connection.chatHandler = (alias: string, data: Package<ChatMessage>) => {
      processChatMessage(context!, alias, data)
    }
    connection.sceneMessageHandler = (alias: string, data: Package<BusMessage>) => {
      processParcelSceneCommsMessage(context!, alias, data)
    }

    if (commConfigurations.debug) {
      connection.stats = context.stats
    }

    context.profileInterval = setInterval(() => {
      if (context && context.currentPosition && context.worldInstanceConnection) {
        context.worldInstanceConnection
          .sendProfileMessage(context.currentPosition, context.userInfo)
          .catch(e => defaultLogger.warn(`error while sending message `, e))
      }
    }, 1000)

    if (commConfigurations.sendAnalytics) {
      context.analyticsInterval = setInterval(() => {
        const connectionAnalytics = connection.analyticsData()
        // We slice the ids in order to reduce the potential event size. Eventually, we should slice all comms ids
        connectionAnalytics.trackedPeers = context?.peerData.keys()
          ? [...context?.peerData.keys()].map(it => it.slice(-6))
          : []
        connectionAnalytics.visiblePeers = context?.stats.visiblePeerIds.map(it => it.slice(-6))

        if (connectionAnalytics) {
          queueTrackingEvent('Comms Status v2', connectionAnalytics)
        }
      }, 30000)
    }

    context.worldRunningObserver = worldRunningObservable.add(isRunning => {
      onWorldRunning(isRunning)
    })

    context.positionObserver = positionObservable.add((obj: Readonly<PositionReport>) => {
      const p = [
        obj.position.x,
        obj.position.y - obj.playerHeight,
        obj.position.z,
        obj.quaternion.x,
        obj.quaternion.y,
        obj.quaternion.z,
        obj.quaternion.w
      ] as Position

      if (context && isWorldRunning) {
        onPositionUpdate(context, p)
      }
    })

    window.addEventListener('beforeunload', () => sendToMordor())

    context.infoCollecterInterval = setInterval(() => {
      if (context) {
        collectInfo(context)
      }
    }, 100)

    return context
  } catch (e) {
    defaultLogger.error(e)
    if (e.message && e.message.includes('is taken')) {
      throw new IdTakenError(e.message)
    } else {
      throw new ConnectionEstablishmentError(e.message)
    }
  }
}

function handleReconnectionError() {
  const store: Store<RootState> = globalThis.globalStore
  const realm = getRealm(store.getState())

  if (realm) {
    store.dispatch(markCatalystRealmConnectionError(realm))
  }

  const candidates = getAllCatalystCandidates(store.getState())

  const otherRealm = pickCatalystRealm(candidates)

  const notificationMessage = realm
    ? `Lost connection to ${realmToString(realm)}, joining realm ${realmToString(otherRealm)} instead`
    : `Joining realm ${realmToString(otherRealm)}`

  notifyStatusThroughChat(notificationMessage)

  store.dispatch(setCatalystRealm(otherRealm))
}

function handleFullLayer() {
  const store: Store<RootState> = globalThis.globalStore
  const realm = getRealm(store.getState())

  if (realm) {
    store.dispatch(markCatalystRealmFull(realm))
  }

  const candidates = getAllCatalystCandidates(store.getState())

  const otherRealm = pickCatalystRealm(candidates)

  notifyStatusThroughChat(
    `Joining realm ${otherRealm.catalystName}-${otherRealm.layer} since the previously requested was full`
  )

  store.dispatch(setCatalystRealm(otherRealm))
}

export function onWorldRunning(isRunning: boolean, _context: Context | null = context) {
  if (!isRunning) {
    sendToMordor(_context)
  }
}

export function sendToMordor(_context: Context | null = context) {
  sendToMordorAsync().catch(e => defaultLogger.warn(`error while sending message `, e))
}

async function sendToMordorAsync(_context: Context | null = context) {
  if (_context && _context.worldInstanceConnection && _context.currentPosition) {
    await _context.worldInstanceConnection.sendParcelUpdateMessage(_context.currentPosition, MORDOR_POSITION)
  }
}

export function disconnect() {
  if (context) {
    if (context.profileInterval) {
      clearInterval(context.profileInterval)
    }
    if (context.infoCollecterInterval) {
      clearInterval(context.infoCollecterInterval)
    }
    if (context.analyticsInterval) {
      clearInterval(context.analyticsInterval)
    }
    if (context.positionObserver) {
      positionObservable.remove(context.positionObserver)
    }
    if (context.worldRunningObserver) {
      worldRunningObservable.remove(context.worldRunningObserver)
    }
    if (context.worldInstanceConnection) {
      context.worldInstanceConnection.close()
    }
  }
}

export async function fetchLayerUsersParcels(): Promise<ParcelArray[]> {
  const store: Store<RootState> = globalThis.globalStore
  const realm = getRealm(store.getState())
  const commsUrl = getCommsServer(store.getState())

  if (realm && realm.layer && commsUrl) {
    const layerUsersResponse = await fetch(`${commsUrl}/layers/${realm.layer}/users`)
    if (layerUsersResponse.ok) {
      const layerUsers: LayerUserInfo[] = await layerUsersResponse.json()
      return layerUsers.filter(it => it.parcel).map(it => it.parcel!)
    }
  }

  return []
}

globalThis.printCommsInformation = function() {
  if (context) {
    defaultLogger.log('Communication topics: ' + previousTopics)
    context.stats.printDebugInformation()
  }
}
