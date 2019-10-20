/// <reference lib="dom" />

import { Message } from 'google-protobuf'
import { Category, ChatData, PositionData, ProfileData, DataHeader } from './proto/comms'
import {
  MessageType,
  PingMessage,
  TopicMessage,
  TopicFWMessage,
  Format,
  SubscriptionMessage,
  MessageHeader,
  TopicIdentityMessage,
  TopicIdentityFWMessage
} from './proto/broker'
import { Position, position2parcel } from './utils'
import { UserInformation } from './types'
import { parcelLimits } from 'config'
import { IBrokerConnection, BrokerMessage } from './IBrokerConnection'
import { Stats } from './debug'
import { createLogger } from 'shared/logger'

export enum SocketReadyState {
  CONNECTING,
  OPEN,
  CLOSING,
  CLOSED
}

import { Reporter } from './PresenceReporter'

class SendResult {
  constructor(public bytesSize: number) {}
}

export function positionHash(p: Position) {
  const parcel = position2parcel(p)
  const x = (parcel.x + parcelLimits.maxParcelX) >> 2
  const z = (parcel.z + parcelLimits.maxParcelZ) >> 2
  return `${x}:${z}`
}

export class WorldInstanceConnection {
  public aliases: Record<number, string> = {}
  public positionHandler: ((fromAlias: string, positionData: PositionData) => void) | null = null
  public profileHandler: ((fromAlias: string, identity: string, profileData: ProfileData) => void) | null = null
  public chatHandler: ((fromAlias: string, chatData: ChatData) => void) | null = null
  // TODO: Once we have the correct class, change ChatData
  public sceneMessageHandler: ((fromAlias: string, chatData: ChatData) => void) | null = null
  public ping: number = -1

  public fatalErrorSent = false

  public stats: Stats | null = null
  private pingInterval: any = null

  private logger = createLogger('World: ')

  constructor(public connection: IBrokerConnection) {
    this.pingInterval = setInterval(() => {
      const msg = new PingMessage()
      msg.setType(MessageType.PING)
      msg.setTime(Date.now())
      const bytes = msg.serializeBinary()

      if (this.connection.hasUnreliableChannel) {
        this.connection.sendUnreliable(bytes)
      } else {
        this.ping = -1
      }
    }, 10000)
    this.connection.onMessageObservable.add(this.handleMessage.bind(this))
  }

  sendPositionMessage(p: Position) {
    const topic = positionHash(p)

    const d = new PositionData()
    d.setCategory(Category.POSITION)
    d.setTime(Date.now())
    d.setPositionX(p[0])
    d.setPositionY(p[1])
    d.setPositionZ(p[2])
    d.setRotationX(p[3])
    d.setRotationY(p[4])
    d.setRotationZ(p[5])
    d.setRotationW(p[6])

    const r = this.sendTopicMessage(false, topic, d)
    if (this.stats) {
      this.stats.position.incrementSent(1, r.bytesSize)
    }
  }

  sendParcelUpdateMessage(current: Position, newPosition: Position) {
    const topic = positionHash(current)

    const d = new PositionData()
    d.setCategory(Category.POSITION)
    d.setTime(Date.now())
    d.setPositionX(newPosition[0])
    d.setPositionY(newPosition[1])
    d.setPositionZ(newPosition[2])
    d.setRotationX(newPosition[3])
    d.setRotationY(newPosition[4])
    d.setRotationZ(newPosition[5])
    d.setRotationW(newPosition[6])

    const r = this.sendTopicMessage(false, topic, d)
    if (this.stats) {
      this.stats.position.incrementSent(1, r.bytesSize)
    }
  }

  sendProfileMessage(p: Position, userProfile: UserInformation) {
    const topic = positionHash(p)

    const d = new ProfileData()
    d.setCategory(Category.PROFILE)
    d.setTime(Date.now())
    userProfile.version && d.setProfileVersion('' + userProfile.version)

    const r = this.sendTopicIdentityMessage(true, topic, d)
    if (this.stats) {
      this.stats.profile.incrementSent(1, r.bytesSize)
    }
  }

  sendParcelSceneCommsMessage(sceneId: string, message: string) {
    const topic = sceneId

    // TODO: create its own class once we get the .proto file
    const d = new ChatData()
    d.setCategory(Category.SCENE_MESSAGE)
    d.setTime(Date.now())
    d.setMessageId(sceneId)
    d.setText(message)

    const r = this.sendTopicMessage(true, topic, d)

    if (this.stats) {
      this.stats.sceneComms.incrementSent(1, r.bytesSize)
    }
  }

  sendChatMessage(p: Position, messageId: string, text: string) {
    const topic = positionHash(p)

    const d = new ChatData()
    d.setCategory(Category.CHAT)
    d.setTime(Date.now())
    d.setMessageId(messageId)
    d.setText(text)

    const r = this.sendTopicMessage(true, topic, d)

    if (this.stats) {
      this.stats.chat.incrementSent(1, r.bytesSize)
    }
  }

  sendTopicMessage(reliable: boolean, topic: string, body: Message): SendResult {
    const encodedBody = body.serializeBinary()

    const message = new TopicMessage()
    message.setType(MessageType.TOPIC)
    message.setTopic(topic)
    message.setBody(encodedBody)

    return this.sendMessage(reliable, message)
  }

  sendTopicIdentityMessage(reliable: boolean, topic: string, body: Message): SendResult {
    const encodedBody = body.serializeBinary()

    const message = new TopicIdentityMessage()
    message.setType(MessageType.TOPIC_IDENTITY)
    message.setTopic(topic)
    message.setBody(encodedBody)

    return this.sendMessage(reliable, message)
  }

  updateSubscriptions(rawTopics: string) {
    if (!this.connection.hasReliableChannel) {
      if (!this.fatalErrorSent) {
        this.fatalErrorSent = true
        throw new Error('trying to send topic subscription message but reliable channel is not ready')
      } else {
        return
      }
    }
    rawTopics.split(' ').map(_ => Reporter.subscribe(_))
    const subscriptionMessage = new SubscriptionMessage()
    subscriptionMessage.setType(MessageType.SUBSCRIPTION)
    subscriptionMessage.setFormat(Format.PLAIN)
    // TODO: use TextDecoder instead of Buffer, it is a native browser API, works faster
    subscriptionMessage.setTopics(Buffer.from(rawTopics, 'utf8'))
    const bytes = subscriptionMessage.serializeBinary()
    this.connection.sendReliable(bytes)
  }

  close() {
    if (this.pingInterval) {
      clearInterval(this.pingInterval)
    }
    this.connection.close()
  }

  private handleMessage(message: BrokerMessage) {
    const msgSize = message.data.length

    let msgType = MessageType.UNKNOWN_MESSAGE_TYPE
    try {
      msgType = MessageHeader.deserializeBinary(message.data).getType()
    } catch (err) {
      this.logger.error('cannot deserialize worldcomm message header ' + message.channel + ' ' + msgSize)
      return
    }

    switch (msgType) {
      case MessageType.UNKNOWN_MESSAGE_TYPE: {
        if (this.stats) {
          this.stats.others.incrementRecv(msgSize)
        }
        this.logger.log('unsupported message')
        break
      }
      case MessageType.TOPIC_FW: {
        if (this.stats) {
          this.stats.topic.incrementRecv(msgSize)
        }
        let dataMessage: TopicFWMessage
        try {
          dataMessage = TopicFWMessage.deserializeBinary(message.data)
        } catch (e) {
          this.logger.error('cannot process topic message', e)
          break
        }

        const body = dataMessage.getBody() as any

        let dataHeader: DataHeader
        try {
          dataHeader = DataHeader.deserializeBinary(body)
        } catch (e) {
          this.logger.error('cannot process data header', e)
          break
        }

        const aliasNum = dataMessage.getFromAlias()
        const alias = aliasNum.toString()
        if (this.aliases[aliasNum]) Reporter.reportSeen(this.aliases[aliasNum])
        const category = dataHeader.getCategory()
        switch (category) {
          case Category.POSITION: {
            const positionData = PositionData.deserializeBinary(body)

            if (this.stats) {
              this.stats.dispatchTopicDuration.stop()
              this.stats.position.incrementRecv(msgSize)
              this.stats.onPositionMessage(alias, positionData)
            }

            this.positionHandler && this.positionHandler(alias, positionData)
            break
          }
          case Category.CHAT: {
            const chatData = ChatData.deserializeBinary(body)

            if (this.stats) {
              this.stats.dispatchTopicDuration.stop()
              this.stats.chat.incrementRecv(msgSize)
            }

            this.chatHandler && this.chatHandler(alias, chatData)
            break
          }
          case Category.SCENE_MESSAGE: {
            const chatData = ChatData.deserializeBinary(body)

            if (this.stats) {
              this.stats.dispatchTopicDuration.stop()
              this.stats.sceneComms.incrementRecv(msgSize)
            }

            this.sceneMessageHandler && this.sceneMessageHandler(alias, chatData)
            break
          }
          default: {
            this.logger.log('ignoring category', category)
            break
          }
        }
        break
      }
      case MessageType.TOPIC_IDENTITY_FW: {
        if (this.stats) {
          this.stats.topic.incrementRecv(msgSize)
        }
        let dataMessage: TopicIdentityFWMessage
        try {
          dataMessage = TopicIdentityFWMessage.deserializeBinary(message.data)
        } catch (e) {
          this.logger.error('cannot process topic identity message', e)
          break
        }

        const body = dataMessage.getBody() as any

        let dataHeader: DataHeader
        try {
          dataHeader = DataHeader.deserializeBinary(body)
        } catch (e) {
          this.logger.error('cannot process data header', e)
          break
        }

        const alias = dataMessage.getFromAlias().toString()
        const userId = atob(dataMessage.getIdentity_asB64())
        Reporter.reportSeen(userId)
        this.aliases[dataMessage.getFromAlias()] = userId
        const category = dataHeader.getCategory()
        switch (category) {
          case Category.PROFILE: {
            const profileData = ProfileData.deserializeBinary(body)
            if (this.stats) {
              this.stats.dispatchTopicDuration.stop()
              this.stats.profile.incrementRecv(msgSize)
            }
            this.profileHandler && this.profileHandler(alias, userId, profileData)
            break
          }
          default: {
            this.logger.log('ignoring category', category)
            break
          }
        }
        break
      }
      case MessageType.PING: {
        let pingMessage
        try {
          pingMessage = PingMessage.deserializeBinary(message.data)
        } catch (e) {
          this.logger.error('cannot deserialize ping message', e, message)
          break
        }

        if (this.stats) {
          this.stats.ping.incrementRecv(msgSize)
        }

        this.ping = Date.now() - pingMessage.getTime()

        break
      }
      default: {
        if (this.stats) {
          this.stats.others.incrementRecv(msgSize)
        }
        this.logger.log('ignoring message with type', msgType)
        break
      }
    }
  }

  private sendMessage(reliable: boolean, topicMessage: Message) {
    const bytes = topicMessage.serializeBinary()
    if (this.stats) {
      this.stats.topic.incrementSent(1, bytes.length)
    }
    if (reliable) {
      if (!this.connection.hasReliableChannel) {
        if (!this.fatalErrorSent) {
          this.fatalErrorSent = true
          throw new Error('trying to send a topic message using null reliable channel')
        } else {
          return new SendResult(0)
        }
      }
      this.connection.sendReliable(bytes)
    } else {
      if (!this.connection.hasUnreliableChannel) {
        if (!this.fatalErrorSent) {
          this.fatalErrorSent = true
          throw new Error('trying to send a topic message using null unreliable channel')
        } else {
          return new SendResult(0)
        }
      }
      this.connection.sendUnreliable(bytes)
    }
    return new SendResult(bytes.length)
  }
}
