import { WorldInstanceConnection } from '../interface/index'
import { Stats } from '../debug'
import { Package, BusMessage, ChatMessage, ProfileVersion, UserInformation, PackageType } from '../interface/types'
import { Position, positionHash } from '../interface/utils'
import { Peer } from 'decentraland-katalyst-peer'
import { createLogger } from 'shared/logger'
import { PeerMessageTypes, PeerMessageType } from 'decentraland-katalyst-peer/src/messageTypes'
import { ChatData, CommsMessage, ProfileData, SceneData, PositionData } from './proto/comms_pb'

const NOOP = () => {
  // do nothing
}

const logger = createLogger('Lighthouse: ')

type MessageData = ChatData | ProfileData | SceneData | PositionData

export class LighthouseWorldInstanceConnection implements WorldInstanceConnection {
  stats: Stats | null = null

  sceneMessageHandler: (alias: string, data: Package<BusMessage>) => void = NOOP
  chatHandler: (alias: string, data: Package<ChatMessage>) => void = NOOP
  profileHandler: (alias: string, identity: string, data: Package<ProfileVersion>) => void = NOOP
  positionHandler: (alias: string, data: Package<Position>) => void = NOOP

  isAuthenticated: boolean = true // TODO - remove this

  ping: number = -1

  constructor(private peer: Peer) {
    logger.info(`connected peer as `, peer.peerId)
    peer.callback = (sender, room, payload) => {
      const commsMessage = CommsMessage.deserializeBinary(payload)

      switch (commsMessage.getDataCase()) {
        case CommsMessage.DataCase.CHAT_DATA:
          this.chatHandler(sender, createPackage(commsMessage, 'chat', mapToPackageChat(commsMessage.getChatData()!)))
          break
        case CommsMessage.DataCase.POSITION_DATA:
          this.positionHandler(
            sender,
            createPackage(commsMessage, 'position', mapToPositionMessage(commsMessage.getPositionData()!))
          )
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
        default: {
          logger.warn(`message with unknown type received ${commsMessage.getDataCase()}`)
          break
        }
      }
    }
  }

  printDebugInformation() {
    // TODO - implement this - moliva - 20/12/2019
  }

  close() {
    const rooms = this.peer.currentRooms
    return Promise.all(
      rooms.map(room => this.peer.leaveRoom(room.id).catch(e => logger.trace(`error while leaving room ${room.id}`, e)))
    )
  }

  async sendInitialMessage(userInfo: Partial<UserInformation>) {
    const topic = userInfo.userId!

    await this.sendProfileData(userInfo, topic)
  }

  async sendProfileMessage(currentPosition: Position, userInfo: UserInformation) {
    const topic = positionHash(currentPosition)

    await this.sendProfileData(userInfo, topic)
  }

  async sendPositionMessage(p: Position) {
    const topic = positionHash(p)

    await this.sendPositionData(p, topic)
  }

  async sendParcelUpdateMessage(currentPosition: Position, p: Position) {
    const topic = positionHash(currentPosition)

    await this.sendPositionData(p, topic)
  }

  async sendParcelSceneCommsMessage(sceneId: string, message: string) {
    const topic = sceneId

    const sceneData = new SceneData()
    sceneData.setSceneId(sceneId)
    sceneData.setText(message)

    await this.sendData(topic, sceneData, PeerMessageTypes.reliable)
  }

  async sendChatMessage(currentPosition: Position, messageId: string, text: string) {
    const topic = positionHash(currentPosition)

    const chatMessage = new ChatData()
    chatMessage.setMessageId(messageId)
    chatMessage.setText(text)

    await this.sendData(topic, chatMessage, PeerMessageTypes.reliable)
  }

  async updateSubscriptions(rooms: string[]) {
    const currentRooms = this.peer.currentRooms
    const joining = rooms.map(room => {
      if (!currentRooms.some(current => current.id === room)) {
        return this.peer.joinRoom(room)
      } else {
        return Promise.resolve()
      }
    })
    const leaving = currentRooms.map(current => {
      if (!rooms.some(room => current.id === room)) {
        return this.peer.leaveRoom(current.id)
      } else {
        return Promise.resolve()
      }
    })
    return Promise.all([...joining, ...leaving]).then(NOOP)
  }

  private async sendData(topic: string, messageData: MessageData, type: PeerMessageType = PeerMessageTypes.unreliable) {
    await this.peer.sendMessage(topic, createCommsMessage(messageData).serializeBinary(), type)
  }

  private async sendPositionData(p: Position, topic: string) {
    const positionData = createPositionData(p)
    await this.sendData(topic, positionData)
  }

  private async sendProfileData(userInfo: UserInformation, topic: string) {
    const profileData = createProfileData(userInfo)
    await this.sendData(topic, profileData)
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
    positionData.getRotationW()
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
  return { user: profileData.getUserId(), version: profileData.getProfileVersion() }
}

function createProfileData(userInfo: UserInformation) {
  const profileData = new ProfileData()
  profileData.setProfileVersion(userInfo.version ? userInfo.version.toString() : '')
  profileData.setUserId(userInfo.userId ? userInfo.userId : '')
  return profileData
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
  return positionData
}

function createCommsMessage(data: MessageData) {
  const commsMessage = new CommsMessage()
  commsMessage.setTime(Date.now())

  if (data instanceof ChatData) commsMessage.setChatData(data)
  if (data instanceof SceneData) commsMessage.setSceneData(data)
  if (data instanceof ProfileData) commsMessage.setProfileData(data)
  if (data instanceof PositionData) commsMessage.setPositionData(data)

  return commsMessage
}
