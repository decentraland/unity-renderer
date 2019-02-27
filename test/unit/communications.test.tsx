import * as chai from 'chai'
import * as sinon from 'sinon'
import * as sinonChai from 'sinon-chai'

import {
  WebRtcMessage, ConnectMessage, WelcomeMessage, AuthMessage, TopicMessage, PingMessage,
  PositionData, ProfileData, ChatData,
  ChangeTopicMessage, MessageType, Role
} from '../../packages/shared/comms/commproto_pb'
import { Position, CommunicationArea, Parcel } from 'shared/comms/utils'
import { WorldInstanceConnection, SocketReadyState } from 'shared/comms/worldInstanceConnection'
import { Context, processChatMessage, processPositionMessage, processProfileMessage, PeerTrackingInfo, onPositionUpdate } from 'shared/comms'
import { PkgStats } from 'shared/comms/debug'

chai.use(sinonChai)

const expect = chai.expect

let webSocket = null

class MockWebSocket {
  public readyState: SocketReadyState = SocketReadyState.CLOSED

  constructor(public url: string) {
    webSocket = this
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

    const ORIGINAL_WEB_SOCKET = window['WebSocket']

    before(() => {
      // NOTE: little hack to be able to mock websocket requests
      window['WebSocket'] = MockWebSocket
    })

    let worldConn
    beforeEach(() => {
      webSocket = null
      worldConn = new WorldInstanceConnection('coordinator')
      worldConn.connect()

      const mockWebRtc = {
        addIceCandidate: sinon.stub(),
        setRemoteDescription: sinon.stub(),
        setLocalDescription: sinon.stub(),
        createAnswer: sinon.stub(),
        onicecandidate: worldConn.webRtcConn.onicecandidate,
        ondatachannel: worldConn.webRtcConn.ondatachannel
      }
      worldConn.webRtcConn = mockWebRtc
      webSocket.readyState = SocketReadyState.OPEN
      webSocket.send = sinon.stub()
    })

    after(() => {
      window['WebSocket'] = ORIGINAL_WEB_SOCKET
    })

    describe('coordinator messages', () => {
      it('welcome', async () => {
        const msg = new WelcomeMessage()
        msg.setType(MessageType.WELCOME)
        msg.setAlias('client1')
        msg.setAvailableServersList(['server1'])

        await webSocket.onmessage({ data: msg.serializeBinary() })

        expect(worldConn.alias).to.equal('client1')

        expect(webSocket.send).to.have.been.calledWithMatch((bytes) => {
          const msgType = ConnectMessage.deserializeBinary(bytes).getType()
          expect(msgType).to.equal(MessageType.CONNECT)
          return true
        })
      })

      it('webrtc ice candidate (from unknown peer)', async () => {
        const msg = new WebRtcMessage()
        msg.setType(MessageType.WEBRTC_ICE_CANDIDATE)
        msg.setFromAlias('server2')

        await webSocket.onmessage({ data: msg.serializeBinary() })

        expect(worldConn.webRtcConn.addIceCandidate).to.not.have.been.called
      })

      it('webrtc ice candidate', async () => {
        worldConn.commServerAlias = 'server1'
        const msg = new WebRtcMessage()
        msg.setType(MessageType.WEBRTC_ICE_CANDIDATE)
        msg.setFromAlias('server1')
        msg.setSdp('sdp')

        await webSocket.onmessage({ data: msg.serializeBinary() })

        expect(worldConn.webRtcConn.addIceCandidate).to.have.been.calledWith('sdp')
      })

      it('webrtc offer', async () => {
        worldConn.commServerAlias = 'server1'

        const answer = { sdp: "answer-sdp" }
        worldConn.webRtcConn.createAnswer.resolves(answer)

        const msg = new WebRtcMessage()
        msg.setType(MessageType.WEBRTC_OFFER)
        msg.setFromAlias('server1')
        msg.setSdp('sdp')

        await webSocket.onmessage({ data: msg.serializeBinary() })

        expect(worldConn.webRtcConn.setRemoteDescription).to.have.been.calledWithMatch((desc) => {
          expect(desc.type).to.be.equal('offer')
          expect(desc.sdp).to.be.equal('sdp')
          return true
        })

        expect(worldConn.webRtcConn.createAnswer).to.have.been.called
        expect(worldConn.webRtcConn.setLocalDescription).to.have.been.calledWith(answer)

        expect(webSocket.send).to.have.been.calledWithMatch((bytes) => {
          const msg = WebRtcMessage.deserializeBinary(bytes)
          expect(msg.getType()).to.equal(MessageType.WEBRTC_ANSWER)
          expect(msg.getSdp()).to.equal(answer.sdp)
          expect(msg.getToAlias()).to.equal('server1')
          return true
        })
      })

      it('webrtc answer', async () => {
        worldConn.commServerAlias = 'server1'
        const msg = new WebRtcMessage()
        msg.setType(MessageType.WEBRTC_ANSWER)
        msg.setFromAlias('server1')
        msg.setSdp('sdp')

        await webSocket.onmessage({ data: msg.serializeBinary() })

        expect(worldConn.webRtcConn.setRemoteDescription).to.have.been.calledWithMatch((desc) => {
          expect(desc.type).to.be.equal('answer')
          expect(desc.sdp).to.be.equal('sdp')
          return true
        })
      })
    })

    describe('webrtc', () => {
      it('onicecandidate', () => {
        worldConn.commServerAlias = 'server1'
        const event = {
          candidate: {
            candidate: 'candidate-sdp'
          }
        }
        worldConn.webRtcConn.onicecandidate(event)

        expect(webSocket.send).to.have.been.calledWithMatch((bytes) => {
          const msg = WebRtcMessage.deserializeBinary(bytes)
          expect(msg.getType()).to.equal(MessageType.WEBRTC_ICE_CANDIDATE)
          expect(msg.getToAlias()).to.equal('server1')
          expect(msg.getSdp()).to.equal('candidate-sdp')
          return true
        })
      })

      describe('reliable data channel', () => {
        let channel

        beforeEach(() => {
          worldConn.commServerAlias = 'server1'
          channel = {
            send: sinon.stub(),
            label: 'reliable'
          }

          const event = { channel }

          worldConn.webRtcConn.ondatachannel(event)

          channel['onopen']()

        })

        it('register datachannel', () => {
          expect(worldConn.reliableDataChannel).to.equal(channel)
          expect(worldConn.authenticated).to.be.true

          expect(channel.send).to.have.been.calledWithMatch((bytes) => {
            const msg = AuthMessage.deserializeBinary(bytes)
            expect(msg.getType()).to.equal(MessageType.AUTH)
            expect(msg.getRole()).to.equal(Role.CLIENT)
            expect(msg.getMethod()).to.equal('noop')
            return true
          })
        })

        it('receive topic message, no handler registered', () => {
          const msg = new TopicMessage()
          msg.setType(MessageType.TOPIC)
          msg.setFromAlias('client2')
          msg.setTopic('topic1')

          const e = { data: msg.serializeBinary() }
          channel.onmessage(e)
        })

        it('receive topic message, handler registered', () => {
          const handler = sinon.stub()
          worldConn.subscriptions.set('topic1', handler)
          const msg = new TopicMessage()
          msg.setType(MessageType.TOPIC)
          msg.setFromAlias('client2')
          msg.setTopic('topic1')

          const e = { data: msg.serializeBinary() }
          channel.onmessage(e)
          expect(handler).to.have.been.calledWith('client2')
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
        worldConn.commServerAlias = 'server1'
        const channel = {
          send: sinon.stub(),
          label: 'unreliable'
        }

        const event = { channel }

        worldConn.webRtcConn.ondatachannel(event)

        channel['onopen']()

        expect(worldConn.unreliableDataChannel).to.equal(channel)
        expect(worldConn.authenticated).to.be.false
      })

      describe('outbound messages', () => {
        beforeEach(() => {
          worldConn.reliableDataChannel = {
            send: sinon.stub()
          }
          worldConn.unreliableDataChannel = {
            send: sinon.stub()
          }
        })

        it('add topic', () => {
          const handler = (fromAlias: string, data:Uint8Array): PkgStats | null => {
            return null
          }
          worldConn.addTopic('topic1', handler)

          expect(worldConn.subscriptions).to.have.key('topic1')
          expect(worldConn.subscriptions.get('topic1')).to.equal(handler)
          expect(worldConn.reliableDataChannel.send).to.have.been.calledWithMatch((bytes) => {
            const msg = ChangeTopicMessage.deserializeBinary(bytes)
            expect(msg.getType()).to.equal(MessageType.ADD_TOPIC)
            expect(msg.getTopic()).to.equal('topic1')
            return true
          })
        })

        it('remove topic', () => {
          const handler = (fromAlias: string, data:Uint8Array): PkgStats | null => {
            return null
          }
          worldConn.subscriptions.set('topic1', handler)

          worldConn.removeTopic('topic1')

          expect(worldConn.subscriptions).to.not.have.key('topic1')
          expect(worldConn.reliableDataChannel.send).to.have.been.calledWithMatch((bytes) => {
            const msg = ChangeTopicMessage.deserializeBinary(bytes)
            expect(msg.getType()).to.equal(MessageType.REMOVE_TOPIC)
            expect(msg.getTopic()).to.equal('topic1')
            return true
          })
        })

        it('position', () => {
          const p = [ 20, 20, 20, 20, 20, 20, 20 ]
          worldConn.sendPositionMessage(p)

          expect(worldConn.unreliableDataChannel.send).to.have.been.calledWithMatch((bytes) => {
            const msg = TopicMessage.deserializeBinary(bytes)
            expect(msg.getType()).to.equal(MessageType.TOPIC)
            expect(msg.getTopic()).to.equal('position:1:1')

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
          const p = [ 20, 20, 20, 20, 20, 20, 20 ]
          const profile = {
            displayName: 'testname',
            publicKey: 'pubkey',
            avatarType: 'fox'
          }
          worldConn.sendProfileMessage(p, profile)

          expect(worldConn.reliableDataChannel.send).to.have.been.calledWithMatch((bytes) => {
            const msg = TopicMessage.deserializeBinary(bytes)
            expect(msg.getType()).to.equal(MessageType.TOPIC)
            expect(msg.getTopic()).to.equal('profile:1:1')

            const data = ProfileData.deserializeBinary(msg.getBody() as Uint8Array)
            expect(data.getAvatarType()).to.equal('fox')
            expect(data.getDisplayName()).to.equal('testname')
            expect(data.getPublicKey()).to.equal('pubkey')
            return true
          })
        })

        it('chat', () => {
          const p = [ 20, 20, 20, 20, 20, 20, 20 ]
          const messageId = 'chat1'
          const text = 'hello'
          worldConn.sendChatMessage(p, messageId, text)

          expect(worldConn.reliableDataChannel.send).to.have.been.calledWithMatch((bytes) => {
            const msg = TopicMessage.deserializeBinary(bytes)
            expect(msg.getType()).to.equal(MessageType.TOPIC)
            expect(msg.getTopic()).to.equal('chat:1:1')

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
        const worldConn = new WorldInstanceConnection('coordinator')
        const chatData = new ChatData()
        chatData.setText('text')
        chatData.setMessageId('chat1')
        const bytes = chatData.serializeBinary()
        processChatMessage(context, worldConn, 'client2', bytes)

        expect(context.peerData).to.have.key('client2')
        expect(context.peerData.get('client2').receivedPublicChatMessages).to.have.key('chat1')
    })

    describe('position handler', () => {
      it('new position', () => {
        const context = new Context({})
        const worldConn = new WorldInstanceConnection('coordinator')
        const positionData = new PositionData()

        positionData.setTime(Date.now())
        positionData.setPositionX(20)
        positionData.setPositionY(20)
        positionData.setPositionZ(20)
        positionData.setRotationX(20)
        positionData.setRotationY(20)
        positionData.setRotationZ(20)
        positionData.setRotationW(20)

        const bytes = positionData.serializeBinary()
        processPositionMessage(context, worldConn, 'client2', bytes)

        expect(context.peerData).to.have.key('client2')
        const trackingInfo = context.peerData.get('client2')
        expect(trackingInfo.position).to.deep.equal([20, 20, 20, 20, 20, 20, 20])
      })
      it('old position', () => {
        const context = new Context({})
        const info = new PeerTrackingInfo()
        info.lastPositionUpdate = Date.now()
        info.position = [20, 20, 20, 20, 20, 20, 20]
        context.peerData.set('client2', info)
        const worldConn = new WorldInstanceConnection('coordinator')
        const positionData = new PositionData()

        positionData.setTime(new Date(2008).getTime())
        positionData.setPositionX(30)
        positionData.setPositionY(30)
        positionData.setPositionZ(30)
        positionData.setRotationX(30)
        positionData.setRotationY(30)
        positionData.setRotationZ(30)
        positionData.setRotationW(30)

        const bytes = positionData.serializeBinary()
        processPositionMessage(context, worldConn, 'client2', bytes)

        expect(context.peerData).to.have.key('client2')
        const trackingInfo = context.peerData.get('client2')
        expect(trackingInfo.position).to.deep.equal([20, 20, 20, 20, 20, 20, 20])
      })
    })

    describe('profile handler', () => {
      it('new profile message', () => {
        const context = new Context({})
        const worldConn = new WorldInstanceConnection('coordinator')

        const profileData = new ProfileData()
        profileData.setTime(Date.now())
        profileData.setDisplayName('testname')
        profileData.setPublicKey('pubkey')
        profileData.setAvatarType('fox')

        const bytes = profileData.serializeBinary()
        processProfileMessage(context, worldConn, 'client2', bytes)

        expect(context.peerData).to.have.key('client2')
        const trackingInfo = context.peerData.get('client2')
        expect(trackingInfo.profile).to.deep.equal({
          displayName: 'testname',
          publicKey: 'pubkey',
          avatarType: 'fox'
        })
      })

      it('old profile message', () => {
        const profile = {
          displayName: 'testname1',
          publicKey: 'pubkey1',
          avatarType: 'fox1'
        }

        const context = new Context({})
        const info = new PeerTrackingInfo()
        info.lastProfileUpdate = Date.now()
        info.profile = profile
        context.peerData.set('client2', info)
        const worldConn = new WorldInstanceConnection('coordinator')

        const profileData = new ProfileData()
        profileData.setTime(new Date(2008).getTime())
        profileData.setDisplayName('testname')
        profileData.setPublicKey('pubkey')
        profileData.setAvatarType('fox')

        const bytes = profileData.serializeBinary()
        processProfileMessage(context, worldConn, 'client2', bytes)

        expect(context.peerData).to.have.key('client2')
        const trackingInfo = context.peerData.get('client2')
        expect(trackingInfo.profile).to.equal(profile)
      })
    })
  })

  describe('onPositionUpdate', () => {
    it('parcel has changed', () => {
      const context = new Context({})
      context.commRadius = 1
      context.currentPosition = [ 20, 20, 20, 20, 20, 20, 20 ]
      context.positionTopics.add('position:50:50')
      context.positionTopics.add('position:0:0')
      const worldConn = new WorldInstanceConnection('coordinator')
      worldConn.reliableDataChannel = { send: sinon.stub() }
      worldConn.unreliableDataChannel = { send: sinon.stub() }
      worldConn.addTopic = sinon.stub()
      worldConn.removeTopic = sinon.stub()
      context.worldInstanceConnection = worldConn
      onPositionUpdate(context, [ 0, 0, 0, 0, 0, 0, 0 ])

      expect(context.positionTopics.size).to.equal(9)
      expect(context.profileTopics.size).to.equal(9)
      expect(context.chatTopics.size).to.equal(9)

      // NOTE; there are 27 topics, but we were already subscribed to position:0:0
      expect(worldConn.addTopic).to.have.callCount(26)

      expect(worldConn.removeTopic).to.have.been.calledWith('position:50:50')
    })

    it('parcel has no changed', () => {
      const context = new Context({})
      context.commRadius = 1
      context.currentPosition = [ 20, 20, 20, 20, 20, 20, 20 ]
      const worldConn = new WorldInstanceConnection('coordinator')
      worldConn.reliableDataChannel = { send: sinon.stub() }
      worldConn.unreliableDataChannel = { send: sinon.stub() }
      worldConn.addTopic = sinon.stub()
      worldConn.removeTopic = sinon.stub()
      context.worldInstanceConnection = worldConn
      onPositionUpdate(context, context.currentPosition)

      expect(context.positionTopics.size).to.equal(0)
      expect(context.profileTopics.size).to.equal(0)
      expect(context.chatTopics.size).to.equal(0)

      expect(worldConn.addTopic).to.have.not.been.called
      expect(worldConn.removeTopic).to.have.not.been.called
    })
  })
})
