import * as proto from 'shared/protocol/decentraland/kernel/comms/rfc4/comms.gen'
import type { Avatar } from '@dcl/schemas'
import { uuid } from 'lib/javascript/uuid'
import { Observable } from 'mz-observable'
import { eventChannel } from 'redux-saga'
import { getBannedUsers } from 'shared/meta/selectors'
import { validateAvatar } from 'shared/profiles/schemaValidation'
import { getCurrentUserProfile } from 'shared/profiles/selectors'
import { ensureAvatarCompatibilityFormat } from 'lib/decentraland/profiles/transformations/profileToServerFormat'
import { incrementCommsMessageReceived, incrementCommsMessageReceivedByName } from 'shared/session/getPerformanceInfo'
import { getCurrentUserId } from 'shared/session/selectors'
import { store } from 'shared/store/isolatedStore'
import { ChatMessage as InternalChatMessage, ChatMessageType } from 'shared/types'
import { isBlockedOrBanned } from 'shared/voiceChat/selectors'
import { messageReceived } from '../chat/actions'
import { handleRoomDisconnection } from './actions'
import { AdapterDisconnectedEvent } from './adapters/types'
import { RoomConnection } from './interface'
import { AvatarMessageType, Package } from './interface/types'
import {
  avatarMessageObservable,
  getPeer,
  onRoomLeft,
  receiveUserPosition,
  onPeerDisconnected,
  setupPeer as setupPeerTrackingInfo
} from './peers'
import { scenesSubscribedToCommsEvents } from './sceneSubscriptions'
import { globalObservable } from 'shared/observables'
import { BringDownClientAndShowError } from 'shared/loading/ReportFatalError'

type VersionUpdateInformation = {
  userId: string
  version: number
}

const versionUpdateOverCommsChannel = new Observable<VersionUpdateInformation>()
const receiveProfileOverCommsChannel = new Observable<Avatar>()
const sendMyProfileOverCommsChannel = new Observable<Record<string, never>>()

export async function bindHandlersToCommsContext(room: RoomConnection) {
  // RFC4 messages
  room.events.on('position', (e) => processPositionMessage(room, e))
  room.events.on('profileMessage', processProfileUpdatedMessage)
  room.events.on('chatMessage', processChatMessage)
  room.events.on('sceneMessageBus', processParcelSceneCommsMessage)
  room.events.on('profileRequest', processProfileRequest)
  room.events.on('profileResponse', processProfileResponse)

  room.events.on('*', (type, _) => {
    incrementCommsMessageReceived()
    incrementCommsMessageReceivedByName(type)
  })

  // transport messages
  room.events.on('PEER_DISCONNECTED', onPeerDisconnected)
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

async function handleDisconnectionEvent(data: AdapterDisconnectedEvent, room: RoomConnection) {
  try {
    await onRoomLeft(room)
  } catch (err) {
    console.error(err)
    // TODO: handle this
  }

  store.dispatch(handleRoomDisconnection(room))

  // when we are kicked, the explorer should re-load, or maybe go to offline~offline realm
  if (data.kicked) {
    BringDownClientAndShowError(
      'Disconnected from realm as the user id is already taken.' +
        'Please make sure you are not logged into the world through another tab'
    )
  }
}

// TODO: use position message to setupPeerTrackingInfo
function processProfileUpdatedMessage(message: Package<proto.AnnounceProfileVersion>) {
  const peerTrackingInfo = setupPeerTrackingInfo(message.address)
  peerTrackingInfo.ethereumAddress = message.address
  peerTrackingInfo.lastUpdate = Date.now()

  if (message.data.profileVersion > peerTrackingInfo.lastProfileVersion) {
    peerTrackingInfo.lastProfileVersion = message.data.profileVersion

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
    if (message.data.message.startsWith('␆') /* pong */ || message.data.message.startsWith('␑') /* ping */) {
      // TODO: remove this
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
  profile.userId = message.address
  profile.ethAddress = message.address
  peerTrackingInfo.lastProfileVersion = profile.version
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

function processPositionMessage(room: RoomConnection, message: Package<proto.Position>) {
  const peer = receiveUserPosition(message.address, message.data)
  if (peer?.lastProfileVersion === -1) {
    room.sendProfileRequest({
      address: message.address,
      profileVersion: 0
    })
  }
}
