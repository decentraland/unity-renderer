/// <reference lib="dom" />

import { Message } from 'google-protobuf'
import { log, error as logError } from 'engine/logger'
import { Stats } from './debug'
import {
  Category,
  ChatData,
  PositionData,
  ProfileData,
  MessageType,
  PingMessage,
  CoordinatorMessage,
  WebRtcMessage,
  TopicMessage,
  DataMessage,
  DataHeader,
  WorldCommMessage,
  WelcomeMessage,
  ConnectMessage,
  AuthMessage,
  Role,
  Format,
  TopicSubscriptionMessage
} from './commproto_pb'
import { Position, position2parcel } from './utils'
import { UserInformation } from './types'
import { parcelLimits, commConfigurations } from 'config'

export enum SocketReadyState {
  CONNECTING,
  OPEN,
  CLOSING,
  CLOSED
}

class SendResult {
  constructor(public bytesSize: number) {}
}

type IDataChannel = {
  readyState: RTCDataChannelState
  send(bytes: Uint8Array): void
}

export function positionHash(p: Position) {
  const parcel = position2parcel(p)
  const x = (parcel.x + parcelLimits.maxParcelX) >> 2
  const z = (parcel.z + parcelLimits.maxParcelZ) >> 2
  return `${x}:${z}`
}

export class WorldInstanceConnection {
  public alias: string | null = null
  public ping: number = -1

  public commServerAlias: number | null = null
  public ws: WebSocket | null = null
  public webRtcConn: RTCPeerConnection | null = null
  public positionHandler: ((fromAlias: string, positionData: PositionData) => void) | null = null
  public profileHandler: ((fromAlias: string, profileData: ProfileData) => void) | null = null
  public chatHandler: ((fromAlias: string, chatData: ChatData) => void) | null = null
  public authenticated = false
  public reliableDataChannel: IDataChannel | null = null
  public unreliableDataChannel: IDataChannel | null = null
  public pingInterval: any = null
  public stats: Stats | null = null

  constructor(public url: string) {}

  connect() {
    this.pingInterval = setInterval(() => {
      if (this.unreliableDataChannel && this.authenticated) {
        const msg = new PingMessage()
        msg.setType(MessageType.PING)
        msg.setTime(Date.now())
        const bytes = msg.serializeBinary()

        if (this.unreliableDataChannel.readyState === 'open') {
          this.unreliableDataChannel.send(bytes)
        } else {
          this.ping = -1
        }
      }
    }, 10000)

    this.webRtcConn = new RTCPeerConnection({
      iceServers: commConfigurations.iceServers
    })

    this.webRtcConn.onsignalingstatechange = (e: Event) => log(`signaling state: ${this.webRtcConn!.signalingState}`)
    this.webRtcConn.oniceconnectionstatechange = (e: Event) =>
      log(`ice connection state: ${this.webRtcConn!.iceConnectionState}`)

    this.ws = new WebSocket(this.url)
    this.ws.binaryType = 'arraybuffer'
    const ws = this.ws

    const sendCoordinatorMessage = (msg: Message) => {
      if (!ws || ws.readyState !== SocketReadyState.OPEN) {
        throw new Error('try to send answer to a non ready ws')
      }

      const bytes = msg.serializeBinary()

      if (this.stats) {
        this.stats.webRtcSession.incrementSent(1, bytes.length)
      }

      ws.send(bytes)
    }

    ws.onmessage = async event => {
      const data = event.data
      const msg = new Uint8Array(data)
      const msgSize = msg.length

      const msgType = CoordinatorMessage.deserializeBinary(data).getType()

      switch (msgType) {
        case MessageType.UNKNOWN_MESSAGE_TYPE: {
          if (this.stats) {
            this.stats.others.incrementRecv(msgSize)
          }
          log('unsopported message')
          break
        }
        case MessageType.WELCOME: {
          if (this.stats) {
            this.stats.others.incrementRecv(msgSize)
          }

          let message
          try {
            message = WelcomeMessage.deserializeBinary(msg)
          } catch (e) {
            logError('cannot deserialize welcome client message', e, msg)
            break
          }

          const alias = `${message.getAlias()}`
          const availableServers = message.getAvailableServersList()

          if (availableServers.length === 0) {
            throw new Error('no available servers')
          }

          const serverAlias = availableServers[0]
          this.commServerAlias = serverAlias
          this.alias = alias
          log('my alias is', alias)

          const connectMessage = new ConnectMessage()
          connectMessage.setType(MessageType.CONNECT)
          connectMessage.setToAlias(serverAlias)
          sendCoordinatorMessage(connectMessage)
          break
        }
        case MessageType.WEBRTC_ICE_CANDIDATE:
        case MessageType.WEBRTC_OFFER:
        case MessageType.WEBRTC_ANSWER: {
          if (this.stats) {
            this.stats.webRtcSession.incrementRecv(msgSize)
          }

          let message: WebRtcMessage
          try {
            message = WebRtcMessage.deserializeBinary(msg)
          } catch (e) {
            logError('cannot deserialize webrtc ice candidate message', e, msg)
            break
          }

          const sdp = message.getSdp()

          if (message.getFromAlias() !== this.commServerAlias) {
            log('ignore webrtc message from unknown peer', message.getFromAlias())
            break
          }

          if (msgType === MessageType.WEBRTC_ICE_CANDIDATE) {
            await this.webRtcConn!.addIceCandidate({ candidate: sdp })
          } else if (msgType === MessageType.WEBRTC_OFFER) {
            try {
              await this.webRtcConn!.setRemoteDescription(new RTCSessionDescription({ type: 'offer', sdp: sdp }))
              const desc = await this.webRtcConn!.createAnswer()
              await this.webRtcConn!.setLocalDescription(desc)

              if (desc.sdp) {
                const msg = new WebRtcMessage()
                msg.setToAlias(this.commServerAlias)
                msg.setType(MessageType.WEBRTC_ANSWER)
                msg.setSdp(desc.sdp)
                sendCoordinatorMessage(msg)
              }
            } catch (err) {
              logError(err)
            }
          } else if (msgType === MessageType.WEBRTC_ANSWER) {
            try {
              await this.webRtcConn!.setRemoteDescription(new RTCSessionDescription({ type: 'answer', sdp: sdp }))
            } catch (err) {
              logError(err)
            }
          }
          break
        }
        default: {
          if (this.stats) {
            this.stats.others.incrementRecv(msgSize)
          }
          log('ignoring message with type', msgType)
          break
        }
      }
    }

    ws.onerror = event => {
      logError('socket error', event)
    }

    this.webRtcConn.onicecandidate = async (event: RTCPeerConnectionIceEvent) => {
      if (event.candidate !== null) {
        const msg = new WebRtcMessage()
        msg.setType(MessageType.WEBRTC_ICE_CANDIDATE)
        // TODO: Ensure commServerAlias, it may be null
        msg.setToAlias(this.commServerAlias!)
        msg.setSdp(event.candidate.candidate)
        sendCoordinatorMessage(msg)
      }
    }

    this.webRtcConn.ondatachannel = (e: RTCDataChannelEvent) => {
      let dc = e.channel

      dc.onclose = () => log('dc has closed')
      dc.onopen = () => {
        const label = dc.label
        log('dc has opened', label)

        if (label === 'reliable') {
          this.reliableDataChannel = dc
          const authMessage = new AuthMessage()
          authMessage.setType(MessageType.AUTH)
          authMessage.setRole(Role.CLIENT)
          authMessage.setMethod('noop')
          const bytes = authMessage.serializeBinary()

          if (dc.readyState === 'open') {
            dc.send(bytes)
            this.authenticated = true
          } else {
            logError('cannot send authentication, data channel is not ready')
          }
        } else if (label === 'unreliable') {
          this.unreliableDataChannel = dc
        }
      }

      dc.onmessage = (e: MessageEvent) => {
        if (this.stats) {
          this.stats.dispatchTopicDuration.start()
        }

        const data = e.data
        const msg = new Uint8Array(data)
        const msgSize = msg.length

        let msgType = MessageType.UNKNOWN_MESSAGE_TYPE
        try {
          msgType = WorldCommMessage.deserializeBinary(data).getType()
        } catch (err) {
          logError('cannot deserialize worldcomm message header ' + dc.label + ' ' + msgSize)
          return
        }

        switch (msgType) {
          case MessageType.UNKNOWN_MESSAGE_TYPE: {
            if (this.stats) {
              this.stats.others.incrementRecv(msgSize)
            }
            log('unsopported message')
            break
          }
          case MessageType.DATA: {
            if (this.stats) {
              this.stats.topic.incrementRecv(msgSize)
            }
            let message: DataMessage
            try {
              message = DataMessage.deserializeBinary(data)
            } catch (e) {
              logError('cannot process topic message', e)
              break
            }

            const body = message.getBody() as any

            let dataHeader: DataHeader
            try {
              dataHeader = DataHeader.deserializeBinary(body)
            } catch (e) {
              logError('cannot process data header', e)
              break
            }

            const alias = `${message.getFromAlias()}`
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
              case Category.PROFILE: {
                const profileData = ProfileData.deserializeBinary(body)
                if (this.stats) {
                  this.stats.dispatchTopicDuration.stop()
                  this.stats.profile.incrementRecv(msgSize)
                }
                this.profileHandler && this.profileHandler(alias, profileData)
                break
              }
              default: {
                log('ignoring category', category)
                break
              }
            }
            break
          }
          case MessageType.PING: {
            let message
            try {
              message = PingMessage.deserializeBinary(msg)
            } catch (e) {
              logError('cannot deserialize ping message', e, msg)
              break
            }

            if (this.stats) {
              this.stats.ping.incrementRecv(msgSize)
            }

            this.ping = Date.now() - message.getTime()

            break
          }
          default: {
            if (this.stats) {
              this.stats.others.incrementRecv(msgSize)
            }
            log('ignoring message with type', msgType)
            break
          }
        }
      }
    }
  }

  sendPositionMessage(p: Position) {
    const topic = `position:${positionHash(p)}`

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

  sendProfileMessage(p: Position, userProfile: UserInformation) {
    const topic = `profile:${positionHash(p)}`

    const d = new ProfileData()
    d.setCategory(Category.PROFILE)
    d.setTime(Date.now())
    userProfile.avatarType && d.setAvatarType(userProfile.avatarType)
    userProfile.displayName && d.setDisplayName(userProfile.displayName)
    userProfile.publicKey && d.setPublicKey(userProfile.publicKey)

    const r = this.sendTopicMessage(true, topic, d)
    if (this.stats) {
      this.stats.profile.incrementSent(1, r.bytesSize)
    }
  }

  sendChatMessage(p: Position, messageId: string, text: string) {
    const topic = `chat:${positionHash(p)}`

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

    const topicMessage = new TopicMessage()
    topicMessage.setType(MessageType.TOPIC)
    topicMessage.setTopic(topic)
    topicMessage.setBody(encodedBody)

    const bytes = topicMessage.serializeBinary()
    if (this.stats) {
      this.stats.topic.incrementSent(1, bytes.length)
    }

    if (reliable) {
      if (!this.reliableDataChannel) {
        throw new Error('trying to send a topic message using null reliable channel')
      }

      if (this.reliableDataChannel.readyState === 'open') {
        this.reliableDataChannel.send(bytes)
      }
    } else {
      if (!this.unreliableDataChannel) {
        throw new Error('trying to send a topic message using null unreliable channel')
      }

      if (this.unreliableDataChannel.readyState === 'open') {
        this.unreliableDataChannel.send(bytes)
      }
    }

    return new SendResult(bytes.length)
  }

  updateSubscriptions(rawTopics: string) {
    if (!this.reliableDataChannel || this.reliableDataChannel.readyState !== 'open') {
      throw new Error('trying to send topic subscription message but reliable channel is not ready')
    }
    const subscriptionMessage = new TopicSubscriptionMessage()
    subscriptionMessage.setType(MessageType.TOPIC_SUBSCRIPTION)
    subscriptionMessage.setFormat(Format.PLAIN)
    subscriptionMessage.setTopics(Buffer.from(rawTopics, 'utf8'))
    const bytes = subscriptionMessage.serializeBinary()
    this.reliableDataChannel.send(bytes)
  }

  close() {
    if (this.pingInterval) {
      clearInterval(this.pingInterval)
    }

    if (this.webRtcConn) {
      this.webRtcConn.onsignalingstatechange = null
      this.webRtcConn.oniceconnectionstatechange = null
      this.webRtcConn.onicecandidate = null
      this.webRtcConn.ondatachannel = null
      this.webRtcConn.close()
      this.webRtcConn = null
    }

    if (this.ws) {
      this.ws.onmessage = null
      this.ws.onerror = null
      this.ws.onclose = null
      this.ws.close()
    }
  }
}
