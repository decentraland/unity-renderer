import { COMMS_PROFILE_TIMEOUT } from 'config'
import { ChatMessage as InternalChatMessage, ChatMessageType } from 'shared/types'
import {
  getPeer,
  avatarMessageObservable,
  setupPeer,
  ensureTrackingUniqueAndLatest,
  receiveUserPosition,
  removeAllPeers,
  removePeerByAddress,
  receivePeerUserData
} from './peers'
import { AvatarMessageType, Package } from './interface/types'
import * as proto from '@dcl/protocol/out-ts/decentraland/kernel/comms/rfc4/comms.gen'
import { store } from 'shared/store/isolatedStore'
import { getCurrentUserProfile } from 'shared/profiles/selectors'
import { messageReceived } from '../chat/actions'
import { getBannedUsers } from 'shared/meta/selectors'
import { processVoiceFragment } from 'shared/voiceChat/handlers'
import future, { IFuture } from 'fp-future'
import { handleRoomDisconnection } from './actions'
import { Avatar } from '@dcl/schemas'
import { Observable } from 'mz-observable'
import { eventChannel } from 'redux-saga'
import { ProfileAsPromise } from 'shared/profiles/ProfileAsPromise'
import { trackEvent } from 'shared/analytics'
import { ProfileType } from 'shared/profiles/types'
import { ensureAvatarCompatibilityFormat } from 'shared/profiles/transformations/profileToServerFormat'
import { scenesSubscribedToCommsEvents } from './sceneSubscriptions'
import { isBlockedOrBanned } from 'shared/voiceChat/selectors'
import { uuid } from 'atomicHelpers/math'
import { validateAvatar } from 'shared/profiles/schemaValidation'
import { AdapterDisconnectedEvent, PeerDisconnectedEvent } from './adapters/types'
import { RoomConnection } from './interface'
import { incrementCommsMessageReceived, incrementCommsMessageReceivedByName } from 'shared/session/getPerformanceInfo'
import { sendPublicChatMessage } from '.'
import { getCurrentIdentity } from 'shared/session/selectors'
import { commsLogger } from './context'
import { incrementCounter } from 'shared/occurences'
import { ensureRealmAdapterPromise, getFetchContentUrlPrefixFromRealmAdapter } from 'shared/realm/selectors'

type PingRequest = {
  alias: number
  sentTime: number
  onPong: (dt: number, address: string) => void
}

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

const pendingProfileRequests: Map<string, Set<IFuture<Avatar | null>>> = new Map()

export async function requestProfileToPeers(
  roomConnection: RoomConnection,
  address: string,
  profileVersion: number
): Promise<Avatar | null> {
  if (!pendingProfileRequests.has(address)) {
    pendingProfileRequests.set(address, new Set())
  }

  const thisFuture = future<Avatar | null>()

  pendingProfileRequests.get(address)!.add(thisFuture)

  void thisFuture.then((value) => {
    incrementCounter(value ? 'profile-over-comms-succesful' : 'profile-over-comms-failed')
  })

  // send the request
  await roomConnection.sendProfileRequest({
    address,
    profileVersion
  })

  // send another retry in a couple seconds
  setTimeout(function () {
    if (thisFuture.isPending) {
      roomConnection
        .sendProfileRequest({
          address,
          profileVersion
        })
        .catch(commsLogger.error)
    }
  }, COMMS_PROFILE_TIMEOUT / 3)

  // send another retry in a couple seconds
  setTimeout(function () {
    if (thisFuture.isPending) {
      roomConnection
        .sendProfileRequest({
          address,
          profileVersion
        })
        .catch(commsLogger.error)
    }
  }, COMMS_PROFILE_TIMEOUT / 2)

  // and lastly fail
  setTimeout(function () {
    if (thisFuture.isPending) {
      // We resolve with a null profile. This will fallback to a random profile
      thisFuture.resolve(null)
      const pendingRequests = pendingProfileRequests.get(address)
      if (pendingRequests) {
        pendingRequests.delete(thisFuture)
      }
    }
  }, COMMS_PROFILE_TIMEOUT)

  return thisFuture
}

function handleDisconnectionEvent(data: AdapterDisconnectedEvent, room: RoomConnection) {
  store.dispatch(handleRoomDisconnection(room))
  // when we are kicked, the explorer should re-load, or maybe go to offline~offline realm
  if (data.kicked) {
    const url = new URL(document.location.toString())
    url.search = ''
    url.searchParams.set('disconnection-reason', 'logged-in-somewhere-else')
    document.location = url.toString()
  }
}

function handleDisconnectPeer(data: PeerDisconnectedEvent) {
  removePeerByAddress(data.address)
}

function processProfileUpdatedMessage(message: Package<proto.AnnounceProfileVersion>) {
  const peerTrackingInfo = setupPeer(message.address)
  peerTrackingInfo.ethereumAddress = message.address
  peerTrackingInfo.lastUpdate = Date.now()

  if (message.data.profileVersion > peerTrackingInfo.lastProfileVersion) {
    peerTrackingInfo.lastProfileVersion = message.data.profileVersion

    // remove duplicates
    ensureTrackingUniqueAndLatest(peerTrackingInfo)

    const profileVersion = +message.data.profileVersion

    ProfileAsPromise(
      message.address,
      profileVersion,
      /* we ask for LOCAL to ask information about the profile using comms to not overload the servers*/
      ProfileType.LOCAL
    )
      .then(async (avatar) => {
        // send to Avatars scene
        const realmAdapter = await ensureRealmAdapterPromise()
        const fetchContentServerWithPrefix = getFetchContentUrlPrefixFromRealmAdapter(realmAdapter)
        receivePeerUserData(avatar, peerTrackingInfo.baseUrl || fetchContentServerWithPrefix)
      })
      .catch((e: Error) => {
        trackEvent('error', {
          message: `error loading profile ${message.address}:${profileVersion}: ` + e.message,
          context: 'kernel#saga',
          stack: e.stack || 'processProfileUpdatedMessage'
        })
      })
  }
}

// TODO: Change ChatData to the new class once it is added to the .proto
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

globalThis.__sendPing = sendPing

const answeredPings = new Set<number>()

function processChatMessage(message: Package<proto.Chat>) {
  const myProfile = getCurrentUserProfile(store.getState())
  const fromAlias: string = message.address
  const senderPeer = setupPeer(fromAlias)

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
  const myIdentity = getCurrentIdentity(store.getState())
  const myAddress = myIdentity?.address

  // We only send profile responses for our own address
  if (message.data.address.toLowerCase() === myAddress?.toLowerCase()) {
    sendMyProfileOverCommsChannel.notifyObservers({})
  }
}

function processProfileResponse(message: Package<proto.ProfileResponse>) {
  const peerTrackingInfo = setupPeer(message.address)

  const profile = ensureAvatarCompatibilityFormat(JSON.parse(message.data.serializedProfile))

  if (!validateAvatar(profile)) {
    console.error('Invalid avatar received', validateAvatar.errors)
    debugger
  }

  if (peerTrackingInfo.ethereumAddress !== profile.userId) {
    debugger
    return
  }

  const promises = pendingProfileRequests.get(profile.userId)

  if (promises?.size) {
    promises.forEach((it) => it.resolve(profile))
    pendingProfileRequests.delete(profile.userId)
  }

  // If we received an unexpected profile, maybe the profile saga can use this preemptively
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
