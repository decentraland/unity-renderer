import * as proto from 'shared/protocol/decentraland/kernel/comms/rfc4/comms.gen'
import type { Avatar } from '@dcl/schemas'
import { uuid } from 'lib/javascript/uuid'
import { Observable } from 'mz-observable'
import { eventChannel } from 'redux-saga'
import { getBannedUsers } from 'shared/meta/selectors'
import { incrementCounter } from 'shared/analytics/occurences'
import { validateAvatar } from 'shared/profiles/schemaValidation'
import { getCurrentUserProfile } from 'shared/profiles/selectors'
import { ensureAvatarCompatibilityFormat } from 'lib/decentraland/profiles/transformations/profileToServerFormat'
import { incrementCommsMessageReceived, incrementCommsMessageReceivedByName } from 'shared/session/getPerformanceInfo'
import { getCurrentUserId } from 'shared/session/selectors'
import { store } from 'shared/store/isolatedStore'
import { ChatMessage as InternalChatMessage, ChatMessageType } from 'shared/types'
import { processVoiceFragment } from 'shared/voiceChat/handlers'
import { isBlockedOrBanned } from 'shared/voiceChat/selectors'
import { sendPublicChatMessage } from '.'
import { messageReceived } from '../chat/actions'
import { handleRoomDisconnection } from './actions'
import { AdapterDisconnectedEvent, PeerDisconnectedEvent } from './adapters/types'
import { RoomConnection } from './interface'
import { AvatarMessageType, Package } from './interface/types'
import {
  avatarMessageObservable,
  ensureTrackingUniqueAndLatest,
  getPeer,
  receiveUserPosition,
  removeAllPeers,
  removePeerByAddress,
  setupPeer as setupPeerTrackingInfo
} from './peers'
import { scenesSubscribedToCommsEvents } from './sceneSubscriptions'
import { globalObservable } from 'shared/observables'
import { BringDownClientAndShowError } from 'shared/loading/ReportFatalError'

type PingRequest = {
  alias: number
  sentTime: number
  onPong: (dt: number, address: string) => void
}
type VersionUpdateInformation = {
  userId: string
  version: number
}

const versionUpdateOverCommsChannel = new Observable<VersionUpdateInformation>()
const receiveProfileOverCommsChannel = new Observable<Avatar>()
const sendMyProfileOverCommsChannel = new Observable<Record<string, never>>()
const pingRequests = new Map<number, PingRequest>()
let pingIndex = 0

export async function bindHandlersToCommsContext(room: RoomConnection) {
  removeAllPeers()
  pingRequests.clear()

  // RFC4 messages
  room.events.on('position', processPositionMessage)
  room.events.on('profileMessage', processProfileUpdatedMessage)
  room.events.on('chatMessage', processChatMessage)
  room.events.on('sceneMessageBus', processParcelSceneCommsMessage)
  room.events.on('profileRequest', processProfileRequest)
  room.events.on('profileResponse', processProfileResponse)
  room.events.on('voiceMessage', processVoiceFragment)

  room.events.on('*', (type, _) => {
    incrementCommsMessageReceived()
    incrementCommsMessageReceivedByName(type)
  })

  // transport messages
  room.events.on('PEER_DISCONNECTED', handleDisconnectPeer)
  room.events.on('DISCONNECTION', (event) => handleDisconnectionEvent(event, room))
}

/**
 *
 * @returns true if the user is connected, false if not
 */
export async function requestProfileFromPeers(
  roomConnection: RoomConnection,
  address: string,
  profileVersion: number
): Promise<boolean> {
  if (getPeer(address)) {
    await roomConnection.sendProfileRequest({
      address,
      profileVersion
    })
    return true
  }
  return false
}

function handleDisconnectionEvent(data: AdapterDisconnectedEvent, room: RoomConnection) {
  store.dispatch(handleRoomDisconnection(room))

  // when we are kicked, the explorer should re-load, or maybe go to offline~offline realm
  if (data.kicked) {
    BringDownClientAndShowError(
      'Disconnected from realm as the user id is already taken.' +
        'Please make sure you are not logged into the world through another tab'
    )
  }
}

function handleDisconnectPeer(data: PeerDisconnectedEvent) {
  removePeerByAddress(data.address)
}

function processProfileUpdatedMessage(message: Package<proto.AnnounceProfileVersion>) {
  const peerTrackingInfo = setupPeerTrackingInfo(message.address)
  peerTrackingInfo.ethereumAddress = message.address
  peerTrackingInfo.lastUpdate = Date.now()

  if (message.data.profileVersion > peerTrackingInfo.lastProfileVersion) {
    peerTrackingInfo.lastProfileVersion = message.data.profileVersion

    // remove duplicates
    ensureTrackingUniqueAndLatest(peerTrackingInfo)
    versionUpdateOverCommsChannel.notifyObservers({ userId: message.address, version: message.data.profileVersion })
  }
}

function processParcelSceneCommsMessage(message: Package<proto.Scene>) {
  const peer = getPeer(message.address)

  if (peer) {
    const { sceneId, data } = message.data
    scenesSubscribedToCommsEvents.forEach(($) => {
      if ($.cid === sceneId) {
        $.receiveCommsMessage(data, peer)
      }
    })
  }
}

function pingMessage(nonce: number) {
  return `␑${nonce}`
}
function pongMessage(nonce: number, address: string) {
  return `␆${nonce} ${address}`
}

export function sendPing(onPong?: (dt: number, address: string) => void) {
  const nonce = Math.floor(Math.random() * 0xffffffff)
  let responses = 0
  pingRequests.set(nonce, {
    sentTime: Date.now(),
    alias: pingIndex++,
    onPong:
      onPong ||
      ((dt, address) => {
        console.log(
          `ping got ${++responses} responses (ping: ${dt.toFixed(2)}ms, nonce: ${nonce}, address: ${address})`
        )
      })
  })
  sendPublicChatMessage(pingMessage(nonce))
  incrementCounter('ping_sent_counter')
}

const answeredPings = new Set<number>()

function processChatMessage(message: Package<proto.Chat>) {
  const myProfile = getCurrentUserProfile(store.getState())
  const fromAlias: string = message.address
  if (!fromAlias) {
    globalObservable.emit('error', {
      error: new Error(`Unexpected message without address: ${JSON.stringify(message)}`),
      code: 'comms',
      level: 'warning'
    })
    return
  }
  const senderPeer = setupPeerTrackingInfo(fromAlias)

  senderPeer.lastUpdate = Date.now()

  if (senderPeer.ethereumAddress) {
    if (message.data.message.startsWith('␆') /* pong */) {
      const [nonceStr, address] = message.data.message.slice(1).split(' ')
      const nonce = parseInt(nonceStr, 10)
      const request = pingRequests.get(nonce)
      if (request) {
        incrementCounter('pong_received_counter')
        request.onPong(Date.now() - request.sentTime, address || 'none')
      }
    } else if (message.data.message.startsWith('␑') /* ping */) {
      const nonce = parseInt(message.data.message.slice(1), 10)
      if (answeredPings.has(nonce)) {
        incrementCounter('ping_received_twice_counter')
        return
      }
      answeredPings.add(nonce)
      if (myProfile) sendPublicChatMessage(pongMessage(nonce, myProfile.ethAddress))
      incrementCounter('pong_sent_counter')
    } else if (message.data.message.startsWith('␐')) {
      const [id, timestamp] = message.data.message.split(' ')

      avatarMessageObservable.notifyObservers({
        type: AvatarMessageType.USER_EXPRESSION,
        userId: senderPeer.ethereumAddress,
        expressionId: id.slice(1),
        timestamp: parseInt(timestamp, 10)
      })
    } else {
      const isBanned =
        !myProfile ||
        (senderPeer.ethereumAddress &&
          isBlockedOrBanned(myProfile, getBannedUsers(store.getState()), senderPeer.ethereumAddress)) ||
        false

      if (!isBanned) {
        const messageEntry: InternalChatMessage = {
          messageType: ChatMessageType.PUBLIC,
          messageId: uuid(),
          sender: senderPeer.ethereumAddress,
          body: message.data.message,
          timestamp: message.data.timestamp
        }
        store.dispatch(messageReceived(messageEntry))
      }
    }
  }
}

// Receive a "rpc" signal over comms to send our profile
function processProfileRequest(message: Package<proto.ProfileRequest>) {
  const myAddress = getCurrentUserId(store.getState())

  // We only send profile responses for our own address
  if (message.data.address.toLowerCase() === myAddress?.toLowerCase()) {
    sendMyProfileOverCommsChannel.notifyObservers({})
  }
}

function processProfileResponse(message: Package<proto.ProfileResponse>) {
  const peerTrackingInfo = setupPeerTrackingInfo(message.address)

  const profile = ensureAvatarCompatibilityFormat(JSON.parse(message.data.serializedProfile))

  if (!validateAvatar(profile)) {
    console.trace('Invalid avatar received', validateAvatar.errors)
    debugger
  }

  if (peerTrackingInfo.ethereumAddress !== profile.userId) {
    debugger
    return
  }

  receiveProfileOverCommsChannel.notifyObservers(profile)
}

export function createSendMyProfileOverCommsChannel() {
  return eventChannel<Record<string, never>>((emitter) => {
    const listener = sendMyProfileOverCommsChannel.add(emitter)
    return () => {
      sendMyProfileOverCommsChannel.remove(listener)
    }
  })
}

export function createVersionUpdateOverCommsChannel() {
  return eventChannel<VersionUpdateInformation>((emitter) => {
    const listener = versionUpdateOverCommsChannel.add(emitter)
    return () => {
      versionUpdateOverCommsChannel.remove(listener)
    }
  })
}

export function createReceiveProfileOverCommsChannel() {
  return eventChannel<Avatar>((emitter) => {
    const listener = receiveProfileOverCommsChannel.add(emitter)
    return () => {
      receiveProfileOverCommsChannel.remove(listener)
    }
  })
}

function processPositionMessage(message: Package<proto.Position>) {
  receiveUserPosition(message.address, message.data)
}
