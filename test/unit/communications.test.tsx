import { future, IFuture } from 'fp-future'
import * as chai from 'chai'
import * as sinon from 'sinon'
import * as sinonChai from 'sinon-chai'

import { parcelLimits } from 'config'
import {
  WebRtcMessage,
  ConnectMessage,
  WelcomeMessage,
  AuthMessage,
  TopicMessage,
  TopicFWMessage,
  PingMessage,
  SubscriptionMessage,
  MessageType,
  Role,
  Format,
  TopicIdentityMessage
} from '../../packages/shared/comms/proto/broker'
import { AuthData } from '../../packages/shared/comms/proto/comms'
import { PositionData, ProfileData, ChatData, Category } from '../../packages/shared/comms/proto/comms'
import { Position, CommunicationArea, Parcel, position2parcel } from 'shared/comms/utils'
import { WorldInstanceConnection, SocketReadyState, positionHash } from 'shared/comms/worldInstanceConnection'
import {
  Context,
  processChatMessage,
  processPositionMessage,
  processProfileMessage,
  PeerTrackingInfo,
  onPositionUpdate
} from 'shared/comms'
import { BrokerConnection } from 'shared/comms/BrokerConnection'
import { IBrokerConnection, BrokerMessage } from 'shared/comms/IBrokerConnection'
import { Observable } from 'decentraland-ecs/src'
import { TopicIdentityFWMessage } from '../../packages/shared/comms/proto/broker'

chai.use(sinonChai)

const expect = chai.expect

let webSocket!: WebSocket

class MockWebSocket {
  public readyState: SocketReadyState = SocketReadyState.CLOSED

  constructor(public url: string) {
    webSocket = this as any
  }
}

describe('Communications', function() {
  describe('CommunicationArea', () => {
    function makePosition(x: number, z: number): Position {
      return [x, 0, z, 0, 0, 0, 0]
    }

    it('should contain the center', () => {
      const area = new CommunicationArea(new Parcel(0, 0), 2)
      expect(area.contains(makePosition(0, 0))).to.be.true
    })

    it('should contain the vertexes', () => {
      const area = new CommunicationArea(new Parcel(0, 0), 2)
      expect(area.contains(makePosition(2, 2))).to.be.true
      expect(area.contains(makePosition(-2, 0))).to.be.true
      expect(area.contains(makePosition(-2, -2))).to.be.true
      expect(area.contains(makePosition(2, -2))).to.be.true
    })

    it('should contain a point outside the area', () => {
      const area = new CommunicationArea(new Parcel(0, 0), 2)
      expect(area.contains(makePosition(1.3, 0))).to.be.true
    })

    it('should not contain a point outside the area', () => {
      const area = new CommunicationArea(new Parcel(0, 0), 2)
      expect(area.contains(makePosition(60, 0))).to.be.false
    })
  })

  describe('WorldInstanceConnection', () => {
    const ORIGINAL_WEB_SOCKET = (window as any)['WebSocket']

    before(() => {
      // NOTE: little hack to be able to mock websocket requests
      ;(window as any)['WebSocket'] = MockWebSocket
    })

    let connection: BrokerConnection
    let worldConn: WorldInstanceConnection
    let mockWebRtc: any
    let auth: any = {
      getMessageCredentials: async (msg: string) => {
        return {
          'x-signature': 'signature',
          'x-identity': 'identity',
          'x-timestamp': 'timestamp',
          'x-access-token': 'access token'
        }
      }
    }

    beforeEach(() => {
      webSocket = null as any
      connection = new BrokerConnection(auth, '')
      worldConn = new WorldInstanceConnection(connection)

      mockWebRtc = {
        addIceCandidate: sinon.stub(),
        setRemoteDescription: sinon.stub(),
        setLocalDescription: sinon.stub(),
        createAnswer: sinon.stub(),
        onicecandidate: connection.webRtcConn!.onicecandidate,
        ondatachannel: connection.webRtcConn!.ondatachannel
      }
      connection.webRtcConn = mockWebRtc
      ;(webSocket as any).readyState = SocketReadyState.OPEN

      webSocket.send = sinon.stub()
    })

    after(() => {
      ;(window as any)['WebSocket'] = ORIGINAL_WEB_SOCKET
    })

    describe('coordinator messages', () => {
      it('welcome', async () => {
        const msg = new WelcomeMessage()
        msg.setType(MessageType.WELCOME)
        msg.setAlias(1)
        msg.setAvailableServersList([1])

        const event = new MessageEvent('websocket', { data: msg.serializeBinary() })
        await connection.onWsMessage(event)

        expect(connection.alias).to.equal('1')

        expect(webSocket.send).to.have.been.calledWithMatch((bytes: Uint8Array) => {
          const msgType = ConnectMessage.deserializeBinary(bytes).getType()
          expect(msgType).to.equal(MessageType.CONNECT)
          return true
        })
      })

      it('webrtc ice candidate (from unknown peer)', async () => {
        const msg = new WebRtcMessage()
        msg.setType(MessageType.WEBRTC_ICE_CANDIDATE)
        msg.setFromAlias(1)

        const event = new MessageEvent('websocket', { data: msg.serializeBinary() })

        await connection.onWsMessage(event)

        expect(connection.webRtcConn!.addIceCandidate).to.not.have.been.called
      })

      it('webrtc ice candidate', async () => {
        connection.commServerAlias = 1
        const msg = new WebRtcMessage()
        msg.setType(MessageType.WEBRTC_ICE_CANDIDATE)
        msg.setFromAlias(1)
        const candidate = {
          candidate: 'candidate:702786350 2 udp 41819902 8.8.8.8 60769 typ relay raddr 8.8.8.8'
        }

        const encoder = new TextEncoder()
        msg.setData(encoder.encode(JSON.stringify(candidate)))

        const event = new MessageEvent('websocket', { data: msg.serializeBinary() })

        mockWebRtc.addIceCandidate.resolves()

        await connection.onWsMessage(event)

        expect(connection.webRtcConn!.addIceCandidate).to.have.been.calledWith(candidate)
      })

      it('webrtc offer', async () => {
        connection.commServerAlias = 1

        const answer = { sdp: 'answer-sdp', type: 'answer' }
        ;(connection.webRtcConn!.createAnswer as any).resolves(answer)

        const offer = { sdp: 'offer-sdp', type: 'offer' }

        const msg = new WebRtcMessage()
        msg.setType(MessageType.WEBRTC_OFFER)
        msg.setFromAlias(1)
        const encoder = new TextEncoder()
        msg.setData(encoder.encode(JSON.stringify(offer)))
        ;(connection.webRtcConn! as any)['localDescription'] = answer
        connection.gotCandidatesFuture.resolve(answer as any)

        const event = new MessageEvent('websocket', { data: msg.serializeBinary() })
        await connection.onWsMessage(event)

        expect(connection.webRtcConn!.setRemoteDescription).to.have.been.calledWithMatch(
          (desc: RTCSessionDescription) => {
            expect(desc.type).to.be.equal('offer')
            expect(desc.sdp).to.be.equal('offer-sdp')
            return true
          }
        )

        expect(connection.webRtcConn!.createAnswer).to.have.been.called
        expect(connection.webRtcConn!.setLocalDescription).to.have.been.calledWith(answer)

        expect(webSocket.send).to.have.been.calledWithMatch((bytes: Uint8Array) => {
          const msg = WebRtcMessage.deserializeBinary(bytes)
          expect(msg.getType()).to.equal(MessageType.WEBRTC_ANSWER)
          expect(msg.getToAlias()).to.equal(1)

          const decoder = new TextDecoder('utf8')
          const r = JSON.parse(decoder.decode(msg.getData() as ArrayBuffer))
          expect(r.sdp).to.be.equal(answer.sdp)
          expect(r.type).to.be.equal(answer.type)
          return true
        })
      })

      it('webrtc answer', async () => {
        const answer = { sdp: 'answer-sdp', type: 'answer' }

        connection.commServerAlias = 1
        const msg = new WebRtcMessage()
        msg.setType(MessageType.WEBRTC_ANSWER)
        msg.setFromAlias(1)
        const encoder = new TextEncoder()
        msg.setData(encoder.encode(JSON.stringify(answer)))

        const event = new MessageEvent('websocket', { data: msg.serializeBinary() })
        await connection.onWsMessage(event)

        expect(connection.webRtcConn!.setRemoteDescription).to.have.been.calledWithMatch(
          (desc: RTCSessionDescription) => {
            expect(desc.type).to.be.equal('answer')
            expect(desc.sdp).to.be.equal('answer-sdp')
            return true
          }
        )
      })
    })

    describe('webrtc', () => {
      it('onicecandidate', () => {
        connection.commServerAlias = 1

        const sdp = 'candidate:702786350 2 udp 41819902 8.8.8.8 60769 typ relay raddr 8.8.8.8'
        const candidate = new RTCIceCandidate({ candidate: sdp, sdpMid: '0', sdpMLineIndex: 0 })
        const event = new RTCPeerConnectionIceEvent('icecandidate', { candidate })
        connection.webRtcConn!.onicecandidate!(event)
        expect(webSocket.send).to.have.been.calledWithMatch((bytes: Uint8Array) => {
          const msg = WebRtcMessage.deserializeBinary(bytes)
          expect(msg.getType()).to.equal(MessageType.WEBRTC_ICE_CANDIDATE)
          expect(msg.getToAlias()).to.equal(1)
          const decoder = new TextDecoder('utf8')
          const r = JSON.parse(decoder.decode(msg.getData() as ArrayBuffer))
          expect(r.candidate).to.be.equal(candidate.candidate)
          return true
        })
      })

      describe('reliable data channel', () => {
        let channel: any

        beforeEach(async () => {
          connection.commServerAlias = 1
          channel = Object.defineProperties(new RTCPeerConnection().createDataChannel('reliable'), {
            readyState: { value: 'open' },
            send: { value: sinon.stub() }
          })
          const event = new RTCDataChannelEvent('datachannel', { channel })
          connection.webRtcConn!.ondatachannel!(event)

          await channel['onopen']()
        })

        it('register datachannel', () => {
          expect(connection.reliableDataChannel).to.equal(channel)
          expect(connection.authenticated).to.be.true
          expect(channel.send).to.have.been.calledWithMatch((bytes: Uint8Array) => {
            const msg = AuthMessage.deserializeBinary(bytes)
            expect(msg.getType()).to.equal(MessageType.AUTH)
            expect(msg.getRole()).to.equal(Role.CLIENT)

            const authData = AuthData.deserializeBinary(msg.getBody() as Uint8Array)
            expect(authData.getSignature()).to.equal('signature')
            expect(authData.getIdentity()).to.equal('identity')
            expect(authData.getTimestamp()).to.equal('timestamp')
            expect(authData.getAccessToken()).to.equal('access token')

            return true
          })
        })

        it('receive position data message', () => {
          worldConn.positionHandler = sinon.stub()

          const body = new PositionData()
          body.setCategory(Category.POSITION)
          body.setTime(Date.now())

          const bodyEncoded = body.serializeBinary()

          const msg = new TopicFWMessage()
          msg.setType(MessageType.TOPIC_FW)
          msg.setFromAlias(1)
          msg.setBody(bodyEncoded)

          const e = { data: msg.serializeBinary() }
          channel.onmessage(e)
          expect(worldConn.positionHandler).to.have.been.calledWith('1', body)
        })

        it('receive profile data message', () => {
          worldConn.profileHandler = sinon.stub()

          const body = new ProfileData()
          body.setCategory(Category.PROFILE)
          body.setTime(Date.now())

          const bodyEncoded = body.serializeBinary()

          const msg = new TopicIdentityFWMessage()
          msg.setType(MessageType.TOPIC_IDENTITY_FW)
          msg.setFromAlias(1)
          msg.setIdentity(btoa('id'))
          msg.setBody(bodyEncoded)

          const e = { data: msg.serializeBinary() }
          channel.onmessage(e)
          expect(worldConn.profileHandler).to.have.been.calledWith('1', 'id', body)
        })

        it('receive chat data message', () => {
          worldConn.chatHandler = sinon.stub()

          const body = new ChatData()
          body.setCategory(Category.CHAT)
          body.setTime(Date.now())

          const bodyEncoded = body.serializeBinary()

          const msg = new TopicFWMessage()
          msg.setType(MessageType.TOPIC_FW)
          msg.setFromAlias(1)
          msg.setBody(bodyEncoded)

          const e = { data: msg.serializeBinary() }
          channel.onmessage(e)
          expect(worldConn.chatHandler).to.have.been.calledWith('1', body)
        })

        it('receive ping message', () => {
          const msg = new PingMessage()
          msg.setType(MessageType.PING)
          msg.setTime(new Date(2008).getTime())

          const e = { data: msg.serializeBinary() }
          channel.onmessage(e)
          expect(worldConn.ping).to.be.gt(0)
        })
      })

      it('register datachannel (unreliable)', () => {
        connection.commServerAlias = 1

        const channel = Object.defineProperties(new RTCPeerConnection().createDataChannel('unreliable'), {
          readyState: { value: 'open' },
          send: { value: sinon.stub() }
        })

        const event = new RTCDataChannelEvent('datachannel', { channel })
        connection.webRtcConn!.ondatachannel!(event)
        channel['onopen']()

        expect(connection.unreliableDataChannel).to.equal(channel)
        expect(connection.authenticated).to.be.false
      })

      describe('outbound messages', () => {
        beforeEach(() => {
          connection.reliableDataChannel = {
            readyState: 'open',
            send: sinon.stub()
          } as any
          connection.unreliableDataChannel = {
            readyState: 'open',
            send: sinon.stub()
          } as any
        })

        it('topic subscriptions', () => {
          worldConn.updateSubscriptions('topic1 topic2')
          expect(connection.reliableDataChannel!.send).to.have.been.calledWithMatch((bytes: Uint8Array) => {
            const msg = SubscriptionMessage.deserializeBinary(bytes)
            expect(msg.getType()).to.equal(MessageType.SUBSCRIPTION)
            expect(msg.getFormat()).to.equal(Format.PLAIN)
            expect(Buffer.from(msg.getTopics()).toString('utf8')).to.equal('topic1 topic2')
            return true
          })
        })

        it('position', () => {
          const p = [20, 20, 20, 20, 20, 20, 20] as Position

          worldConn.sendPositionMessage(p)

          expect(connection.unreliableDataChannel!.send).to.have.been.calledWithMatch((bytes: Uint8Array) => {
            const msg = TopicMessage.deserializeBinary(bytes)
            expect(msg.getType()).to.equal(MessageType.TOPIC)
            expect(msg.getTopic()).to.equal('37:37')

            const data = PositionData.deserializeBinary(msg.getBody() as Uint8Array)
            expect(data.getPositionX()).to.equal(20)
            expect(data.getPositionY()).to.equal(20)
            expect(data.getPositionZ()).to.equal(20)
            expect(data.getRotationX()).to.equal(20)
            expect(data.getRotationY()).to.equal(20)
            expect(data.getRotationZ()).to.equal(20)
            expect(data.getRotationW()).to.equal(20)
            return true
          })
        })

        it('profile', () => {
          const p = [20, 20, 20, 20, 20, 20, 20] as Position
          const profile = {
            version: 'version'
          }
          worldConn.sendProfileMessage(p, profile)

          expect(connection.reliableDataChannel!.send).to.have.been.calledWithMatch((bytes: Uint8Array) => {
            const msg = TopicIdentityMessage.deserializeBinary(bytes)
            expect(msg.getType()).to.equal(MessageType.TOPIC_IDENTITY)
            expect(msg.getTopic()).to.equal('37:37')

            const data = ProfileData.deserializeBinary(msg.getBody() as Uint8Array)
            expect(data.getProfileVersion()).to.equal('version')
            return true
          })
        })

        it('chat', () => {
          const p = [20, 20, 20, 20, 20, 20, 20] as Position
          const messageId = 'chat1'
          const text = 'hello'
          worldConn.sendChatMessage(p, messageId, text)
          expect(connection.reliableDataChannel!.send).to.have.been.calledWithMatch((bytes: Uint8Array) => {
            const msg = TopicMessage.deserializeBinary(bytes)
            expect(msg.getType()).to.equal(MessageType.TOPIC)
            expect(msg.getTopic()).to.equal('37:37')

            const data = ChatData.deserializeBinary(msg.getBody() as Uint8Array)
            expect(data.getMessageId()).to.equal(messageId)
            expect(data.getText()).to.equal(text)
            return true
          })
        })
      })
    })
  })

  describe('topic handlers', () => {
    it('chat handler', () => {
      const context = new Context({})
      const chatData = new ChatData()
      chatData.setText('text')
      chatData.setMessageId('chat1')
      processChatMessage(context, 'client2', chatData)

      expect(context.peerData).to.have.key('client2')
      expect(context.peerData.get('client2')!.receivedPublicChatMessages).to.have.key('chat1')
    })

    describe('position handler', () => {
      it('new position', () => {
        const context = new Context({})
        const positionData = new PositionData()

        positionData.setTime(Date.now())
        positionData.setPositionX(20)
        positionData.setPositionY(20)
        positionData.setPositionZ(20)
        positionData.setRotationX(20)
        positionData.setRotationY(20)
        positionData.setRotationZ(20)
        positionData.setRotationW(20)

        processPositionMessage(context, 'client2', positionData)

        expect(context.peerData).to.have.key('client2')
        const trackingInfo = context.peerData.get('client2') as PeerTrackingInfo
        expect(trackingInfo.position).to.deep.equal([20, 20, 20, 20, 20, 20, 20])
      })

      it('old position', () => {
        const context = new Context({})
        const info = new PeerTrackingInfo()
        info.lastPositionUpdate = Date.now()
        info.position = [20, 20, 20, 20, 20, 20, 20]
        context.peerData.set('client2', info)
        const positionData = new PositionData()

        positionData.setTime(new Date(2008).getTime())
        positionData.setPositionX(30)
        positionData.setPositionY(30)
        positionData.setPositionZ(30)
        positionData.setRotationX(30)
        positionData.setRotationY(30)
        positionData.setRotationZ(30)
        positionData.setRotationW(30)

        processPositionMessage(context, 'client2', positionData)

        expect(context.peerData).to.have.key('client2')
        const trackingInfo = context.peerData.get('client2') as PeerTrackingInfo
        expect(trackingInfo.position).to.deep.equal([20, 20, 20, 20, 20, 20, 20])
      })
    })

    describe('profile handler', () => {
      it('new profile message', () => {
        const context = new Context({})

        const profileData = new ProfileData()
        profileData.setTime(Date.now())
        profileData.setProfileVersion('version')

        const info = new PeerTrackingInfo()
        sinon.spy(info, 'loadProfileIfNecessary')
        context.peerData.set('client2', info)

        processProfileMessage(context, 'client2', 'userId1', profileData)

        expect(context.peerData).to.have.key('client2')
        const trackingInfo = context.peerData.get('client2') as PeerTrackingInfo
        expect(trackingInfo.identity).to.equal('userId1')
        expect(trackingInfo.userInfo).to.deep.equal({
          userId: 'userId1'
        })
        expect(trackingInfo.loadProfileIfNecessary).to.have.been.calledWith('version')
      })

      it('old profile message', () => {
        const context = new Context({})
        const info = new PeerTrackingInfo()
        info.lastProfileUpdate = Date.now()
        info.identity = 'identity2'
        sinon.spy(info, 'loadProfileIfNecessary')
        context.peerData.set('client2', info)

        const profileData = new ProfileData()
        profileData.setTime(new Date(2008).getTime())
        profileData.setProfileVersion('version1')

        processProfileMessage(context, 'client2', 'identity2', profileData)

        expect(context.peerData).to.have.key('client2')
        const trackingInfo = context.peerData.get('client2') as PeerTrackingInfo
        expect(trackingInfo.loadProfileIfNecessary).to.not.have.been.calledWith('version1')
      })
    })
  })

  describe('positionHash', () => {
    function testHashByPosition(p: Position) {
      it(`hash ${p[0]}:${p[2]}`, () => {
        const parcel = position2parcel(p)
        const hash = positionHash(p)
        const [x, z] = hash.split(':').map(n => parseInt(n, 10))

        const unhashX = (x << 2) - parcelLimits.maxParcelX
        const unhashZ = (z << 2) - parcelLimits.maxParcelZ

        expect(parcel.x).to.be.gte(unhashX)
        expect(parcel.x).to.be.lte(unhashX + 4)
        expect(parcel.z).to.be.gte(unhashZ)
        expect(parcel.z).to.be.lte(unhashZ + 4)
      })
    }

    function testHashByParcel(parcel: Parcel) {
      it(`parcel ${parcel.x}:${parcel.z}`, () => {
        const p = [parcel.x * parcelLimits.parcelSize, 0, parcel.z * parcelLimits.parcelSize, 0, 0, 0, 0] as Position
        const hash = positionHash(p)
        const [x, z] = hash.split(':').map(n => parseInt(n, 10))

        const unhashX = (x << 2) - 150
        const unhashZ = (z << 2) - 150

        expect(parcel.x).to.be.gte(unhashX)
        expect(parcel.x).to.be.lte(unhashX + 4)
        expect(parcel.z).to.be.gte(unhashZ)
        expect(parcel.z).to.be.lte(unhashZ + 4)
      })
    }

    testHashByPosition([1000, 0, 1000, 0, 0, 0, 0])
    testHashByPosition([21, 0, 21, 0, 0, 0, 0])
    testHashByPosition([-1, 0, -1, 0, 0, 0, 0])
    testHashByPosition([-1, 0, 0, 0, 0, 0, 0])
    testHashByPosition([-1, 0, 1, 0, 0, 0, 0])
    testHashByPosition([0, 0, -1, 0, 0, 0, 0])
    testHashByPosition([0, 0, 0, 0, 0, 0, 0])
    testHashByPosition([0, 0, 1, 0, 0, 0, 0])
    testHashByPosition([1, 0, -1, 0, 0, 0, 0])
    testHashByPosition([1, 0, 0, 0, 0, 0, 0])
    testHashByPosition([1, 0, 1, 0, 0, 0, 0])

    testHashByParcel(new Parcel(20, 20))
    testHashByParcel(new Parcel(21, 21))
    testHashByParcel(new Parcel(-1, -1))
    testHashByParcel(new Parcel(-1, 0))
    testHashByParcel(new Parcel(-1, 1))
    testHashByParcel(new Parcel(0, -1))
    testHashByParcel(new Parcel(0, 0))
    testHashByParcel(new Parcel(0, 1))
    testHashByParcel(new Parcel(1, -1))
    testHashByParcel(new Parcel(1, 0))
    testHashByParcel(new Parcel(1, 1))
  })

  class BrokerMock implements IBrokerConnection {
    onMessageObservable = new Observable<BrokerMessage>()
    connected = future<void>()
    stats = null
    get hasUnreliableChannel(): boolean {
      return true
    }
    get hasReliableChannel(): boolean {
      return true
    }

    get isConnected(): IFuture<void> {
      this.connected.resolve()
      return this.connected
    }

    get isAuthenticated(): boolean {
      return true
    }

    reliableDataChannel = { readyState: 'open', send: sinon.stub() }
    unreliableDataChannel = { readyState: 'open', send: sinon.stub() }

    sendReliable(data: Uint8Array): void {
      this.reliableDataChannel.send(data)
    }

    sendUnreliable(data: Uint8Array): void {
      this.unreliableDataChannel.send(data)
    }

    printDebugInformation(): void {
      throw new Error('Method not implemented.')
    }
    close(): void {
      throw new Error('Method not implemented.')
    }
  }

  describe('onPositionUpdate', () => {
    it('parcel has changed', () => {
      const context = new Context({})
      context.commRadius = 1
      context.currentPosition = [20, 20, 20, 20, 20, 20, 20] as Position
      const connection = new BrokerMock()
      const worldConn = new WorldInstanceConnection(connection)
      worldConn.updateSubscriptions = sinon.stub()
      context.worldInstanceConnection = worldConn

      onPositionUpdate(context, [0, 0, 0, 0, 0, 0, 0])

      expect(worldConn.updateSubscriptions).to.have.been.calledWithMatch((rawTopics: string) => {
        expect(rawTopics.split(' ')).to.have.length(1)
        return true
      })
      expect(worldConn.updateSubscriptions).to.have.been.calledWithMatch((rawTopics: string) => {
        expect(rawTopics.split(' ')).to.have.length(1)
        return true
      })
    })

    it('parcel has not changed', () => {
      const context = new Context({})
      context.commRadius = 1
      context.currentPosition = [20, 20, 20, 20, 20, 20, 20] as Position
      const connection = new BrokerMock()
      const worldConn = new WorldInstanceConnection(connection)
      worldConn.updateSubscriptions = sinon.stub()
      context.worldInstanceConnection = worldConn

      onPositionUpdate(context, context.currentPosition)

      expect(worldConn.updateSubscriptions).to.have.not.been.called
    })
  })
})
