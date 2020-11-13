import { WorldInstanceConnection } from '../interface/index'
import { Stats } from '../debug'
import {
  Package,
  BusMessage,
  ChatMessage,
  ProfileVersion,
  UserInformation,
  PackageType,
  VoiceFragment,
  ProfileResponse,
  ProfileRequest
} from '../interface/types'
import { Position, positionHash } from '../interface/utils'
import defaultLogger, { createLogger } from 'shared/logger'
import { PeerMessageTypes, PeerMessageType } from 'decentraland-katalyst-peer/src/messageTypes'
import { Peer as PeerType } from 'decentraland-katalyst-peer/src/Peer'
import { PacketCallback } from 'decentraland-katalyst-peer/src/types'
import {
  ChatData,
  CommsMessage,
  ProfileData,
  SceneData,
  PositionData,
  VoiceData,
  ProfileRequestData,
  ProfileResponseData
} from './proto/comms_pb'
import { Realm, CommsStatus } from 'shared/dao/types'
import { compareVersions } from 'atomicHelpers/semverCompare'

import * as Long from 'long'
import { getProfileType } from 'shared/profiles/getProfileType'
import { Profile } from 'shared/types'
import { ProfileType } from 'shared/profiles/types'
import { EncodedFrame } from 'voice-chat-codec/types'
declare const window: any
window.Long = Long

const { Peer, buildCatalystPeerStatsData } = require('decentraland-katalyst-peer')

const NOOP = () => {
  // do nothing
}

const logger = createLogger('Lighthouse: ')

type MessageData =
  | ChatData
  | ProfileData
  | SceneData
  | PositionData
  | VoiceData
  | ProfileRequestData
  | ProfileResponseData

const commsMessageType: PeerMessageType = {
  name: 'sceneComms',
  ttl: 10,
  expirationTime: 10 * 1000,
  optimistic: true
}

const VoiceType: PeerMessageType = {
  name: 'voice',
  ttl: 5,
  optimistic: true,
  discardOlderThan: 2000,
  expirationTime: 10000
}

function ProfileRequestResponseType(action: 'request' | 'response'): PeerMessageType {
  return {
    name: 'profile_' + action,
    ttl: 10,
    optimistic: true,
    discardOlderThan: 0,
    expirationTime: 10000
  }
}

declare var global: any

export class LighthouseWorldInstanceConnection implements WorldInstanceConnection {
  stats: Stats | null = null

  sceneMessageHandler: (alias: string, data: Package<BusMessage>) => void = NOOP
  chatHandler: (alias: string, data: Package<ChatMessage>) => void = NOOP
  profileHandler: (alias: string, identity: string, data: Package<ProfileVersion>) => void = NOOP
  positionHandler: (alias: string, data: Package<Position>) => void = NOOP
  voiceHandler: (alias: string, data: Package<VoiceFragment>) => void = NOOP
  profileResponseHandler: (alias: string, data: Package<ProfileResponse>) => void = NOOP
  profileRequestHandler: (alias: string, data: Package<ProfileRequest>) => void = NOOP

  isAuthenticated: boolean = true // TODO - remove this

  ping: number = -1

  private peer: PeerType

  private rooms: string[] = []

  constructor(
    private peerId: string,
    private realm: Realm,
    private lighthouseUrl: string,
    private peerConfig: any,
    private statusHandler: (status: CommsStatus) => void
  ) {
    // This assignment is to "definetly initialize" peer
    this.peer = this.initializePeer()
  }

  async connectPeer() {
    try {
      await this.peer.awaitConnectionEstablished(60000)
      await this.peer.setLayer(this.realm.layer)
      this.statusHandler({ status: 'connected', connectedPeers: this.connectedPeersCount() })
    } catch (e) {
      defaultLogger.error('Error while connecting to layer', e)
      this.statusHandler({
        status: e.responseJson && e.responseJson.status === 'layer_is_full' ? 'realm-full' : 'error',
        connectedPeers: this.connectedPeersCount()
      })
      throw e
    }
  }

  public async changeRealm(realm: Realm, url: string) {
    this.statusHandler({ status: 'connecting', connectedPeers: this.connectedPeersCount() })
    if (this.peer) {
      await this.cleanUpPeer()
    }

    this.realm = realm
    this.lighthouseUrl = url

    this.initializePeer()
    await this.connectPeer()
    await this.syncRoomsWithPeer()
  }

  printDebugInformation() {
    // TODO - implement this - moliva - 20/12/2019
  }

  close() {
    return this.cleanUpPeer()
  }

  analyticsData() {
    return {
      stats: buildCatalystPeerStatsData(this.peer)
    }
  }

  async sendInitialMessage(userInfo: Partial<UserInformation>) {
    const topic = userInfo.userId!

    await this.sendProfileData(userInfo, topic, 'initialProfile')
  }

  async sendProfileMessage(currentPosition: Position, userInfo: UserInformation) {
    const topic = positionHash(currentPosition)

    await this.sendProfileData(userInfo, topic, 'profile')
  }

  async sendProfileRequest(currentPosition: Position, userId: string, version: number | undefined): Promise<void> {
    const topic = positionHash(currentPosition)

    const profileRequestData = new ProfileRequestData()
    profileRequestData.setUserId(userId)
    profileRequestData.setProfileVersion(version?.toString() ?? '')

    await this.sendData(topic, profileRequestData, ProfileRequestResponseType('request'))
  }

  async sendProfileResponse(currentPosition: Position, profile: Profile): Promise<void> {
    const topic = positionHash(currentPosition)

    const profileResponseData = new ProfileResponseData()
    profileResponseData.setSerializedProfile(JSON.stringify(profile))

    await this.sendData(topic, profileResponseData, ProfileRequestResponseType('response'))
  }

  async sendPositionMessage(p: Position) {
    const topic = positionHash(p)

    await this.sendPositionData(p, topic, 'position')
  }

  async sendParcelUpdateMessage(currentPosition: Position, p: Position) {
    const topic = positionHash(currentPosition)

    await this.sendPositionData(p, topic, 'parcelUpdate')
  }

  async sendParcelSceneCommsMessage(sceneId: string, message: string) {
    const topic = sceneId

    const sceneData = new SceneData()
    sceneData.setSceneId(sceneId)
    sceneData.setText(message)

    await this.sendData(topic, sceneData, commsMessageType)
  }

  async sendVoiceMessage(currentPosition: Position, frame: EncodedFrame): Promise<void> {
    const topic = positionHash(currentPosition)

    const voiceData = new VoiceData()
    voiceData.setEncodedSamples(frame.encoded)
    voiceData.setIndex(frame.index)

    await this.sendData(topic, voiceData, VoiceType)
  }

  async sendChatMessage(currentPosition: Position, messageId: string, text: string) {
    const topic = positionHash(currentPosition)

    const chatMessage = new ChatData()
    chatMessage.setMessageId(messageId)
    chatMessage.setText(text)

    await this.sendData(topic, chatMessage, PeerMessageTypes.reliable('chat'))
  }

  async updateSubscriptions(rooms: string[]) {
    this.rooms = rooms
    await this.syncRoomsWithPeer()
  }

  private async syncRoomsWithPeer() {
    const currentRooms = this.peer.currentRooms
    const joining = this.rooms.map((room) => {
      if (!currentRooms.some((current) => current.id === room)) {
        return this.peer.joinRoom(room)
      } else {
        return Promise.resolve()
      }
    })
    const leaving = currentRooms.map((current) => {
      if (!this.rooms.some((room) => current.id === room)) {
        return this.peer.leaveRoom(current.id)
      } else {
        return Promise.resolve()
      }
    })
    return Promise.all([...joining, ...leaving]).then(NOOP)
  }

  private async sendData(topic: string, messageData: MessageData, type: PeerMessageType) {
    await this.peer.sendMessage(topic, createCommsMessage(messageData).serializeBinary(), type)
  }

  private async sendPositionData(p: Position, topic: string, typeName: string) {
    const positionData = createPositionData(p)
    await this.sendData(topic, positionData, PeerMessageTypes.unreliable(typeName))
  }

  private async sendProfileData(userInfo: UserInformation, topic: string, typeName: string) {
    const profileData = createProfileData(userInfo)
    await this.sendData(topic, profileData, PeerMessageTypes.unreliable(typeName))
  }

  private initializePeer() {
    this.statusHandler({ status: 'connecting', connectedPeers: this.connectedPeersCount() })
    this.peer = this.createPeer()
    global.__DEBUG_PEER = this.peer
    return this.peer
  }

  private connectedPeersCount(): number {
    return this.peer ? this.peer.connectedCount() : 0
  }

  private createPeer(): PeerType {
    if (this.peerConfig.statusHandler) {
      logger.warn(`Overriding peer config status handler from client!`)
    }
    this.peerConfig.statusHandler = (status: string) => {
      if (status === 'reconnection-error') {
        this.statusHandler({ status, connectedPeers: this.connectedPeersCount() })
      }
    }

    // We require a version greater than 0.1 to not send an ID
    const idToUse = compareVersions('0.1', this.realm.lighthouseVersion) === -1 ? undefined : this.peerId

    return new Peer(this.lighthouseUrl, idToUse, this.peerCallback, this.peerConfig)
  }

  private async cleanUpPeer() {
    return this.peer.dispose()
  }

  private peerCallback: PacketCallback = (sender, room, payload, packet) => {
    try {
      const commsMessage = CommsMessage.deserializeBinary(payload)
      switch (commsMessage.getDataCase()) {
        case CommsMessage.DataCase.CHAT_DATA:
          this.chatHandler(sender, createPackage(commsMessage, 'chat', mapToPackageChat(commsMessage.getChatData()!)))
          break
        case CommsMessage.DataCase.POSITION_DATA:
          const positionMessage = mapToPositionMessage(commsMessage.getPositionData()!)
          this.peer.setPeerPosition(sender, positionMessage.slice(0, 3))
          this.positionHandler(sender, createPackage(commsMessage, 'position', positionMessage))
          break
        case CommsMessage.DataCase.SCENE_DATA:
          this.sceneMessageHandler(
            sender,
            createPackage(commsMessage, 'chat', mapToPackageScene(commsMessage.getSceneData()!))
          )
          break
        case CommsMessage.DataCase.PROFILE_DATA:
          this.profileHandler(
            sender,
            commsMessage.getProfileData()!.getUserId(),
            createPackage(commsMessage, 'profile', mapToPackageProfile(commsMessage.getProfileData()!))
          )
          break
        case CommsMessage.DataCase.VOICE_DATA:
          this.voiceHandler(
            sender,
            createPackage(
              commsMessage,
              'voice',
              mapToPackageVoice(
                commsMessage.getVoiceData()!.getEncodedSamples_asU8(),
                commsMessage.getVoiceData()!.getIndex(),
                packet.sequenceId
              )
            )
          )
          break
        case CommsMessage.DataCase.PROFILE_REQUEST_DATA:
          this.profileRequestHandler(
            sender,
            createPackage(
              commsMessage,
              'profileRequest',
              mapToPackageProfileRequest(commsMessage.getProfileRequestData()!)
            )
          )
          break
        case CommsMessage.DataCase.PROFILE_RESPONSE_DATA:
          this.profileResponseHandler(
            sender,
            createPackage(
              commsMessage,
              'profileResponse',
              mapToPackageProfileResponse(commsMessage.getProfileResponseData()!)
            )
          )
          break
        default: {
          logger.warn(`message with unknown type received ${commsMessage.getDataCase()}`)
          break
        }
      }
    } catch (e) {
      logger.error(`Error processing received message from ${sender}. Topic: ${room}`, e)
    }
  }
}

function createPackage<T>(commsMessage: CommsMessage, type: PackageType, data: T): Package<T> {
  return {
    time: commsMessage.getTime(),
    type,
    data
  }
}

function mapToPositionMessage(positionData: PositionData): Position {
  return [
    positionData.getPositionX(),
    positionData.getPositionY(),
    positionData.getPositionZ(),
    positionData.getRotationX(),
    positionData.getRotationY(),
    positionData.getRotationZ(),
    positionData.getRotationW(),
    positionData.getImmediate()
  ]
}

function mapToPackageChat(chatData: ChatData) {
  return {
    id: chatData.getMessageId(),
    text: chatData.getText()
  }
}

function mapToPackageScene(sceneData: SceneData) {
  return {
    id: sceneData.getSceneId(),
    text: sceneData.getText()
  }
}

function mapToPackageProfile(profileData: ProfileData) {
  return {
    user: profileData.getUserId(),
    version: profileData.getProfileVersion(),
    type: mapToPackageProfileType(profileData.getProfileType())
  }
}

function mapToPackageProfileType(profileType: ProfileType) {
  return profileType === ProfileData.ProfileType.LOCAL ? ProfileType.LOCAL : ProfileType.DEPLOYED
}

function mapToPackageProfileRequest(profileRequestData: ProfileRequestData) {
  const versionData = profileRequestData.getProfileVersion()
  return {
    userId: profileRequestData.getUserId(),
    version: versionData !== '' ? versionData : undefined
  }
}

function mapToPackageProfileResponse(profileResponseData: ProfileResponseData) {
  return {
    profile: JSON.parse(profileResponseData.getSerializedProfile()) as Profile
  }
}

function mapToPackageVoice(encoded: Uint8Array, index: number, fallbackIndex: number) {
  // If we receive a packet from an old implementation of voice chat, we use the fallbackIndex
  return { encoded, index: index === 0 ? fallbackIndex : index }
}

function createProfileData(userInfo: UserInformation) {
  const profileData = new ProfileData()
  profileData.setProfileVersion(userInfo.version ? userInfo.version.toString() : '')
  profileData.setUserId(userInfo.userId ? userInfo.userId : '')
  profileData.setProfileType(getProtobufProfileType(getProfileType(userInfo.identity)))
  return profileData
}

function getProtobufProfileType(profileType: ProfileType) {
  return profileType === ProfileType.LOCAL ? ProfileData.ProfileType.LOCAL : ProfileData.ProfileType.DEPLOYED
}

function createPositionData(p: Position) {
  const positionData = new PositionData()
  positionData.setPositionX(p[0])
  positionData.setPositionY(p[1])
  positionData.setPositionZ(p[2])
  positionData.setRotationX(p[3])
  positionData.setRotationY(p[4])
  positionData.setRotationZ(p[5])
  positionData.setRotationW(p[6])
  positionData.setImmediate(p[7])
  return positionData
}

function createCommsMessage(data: MessageData) {
  const commsMessage = new CommsMessage()
  commsMessage.setTime(Date.now())

  if (data instanceof ChatData) commsMessage.setChatData(data)
  if (data instanceof SceneData) commsMessage.setSceneData(data)
  if (data instanceof ProfileData) commsMessage.setProfileData(data)
  if (data instanceof PositionData) commsMessage.setPositionData(data)
  if (data instanceof VoiceData) commsMessage.setVoiceData(data)
  if (data instanceof ProfileRequestData) commsMessage.setProfileRequestData(data)
  if (data instanceof ProfileResponseData) commsMessage.setProfileResponseData(data)

  return commsMessage
}
