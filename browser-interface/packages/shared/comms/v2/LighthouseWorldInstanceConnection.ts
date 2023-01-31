import { CommsEvents, RoomConnection } from '../interface/index'
import { Package } from '../interface/types'
import { CommunicationArea, position2parcelRfc4, positionHashRfc4 } from '../interface/utils'
import {
  Peer as IslandBasedPeer,
  PeerConfig,
  PacketCallback,
  PeerStatus,
  PeerMessageType,
  PeerMessageTypes,
  MinPeerData
} from '@dcl/catalyst-peer'
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
import { CommsStatus } from '../types'

import { ProfileType } from 'shared/profiles/types'
import * as rfc4 from '@dcl/protocol/out-ts/decentraland/kernel/comms/rfc4/comms.gen'
import mitt from 'mitt'
import { createLogger } from 'shared/logger'
import { ExplorerIdentity } from 'shared/session/types'
import { uuid } from 'atomicHelpers/math'
import { commConfigurations, parcelLimits } from 'config'
import { peerIdHandler } from '../logic/peer-id-handler'
import { Observable } from 'mz-observable'
import { AdapterDisconnectedEvent } from '../adapters/types'
import { VoiceHandler } from 'shared/voiceChat/VoiceHandler'
import { createOpusVoiceHandler } from '../adapters/voice/opusVoiceHandler'

type PeerType = IslandBasedPeer

const logger = createLogger('Lighthouse: ')

type MessageData =
  | ChatData
  | ProfileData
  | SceneData
  | PositionData
  | VoiceData
  | ProfileRequestData
  | ProfileResponseData

export type LighthouseConnectionConfig = PeerConfig & {
  preferedIslandId?: string
}

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

declare let globalThis: any

const previousLighthouseConnections = new Set<LighthouseWorldInstanceConnection>()

export class LighthouseWorldInstanceConnection implements RoomConnection {
  events = mitt<CommsEvents>()

  protected peer: PeerType

  private rooms: string[] = []
  private disposed = false
  private lastPosition: rfc4.Position | null = null
  private profileType: ProfileType
  private peerIdAdapter = peerIdHandler({ events: this.events })
  public onIslandChangedObservable = new Observable<{ island: string; peers: MinPeerData[] }>()
  currentIsland: string | undefined

  constructor(
    private lighthouseUrl: string,
    private peerConfig: LighthouseConnectionConfig,
    private statusHandler: (status: CommsStatus) => void,
    private identity: ExplorerIdentity
  ) {
    this.profileType = identity.hasConnectedWeb3 ? ProfileType.DEPLOYED : ProfileType.LOCAL
    // This assignment is to "definetly initialize" peer
    this.peer = this.initializePeer()
    previousLighthouseConnections.add(this)
  }

  async getVoiceHandler(): Promise<VoiceHandler> {
    return createOpusVoiceHandler()
  }

  async connect() {
    try {
      for (const connection of previousLighthouseConnections) {
        if (connection !== this) {
          await connection.disconnect()
        }
      }

      if (!this.peer.connectedCount()) {
        await this.peer.awaitConnectionEstablished(60000)
      }
      this.statusHandler({ status: 'connected', connectedPeers: this.connectedPeersCount() })
      await this.syncRoomsWithPeer()
    } catch (e: any) {
      logger.error('Error while connecting to layer', e)
      this.statusHandler({
        status: e.responseJson && e.responseJson.status === 'layer_is_full' ? 'realm-full' : 'error',
        connectedPeers: this.connectedPeersCount()
      })
      await this.disconnect()
    }
  }

  async disconnect(data?: AdapterDisconnectedEvent) {
    if (this.disposed) return
    this.disposed = true

    await this.peer.dispose()

    previousLighthouseConnections.delete(this)

    this.events.emit('DISCONNECTION', data ?? { kicked: false })
  }

  async sendProfileMessage(profile: rfc4.AnnounceProfileVersion) {
    if (this.lastPosition) {
      const topic = positionHashRfc4(this.lastPosition)

      await this.sendProfileData(this.identity.address, this.profileType, profile.profileVersion, topic, 'profile')
    }
  }

  async sendProfileRequest(profileRequest: rfc4.ProfileRequest): Promise<void> {
    if (this.lastPosition) {
      const topic = positionHashRfc4(this.lastPosition)

      const profileRequestData = new ProfileRequestData()
      profileRequestData.setUserId(profileRequest.address)
      profileRequestData.setProfileVersion(profileRequest.profileVersion.toString() ?? '')

      await this.sendData(topic, profileRequestData, ProfileRequestResponseType('request'))
    }
  }

  async sendProfileResponse(profileResponse: rfc4.ProfileResponse): Promise<void> {
    if (this.lastPosition) {
      const topic = positionHashRfc4(this.lastPosition)

      const profileResponseData = new ProfileResponseData()
      profileResponseData.setSerializedProfile(profileResponse.serializedProfile)

      await this.sendData(topic, profileResponseData, ProfileRequestResponseType('response'))
    }
  }

  async sendPositionMessage(p: rfc4.Position) {
    const topic = positionHashRfc4(p)
    this.lastPosition = p

    await this.sendPositionData(p, topic, 'position')
  }

  async sendParcelSceneMessage(message: rfc4.Scene): Promise<void> {
    if (this.currentIsland) {
      const sceneData = new SceneData()
      sceneData.setSceneId(message.sceneId)
      const text = new TextDecoder().decode(message.data)
      sceneData.setText(text)

      await this.sendData(this.currentIsland, sceneData, commsMessageType)
    }
  }

  async sendVoiceMessage(voice: rfc4.Voice): Promise<void> {
    if (this.lastPosition) {
      const topic = positionHashRfc4(this.lastPosition)

      const voiceData = new VoiceData()
      voiceData.setEncodedSamples(voice.encodedSamples)
      voiceData.setIndex(voice.index)

      await this.sendData(topic, voiceData, VoiceType)
    }
  }

  async sendChatMessage(chat: rfc4.Chat) {
    if (this.lastPosition) {
      const topic = positionHashRfc4(this.lastPosition)

      const chatMessage = new ChatData()
      chatMessage.setMessageId(uuid())
      chatMessage.setText(chat.message)

      await this.sendData(topic, chatMessage, PeerMessageTypes.reliable('chat'))
    }
  }

  private async syncRoomsWithPeer() {
    const currentRooms = [...this.peer.currentRooms]

    function isSameRoom(roomId: string, roomIdOrObject: string) {
      return roomIdOrObject === roomId
    }

    const joining = this.rooms.map((room) => {
      if (!currentRooms.some((current) => isSameRoom(room, current))) {
        return this.peer.joinRoom(room)
      } else {
        return Promise.resolve()
      }
    })
    const leaving = currentRooms.map((current) => {
      if (!this.rooms.some((room) => isSameRoom(room, current))) {
        return this.peer.leaveRoom(current)
      } else {
        return Promise.resolve()
      }
    })
    return Promise.all([...joining, ...leaving])
  }

  private async sendData(topic: string, messageData: MessageData, type: PeerMessageType) {
    if (this.disposed) {
      return
    }
    try {
      await this.peer.sendMessage(topic, createCommsMessage(messageData).serializeBinary(), type)
    } catch (e: any) {
      const message = e.message
      if (typeof message === 'string' && message.startsWith('cannot send a message in a room not joined')) {
        // We can ignore this error. This is usually just a problem of eventual consistency.
        // And when it is not, it is usually caused by another error that we might find above. Effectively, we are just making noise.
      } else {
        throw e
      }
    }
  }

  private async sendPositionData(p: rfc4.Position, topic: string, typeName: string) {
    const positionData = createPositionData(p)
    await this.sendData(topic, positionData, PeerMessageTypes.unreliable(typeName))

    await this.refreshTopics()
  }

  refreshTopics() {
    this.rooms.length = 0
    if (this.currentIsland) this.rooms.push(this.currentIsland)

    if (this.lastPosition) {
      const newParcel = position2parcelRfc4(this.lastPosition)
      const commArea = new CommunicationArea(newParcel, commConfigurations.commRadius)

      const xMin = ((commArea.vMin.x + parcelLimits.maxParcelX) >> 2) << 2
      const xMax = ((commArea.vMax.x + parcelLimits.maxParcelX) >> 2) << 2
      const zMin = ((commArea.vMin.y + parcelLimits.maxParcelZ) >> 2) << 2
      const zMax = ((commArea.vMax.y + parcelLimits.maxParcelZ) >> 2) << 2

      for (let x = xMin; x <= xMax; x += 4) {
        for (let z = zMin; z <= zMax; z += 4) {
          const hash = `${x >> 2}:${z >> 2}`
          if (!this.rooms.includes(hash)) {
            this.rooms.push(hash)
          }
        }
      }
    }
    return this.syncRoomsWithPeer()
  }

  private async sendProfileData(
    address: string,
    profileType: ProfileType,
    version: number,
    topic: string,
    typeName: string
  ) {
    const profileData = createProfileData(address, profileType, version)
    await this.sendData(topic, profileData, PeerMessageTypes.unreliable(typeName))
  }

  private initializePeer() {
    this.statusHandler({ status: 'connecting', connectedPeers: this.connectedPeersCount() })
    this.peer = this.createPeer()
    globalThis.__DEBUG_PEER = this.peer

    if (this.peerConfig.preferedIslandId) {
      this.peer.setPreferedIslandId(this.peerConfig.preferedIslandId)
    }

    return this.peer
  }

  private connectedPeersCount(): number {
    return this.peer ? this.peer.connectedCount() : 0
  }

  private createPeer(): PeerType {
    const statusHandler = (status: PeerStatus): void =>
      this.statusHandler({ status, connectedPeers: this.connectedPeersCount() })

    this.peerConfig.eventsHandler = this.peerConfig.eventsHandler || {}
    // Island based peer based peer
    this.peerConfig.eventsHandler.statusHandler = statusHandler

    this.peerConfig.eventsHandler.onIslandChange = (island, peers) => {
      if (island) this.onIslandChangedObservable.notifyObservers({ island, peers })
      this.currentIsland = island
      this.peerIdAdapter.removeAllBut(peers.map(($) => $.id))
      this.refreshTopics().catch(logger.error)
    }

    this.peerConfig.eventsHandler.onPeerLeftIsland = (peerId) => {
      this.peerIdAdapter.disconnectPeer(peerId)
    }

    // We require a version greater than 0.1 to not send an ID
    return new IslandBasedPeer(this.lighthouseUrl, undefined, this.peerCallback, this.peerConfig)
  }

  private peerCallback: PacketCallback = (sender, room, payload, _packet) => {
    if (this.disposed) return
    try {
      const commsMessage = CommsMessage.deserializeBinary(payload)
      switch (commsMessage.getDataCase()) {
        case CommsMessage.DataCase.CHAT_DATA:
          this.peerIdAdapter.handleMessage(
            'chatMessage',
            createPackage(sender, mapToPackageChat(commsMessage.getChatData()!, commsMessage.getTime()))
          )
          break
        case CommsMessage.DataCase.POSITION_DATA:
          const positionMessage = mapToPositionMessage(commsMessage.getPositionData()!)
          this.peer.setPeerPosition(sender, positionMessage.slice(0, 3) as [number, number, number])
          this.peerIdAdapter.handleMessage(
            'position',
            createPackage(sender, {
              index: commsMessage.getTime(),
              positionX: positionMessage[0],
              positionY: positionMessage[1],
              positionZ: positionMessage[2],
              rotationX: positionMessage[3],
              rotationY: positionMessage[4],
              rotationZ: positionMessage[5],
              rotationW: positionMessage[6]
            })
          )
          break
        case CommsMessage.DataCase.SCENE_DATA:
          this.peerIdAdapter.handleMessage(
            'sceneMessageBus',
            createPackage(sender, mapToPackageScene(commsMessage.getSceneData()!))
          )
          break
        case CommsMessage.DataCase.PROFILE_DATA:
          // TODO: update internal peers list using this information
          const userId = commsMessage.getProfileData()?.getUserId()
          if (userId) {
            if (sender.startsWith('0x')) debugger
            this.peerIdAdapter.identifyPeer(sender, userId)
          }
          this.peerIdAdapter.handleMessage(
            'profileMessage',
            createPackage(sender, mapToPackageProfile(commsMessage.getProfileData()!))
          )
          break
        case CommsMessage.DataCase.VOICE_DATA:
          this.peerIdAdapter.handleMessage(
            'voiceMessage',
            createPackage(
              sender,
              mapToPackageVoice(
                commsMessage.getVoiceData()!.getEncodedSamples_asU8(),
                commsMessage.getVoiceData()!.getIndex()
              )
            )
          )
          break
        case CommsMessage.DataCase.PROFILE_REQUEST_DATA:
          this.peerIdAdapter.handleMessage(
            'profileRequest',
            createPackage(sender, mapToPackageProfileRequest(commsMessage.getProfileRequestData()!))
          )
          break
        case CommsMessage.DataCase.PROFILE_RESPONSE_DATA: {
          this.peerIdAdapter.handleMessage(
            'profileResponse',
            createPackage(sender, {
              serializedProfile: commsMessage.getProfileResponseData()!.getSerializedProfile(),
              baseUrl: 'MOCKED'
            })
          )
          break
        }
        default: {
          logger.warn(`message with unknown type received ${commsMessage.getDataCase()}`)
          break
        }
      }
    } catch (e: any) {
      logger.error(`Error processing received message from ${sender}. Topic: ${room}`, e)
    }
  }
}

function createPackage<T>(sender: string, data: T): Package<T> {
  return {
    address: sender,
    data
  }
}

function mapToPositionMessage(positionData: PositionData): number[] {
  return [
    positionData.getPositionX(),
    positionData.getPositionY(),
    positionData.getPositionZ(),
    positionData.getRotationX(),
    positionData.getRotationY(),
    positionData.getRotationZ(),
    positionData.getRotationW()
  ]
}

function mapToPackageChat(chatData: ChatData, timestamp: number): rfc4.Chat {
  return {
    message: chatData.getText(),
    timestamp
  }
}

const encoder = new TextEncoder()
function mapToPackageScene(sceneData: SceneData): rfc4.Scene {
  return {
    sceneId: sceneData.getSceneId(),
    data: encoder.encode(sceneData.getText() || '')
  }
}

function mapToPackageProfile(profileData: ProfileData): rfc4.AnnounceProfileVersion {
  return {
    profileVersion: +(profileData.getProfileVersion() || '0')
  }
}

function mapToPackageProfileRequest(profileRequestData: ProfileRequestData): rfc4.ProfileRequest {
  const versionData = profileRequestData.getProfileVersion()
  return {
    address: profileRequestData.getUserId(),
    profileVersion: +(versionData || '0')
  }
}

function mapToPackageVoice(encodedSamples: Uint8Array, index: number): rfc4.Voice {
  return {
    encodedSamples,
    index,
    codec: rfc4.Voice_VoiceCodec.VC_OPUS
  }
}

function createProfileData(address: string, profileType: ProfileType, version: number) {
  const profileData = new ProfileData()
  profileData.setProfileVersion(version ? version.toString() : '')
  profileData.setUserId(address)
  profileData.setProfileType(getProtobufProfileType(profileType))
  return profileData
}

function getProtobufProfileType(profileType: ProfileType) {
  return profileType === ProfileType.LOCAL ? ProfileData.ProfileType.LOCAL : ProfileData.ProfileType.DEPLOYED
}

function createPositionData(p: rfc4.Position) {
  const positionData = new PositionData()
  positionData.setPositionX(p.positionX)
  positionData.setPositionY(p.positionY)
  positionData.setPositionZ(p.positionZ)
  positionData.setRotationX(p.rotationX)
  positionData.setRotationY(p.rotationY)
  positionData.setRotationZ(p.rotationZ)
  positionData.setRotationW(p.rotationW)
  positionData.setImmediate(false)
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
