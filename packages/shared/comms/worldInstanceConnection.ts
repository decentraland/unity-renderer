import { Message } from 'google-protobuf'
import { log, error as logError } from 'engine/logger'
import { NetworkStats, PkgStats } from './debug'
import {
  ChatData,
  PositionData,
  ProfileData,
  MessageType,
  PingMessage,
  CoordinatorMessage,
  WebRtcMessage,
  TopicMessage,
  WorldCommMessage,
  WelcomeMessage,
  ConnectMessage,
  AuthMessage,
  Role,
  Format,
  TopicSubscriptionMessage
} from './commproto_pb'
import { Position } from './utils'
import { UserInformation } from './types'
import { parcelLimits } from 'config'

export enum SocketReadyState {
  CONNECTING,
  OPEN,
  CLOSING,
  CLOSED
}

class SendResult {
  constructor(public bytesSize: number) {}
}

export interface IDataChannel {
  send(bytes: Uint8Array)
}

export type TopicHandler = (fromAlias: string, data: Uint8Array) => PkgStats | null

export function positionHash(p: Position) {
  const x = (p[0] + parcelLimits.maxParcelX) >> 2
  const z = (p[2] + parcelLimits.maxParcelZ) >> 2
  return `${x}:${z}`
}

export class WorldInstanceConnection {
  public alias: string = null
  public ping: number = -1
  public commServerAlias: string | null = null
  public ws: WebSocket | null
  public webRtcConn: RTCPeerConnection | null
  public subscriptions = new Map<string, TopicHandler>()
  public authenticated = false
  public reliableDataChannel: IDataChannel | null
  public unreliableDataChannel: IDataChannel | null
  public pingInterval: any = null

  constructor(public url: string, public stats?: NetworkStats) {}

  connect() {
    this.pingInterval = setInterval(() => {
      if (this.unreliableDataChannel && this.authenticated) {
        const msg = new PingMessage()
        msg.setType(MessageType.PING)
        msg.setTime(Date.now())
        const bytes = msg.serializeBinary()
        this.unreliableDataChannel.send(bytes)
      }
    }, 10000)
    this.webRtcConn = new RTCPeerConnection({
      iceServers: [
        {
          urls: 'stun:stun.l.google.com:19302'
        }
      ]
    })

    this.webRtcConn.onsignalingstatechange = e => log(`signaling state: ${this.webRtcConn.signalingState}`)
    this.webRtcConn.oniceconnectionstatechange = e => log(`ice connection state: ${this.webRtcConn.iceConnectionState}`)

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

          const alias = message.getAlias()
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

          let message
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
            await this.webRtcConn.addIceCandidate(sdp)
          } else if (msgType === MessageType.WEBRTC_OFFER) {
            try {
              await this.webRtcConn.setRemoteDescription(new RTCSessionDescription({ type: 'offer', sdp: sdp }))
              const desc = await this.webRtcConn.createAnswer()
              await this.webRtcConn.setLocalDescription(desc)

              const msg = new WebRtcMessage()
              msg.setToAlias(this.commServerAlias)
              msg.setType(MessageType.WEBRTC_ANSWER)
              msg.setSdp(desc.sdp)
              sendCoordinatorMessage(msg)
            } catch (err) {
              logError(err)
            }
          } else if (msgType === MessageType.WEBRTC_ANSWER) {
            try {
              await this.webRtcConn.setRemoteDescription(new RTCSessionDescription({ type: 'answer', sdp: sdp }))
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

    this.webRtcConn.onicecandidate = async event => {
      if (event.candidate !== null) {
        const msg = new WebRtcMessage()
        msg.setType(MessageType.WEBRTC_ICE_CANDIDATE)
        msg.setToAlias(this.commServerAlias)
        msg.setSdp(event.candidate.candidate)
        sendCoordinatorMessage(msg)
      }
    }

    this.webRtcConn.ondatachannel = e => {
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
          this.reliableDataChannel.send(bytes)
          this.authenticated = true
        } else if (label === 'unreliable') {
          this.unreliableDataChannel = dc
        }
      }

      dc.onmessage = e => {
        const data = e.data
        const msg = new Uint8Array(data)
        const msgSize = msg.length

        const msgType = WorldCommMessage.deserializeBinary(data).getType()

        switch (msgType) {
          case MessageType.UNKNOWN_MESSAGE_TYPE: {
            if (this.stats) {
              this.stats.others.incrementRecv(msgSize)
            }
            log('unsopported message')
            break
          }
          case MessageType.TOPIC: {
            if (this.stats) {
              this.stats.topic.incrementRecv(msgSize)
            }
            let message
            try {
              message = TopicMessage.deserializeBinary(data)
            } catch (e) {
              logError('cannot process topic message', e)
            }

            const topic = message.getTopic()
            const handler = this.subscriptions.get(topic)

            if (!handler) {
              log('ignoring topic message with topic', topic)
              break
            }

            const pkgStats = handler(message.getFromAlias(), message.getBody())
            if (pkgStats) {
              pkgStats.incrementRecv(msgSize)
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
    d.setTime(Date.now())
    d.setAvatarType(userProfile.avatarType)
    d.setDisplayName(userProfile.displayName)
    d.setPublicKey(userProfile.publicKey)

    const r = this.sendTopicMessage(true, topic, d)
    if (this.stats) {
      this.stats.profile.incrementSent(1, r.bytesSize)
    }
  }

  sendChatMessage(p: Position, messageId: string, text: string) {
    const topic = `chat:${positionHash(p)}`

    const d = new ChatData()
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
      this.reliableDataChannel.send(bytes)
    } else {
      if (!this.unreliableDataChannel) {
        throw new Error('trying to send a topic message using null unreliable channel')
      }
      this.unreliableDataChannel.send(bytes)
    }

    return new SendResult(bytes.length)
  }

  updateSubscriptions(subscriptions: Map<string, TopicHandler>, rawTopics: string) {
    if (!this.reliableDataChannel) {
      throw new Error('trying to send topic subscription message using null reliable channel')
    }
    this.subscriptions = subscriptions
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
