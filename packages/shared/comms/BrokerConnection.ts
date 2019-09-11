import { future } from 'fp-future'

import {
  MessageType,
  CoordinatorMessage,
  WelcomeMessage,
  ConnectMessage,
  WebRtcMessage,
  AuthMessage,
  Role
} from './proto/broker'
import { Auth } from 'shared/auth'
import { AuthData } from './proto/comms'
import { Message } from 'google-protobuf'
import { SocketReadyState } from './worldInstanceConnection'
import { commConfigurations } from 'config'
import { Stats } from './debug'
import { IBrokerConnection, BrokerMessage } from './IBrokerConnection'
import { ILogger, createLogger } from 'shared/logger'
import { Observable } from '../../decentraland-ecs/src/ecs/Observable'

export class BrokerConnection implements IBrokerConnection {
  public alias: string | null = null
  public authenticated = false

  public commServerAlias: number | null = null
  public webRtcConn: RTCPeerConnection | null = null

  public reliableDataChannel: RTCDataChannel | null = null
  public unreliableDataChannel: RTCDataChannel | null = null

  public stats: Stats | null = null

  public logger: ILogger = createLogger('Broker: ')

  public onMessageObservable = new Observable<BrokerMessage>()

  public gotCandidatesFuture = future<RTCSessionDescription>()
  private unreliableFuture = future<void>()
  private reliableFuture = future<void>()

  private pendingCandidates: RTCIceCandidate[] = []

  get isConnected(): Promise<void> {
    return Promise.all([this.unreliableFuture, this.reliableFuture]) as Promise<any>
  }

  get isAuthenticated() {
    return this.authenticated
  }

  get hasUnreliableChannel() {
    return (this.unreliableDataChannel && this.unreliableDataChannel.readyState === 'open') || false
  }

  get hasReliableChannel() {
    return (this.reliableDataChannel && this.reliableDataChannel.readyState === 'open') || false
  }

  private ws: WebSocket | null = null

  constructor(private auth: Auth, public url: string) {
    this.connectRTC()
    this.connectWS()

    // TODO: reconnect logic, handle disconnections

    setTimeout(() => {
      if (this.reliableFuture.isPending || this.unreliableFuture.isPending) {
        this.onConnectionError(new Error('Communications link cannot be established (Timeout)'))
      }
    }, 60000)
  }

  printDebugInformation(): void {
    if (this.ws && this.ws.readyState === SocketReadyState.OPEN) {
      const state = (this.authenticated ? 'authenticated' : 'not authenticated') + ` my alias is ${this.alias}`
      this.logger.log(state)
    } else {
      this.logger.log(`non active coordinator connection to ${this.url}`)
    }
  }

  sendReliable(data: Uint8Array) {
    if (!this.hasReliableChannel) {
      throw new Error('trying to message using null reliable channel')
    }
    this.reliableDataChannel!.send(data)
  }

  sendUnreliable(data: Uint8Array) {
    if (!this.hasUnreliableChannel) {
      throw new Error('trying to message using null unreliable channel')
    }
    this.unreliableDataChannel!.send(data)
  }

  close() {
    if (this.webRtcConn) {
      this.webRtcConn.onsignalingstatechange = null
      this.webRtcConn.oniceconnectionstatechange = null
      this.webRtcConn.onicecandidate = null
      this.webRtcConn.ondatachannel = null
      if (this.webRtcConn.close) {
        // TODO - added for tests to run, fix this - moliva - 05/09/2019
        this.webRtcConn.close()
      }
      this.webRtcConn = null
    }

    if (this.ws) {
      this.ws.onmessage = null
      this.ws.onerror = null
      this.ws.onclose = null
      if (this.ws.close) {
        // TODO - added for tests to run, fix this - moliva - 05/09/2019
        this.ws.close()
      }
    }
  }

  async onWsMessage(event: MessageEvent) {
    const data = event.data
    const msg = new Uint8Array(data)
    const msgSize = msg.length

    const msgType = CoordinatorMessage.deserializeBinary(data).getType()

    switch (msgType) {
      case MessageType.UNKNOWN_MESSAGE_TYPE: {
        if (this.stats) {
          this.stats.others.incrementRecv(msgSize)
        }
        this.logger.log('unsupported message')
        break
      }
      case MessageType.WELCOME: {
        if (this.stats) {
          this.stats.others.incrementRecv(msgSize)
        }

        let message: WelcomeMessage
        try {
          message = WelcomeMessage.deserializeBinary(msg)
        } catch (e) {
          this.logger.error('cannot deserialize welcome client message', e, msg)
          break
        }

        const alias = `${message.getAlias()}`
        const availableServers = message.getAvailableServersList()

        if (availableServers.length === 0) {
          const error = new Error('Communications link cannot be established (no available servers)')
          this.onConnectionError(error)
          throw error
        }

        const serverAlias = availableServers[0]
        this.commServerAlias = serverAlias
        this.alias = alias
        this.logger.info('my alias is', alias)

        const connectMessage = new ConnectMessage()
        connectMessage.setType(MessageType.CONNECT)
        connectMessage.setToAlias(serverAlias)
        this.sendCoordinatorMessage(connectMessage)
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
          this.logger.error('cannot deserialize webrtc ice candidate message', e, msg)
          break
        }

        if (message.getFromAlias() !== this.commServerAlias) {
          this.logger.log('ignore webrtc message from unknown peer', message.getFromAlias())
          break
        }

        const decoder = new TextDecoder('utf8')
        const sessionData = decoder.decode(message.getData() as ArrayBuffer)

        if (msgType === MessageType.WEBRTC_ICE_CANDIDATE) {
          try {
            const candidate = JSON.parse(sessionData)
            await this.webRtcConn!.addIceCandidate(candidate)
          } catch (err) {
            this.logger.error(err)
          }
        } else if (msgType === MessageType.WEBRTC_OFFER) {
          try {
            await this.webRtcConn!.setRemoteDescription(JSON.parse(sessionData))
            const desc = await this.webRtcConn!.createAnswer({})
            await this.webRtcConn!.setLocalDescription(desc)

            let answer = this.webRtcConn!.localDescription

            if (answer && answer.sdp) {
              const msg = new WebRtcMessage()
              msg.setToAlias(this.commServerAlias)
              msg.setType(MessageType.WEBRTC_ANSWER)
              const encoder = new TextEncoder()
              const data = encoder.encode(JSON.stringify(answer))
              msg.setData(data)
              this.sendCoordinatorMessage(msg)
            }

            this.pendingCandidates.forEach(candidate => {
              this.sendICECandidate(candidate)
            })
            this.pendingCandidates = []
          } catch (err) {
            this.logger.error(err)
          }
        } else if (msgType === MessageType.WEBRTC_ANSWER) {
          try {
            await this.webRtcConn!.setRemoteDescription(JSON.parse(sessionData))
          } catch (err) {
            this.logger.error(err)
          }
        }
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

  private onConnectionError(error: Error) {
    if (this.reliableFuture.isPending) {
      this.reliableFuture.reject(error)
    }
    if (this.unreliableFuture.isPending) {
      this.unreliableFuture.reject(error)
    }
    this.stats && this.stats.printDebugInformation()
    this.close()
  }

  private sendCoordinatorMessage = (msg: Message) => {
    if (!this.ws || this.ws.readyState !== SocketReadyState.OPEN) {
      throw new Error('try to send answer to a non ready ws')
    }

    const bytes = msg.serializeBinary()

    if (this.stats) {
      this.stats.webRtcSession.incrementSent(1, bytes.length)
    }

    this.ws.send(bytes)
  }

  private connectRTC() {
    this.webRtcConn = new RTCPeerConnection({
      iceServers: commConfigurations.iceServers
    })

    this.webRtcConn.onsignalingstatechange = (e: Event) => {
      this.logger.log(`signaling state: ${this.webRtcConn!.signalingState}`)
    }

    this.webRtcConn.oniceconnectionstatechange = (e: Event) => {
      this.logger.log(`ice connection state: ${this.webRtcConn!.iceConnectionState}`)
    }

    this.webRtcConn.ondatachannel = this.onDataChannel
    this.webRtcConn.onicecandidate = this.onIceCandidate
  }

  private connectWS() {
    if (this.ws) {
      this.ws.close()
      this.ws = null
    }

    this.ws = new WebSocket(this.url)
    this.ws.binaryType = 'arraybuffer'

    this.ws.onerror = event => {
      this.logger.error('socket error', event)
      this.ws = null
    }

    this.ws.onmessage = event => {
      this.onWsMessage(event).catch(err => {
        this.logger.error(err)
      })
    }
  }

  private onIceCandidate = (event: RTCPeerConnectionIceEvent) => {
    if (event.candidate && event.candidate.candidate) {
      if (this.webRtcConn!.remoteDescription) {
        this.sendICECandidate(event.candidate)
      } else {
        this.pendingCandidates.push(event.candidate)
      }
    } else {
      this.gotCandidatesFuture.resolve(this.webRtcConn!.localDescription!)
    }
  }

  private sendICECandidate = (candidate: RTCIceCandidate) => {
    const msg = new WebRtcMessage()
    msg.setType(MessageType.WEBRTC_ICE_CANDIDATE)
    // TODO: Ensure commServerAlias, it may be null
    msg.setToAlias(this.commServerAlias!)

    const encoder = new TextEncoder()
    const data = encoder.encode(JSON.stringify(candidate.toJSON()))
    msg.setData(data)

    this.sendCoordinatorMessage(msg)
  }

  private onDataChannel = (e: RTCDataChannelEvent) => {
    let dc = e.channel

    dc.onclose = () => {
      this.logger.log(`DataChannel ${JSON.stringify(dc.label)} has closed`)
    }

    dc.onopen = async () => {
      const label = dc.label
      this.logger.log(`DataChannel ${JSON.stringify(dc.label)} has opened`)

      if (label === 'reliable') {
        this.reliableDataChannel = dc
        const authData = new AuthData()
        const credentials = await this.auth.getMessageCredentials('')
        authData.setSignature(credentials['x-signature'])
        authData.setIdentity(credentials['x-identity'])
        authData.setTimestamp(credentials['x-timestamp'])
        authData.setAccessToken(credentials['x-access-token'])
        const authMessage = new AuthMessage()
        authMessage.setType(MessageType.AUTH)
        authMessage.setRole(Role.CLIENT)
        authMessage.setBody(authData.serializeBinary())
        const bytes = authMessage.serializeBinary()

        if (dc.readyState === 'open') {
          dc.send(bytes)
          this.authenticated = true
          this.reliableFuture.resolve()
        } else {
          this.logger.error('cannot send authentication, data channel is not ready')
        }
      } else if (label === 'unreliable') {
        this.unreliableFuture.resolve()
        this.unreliableDataChannel = dc
      }
    }

    dc.onmessage = (e: MessageEvent) => {
      if (this.stats) {
        this.stats.dispatchTopicDuration.start()
      }

      const data = e.data
      const msg = new Uint8Array(data)

      this.onMessageObservable.notifyObservers({ data: msg, channel: dc.label })
    }
  }
}
