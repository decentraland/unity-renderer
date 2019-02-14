import { expect } from 'chai'
import { commConfigurations as config, commConfigurations, parcelLimits } from 'config'
import {
  Context,
  Position,
  PeerTrackingInfo,
  ServerConnection,
  processConnections,
  onPositionUpdate,
  makeCurrentPosition,
  collectInfo,
  SocketReadyState,
  onCommDataLoaded
} from 'communications/src/client'
import { V2, CommunicationArea } from 'communications/src/utils'
import { Adapter } from 'communications/src/adapter'
import { ScriptingTransport } from 'decentraland-rpc/lib/common/json-rpc/types'
import * as sinon from 'sinon'

const { parcelSize } = parcelLimits

function createContext(): [Context, Adapter] {
  const peerId = 'peer-1'
  const transport = {} as ScriptingTransport
  transport.onMessage = sinon.stub()
  const connector = new Adapter(transport)
  sinon.stub(connector, 'notify')

  const userProfile = {
    displayName: 'testPeer',
    publicKey: 'key',
    avatarType: 'fox'
  }

  const context = new Context(connector, peerId, userProfile)
  return [context, connector]
}

let registeredWebSockets = []

class MockWebSocket {
  public readyState: SocketReadyState = SocketReadyState.CLOSED

  constructor(public url: string) {
    registeredWebSockets.push(this)
  }
}

describe('Communications', function() {
  const ORIGINAL_WEB_SOCKET = window['WebSocket']

  before(() => {
    // NOTE: little hack to be able to mock websocket requests
    window['WebSocket'] = MockWebSocket
  })

  beforeEach(() => {
    registeredWebSockets = []
  })

  after(() => {
    window['WebSocket'] = ORIGINAL_WEB_SOCKET
  })

  describe('CommunicationArea', () => {
    it('should contain the center', () => {
      const area = new CommunicationArea(new V2(0, 0), 2)
      expect(area.contains(new V2(0, 0))).to.be.true
    })

    it('should contain the vertexes', () => {
      const area = new CommunicationArea(new V2(0, 0), 2)
      expect(area.contains(new V2(2, 2))).to.be.true
      expect(area.contains(new V2(-2, 0))).to.be.true
      expect(area.contains(new V2(-2, -2))).to.be.true
      expect(area.contains(new V2(2, -2))).to.be.true
    })

    it('should contain a point outside the area', () => {
      const area = new CommunicationArea(new V2(0, 0), 2)
      expect(area.contains(new V2(1.3, 0))).to.be.true
    })

    it('should not contain a point outside the area', () => {
      const area = new CommunicationArea(new V2(0, 0), 2)
      expect(area.contains(new V2(3, 0))).to.be.false
    })
  })

  describe('ServerConnection', () => {
    describe('containsLocation()', () => {
      it('should return true if contains the location', () => {
        const conn = new ServerConnection(0, 'test-comm-server', new V2(10, 1))
        conn.locations.push(new V2(3, 3))
        expect(conn.containsLocation(new V2(3, 3))).to.be.true
      })

      it("should return false if doesn't contains the location", () => {
        const conn = new ServerConnection(0, 'test-comm-server', new V2(10, 1))
        expect(conn.containsLocation(new V2(3, 3))).to.be.false
      })
    })

    describe('containsAnyLocations()', () => {
      it('should return true if contains at least one of the locations', () => {
        const conn = new ServerConnection(0, 'test-comm-server', new V2(10, 1))
        conn.locations.push(new V2(3, 3))
        expect(conn.containsAnyLocations([new V2(10, 1), new V2(8, 3)])).to.be.true
      })

      it("should return false if doesn't contains any of the location", () => {
        const conn = new ServerConnection(0, 'test-comm-server', new V2(10, 1))
        expect(conn.containsAnyLocations([new V2(12, 1), new V2(3, 3)])).to.be.false
      })
    })

    describe('removeLocation()', () => {
      it('should return remove the location if present', () => {
        const conn = new ServerConnection(0, 'test-comm-server', new V2(10, 1))
        conn.locations.push(new V2(3, 3))
        conn.locations.push(new V2(1, 3))
        conn.locations.push(new V2(10, 10))

        conn.removeLocation(new V2(3, 3))

        expect(conn.locations).to.have.lengthOf(3)
        expect(conn.containsLocation(new V2(3, 3))).to.be.false
        expect(conn.containsLocation(new V2(10, 10))).to.be.true
        expect(conn.containsLocation(new V2(10, 1))).to.be.true
        expect(conn.containsLocation(new V2(1, 3))).to.be.true
      })
    })

    it('addLocation()', () => {
      const conn = new ServerConnection(0, 'test-comm-server', new V2(10, 1))
      conn.addLocation(new V2(0, 0))

      expect(conn.locations).to.deep.equal([new V2(10, 1), new V2(0, 0)])
    })

    describe('minSquareDistance()', () => {
      it('should return the minium square distance to the point', () => {
        const conn = new ServerConnection(0, 'test-comm-server', new V2(10, 1))
        conn.addLocation(new V2(0, 0))

        expect(conn.minSquareDistance(new V2(0, 0))).to.equal(0)
        expect(conn.minSquareDistance(new V2(10, 1))).to.equal(0)
        expect(conn.minSquareDistance(new V2(10, 0))).to.equal(1)
      })
    })
  })

  describe('onPositionUpdate()', () => {
    it('should update current position and connections', () => {
      const [context] = createContext()
      const position = [0, 0, 16.3, 0, 0, 0, 0] as Position
      onPositionUpdate(context, position)
      expect(context.currentPosition.position).to.deep.equal(position)
      expect(context.currentPosition.parcel).to.deep.equal({ x: 0, z: 1 })

      expect(context.connections.length).to.be.gt(0)

      for (let connection of context.connections) {
        expect(connection.ws).to.be.null
        for (let location of connection.locations) {
          expect(context.currentPosition.commArea.contains(location)).to.be.true
        }
      }
    })
  })

  describe('processConnections()', () => {
    it(`should connect to a max of ${config.maxConcurrentConnectionRequests}`, () => {
      const [context] = createContext()
      context.currentPosition = makeCurrentPosition([0, 0, 0, 0, 0, 0, 0])

      const connections = []
      for (let i = 0; i < config.maxConcurrentConnectionRequests + 10; ++i) {
        connections.push(new ServerConnection(i, 'ws://comm-server-${i}', new V2(parcelSize * i, 0)))
      }
      context.connections = connections
      processConnections(context)
      expect(registeredWebSockets).to.have.lengthOf(config.maxConcurrentConnectionRequests)
    })

    it('should connect to near servers first', () => {
      const [context] = createContext()
      context.currentPosition = makeCurrentPosition([0, 0, 0, 0, 0, 0, 0])
      context.connections = [
        new ServerConnection(0, 'ws://far-server-test-comm-server', new V2(10, 10)),
        new ServerConnection(1, 'ws://close-server-test-comm-server', new V2(10, 0)),
        new ServerConnection(2, 'ws://parcel-server-test-comm-server', new V2(0, 0))
      ]
      processConnections(context)
      const urls = registeredWebSockets.map(ws => ws.url)
      expect(urls).to.be.deep.equal([
        'ws://parcel-server-test-comm-server',
        'ws://close-server-test-comm-server',
        'ws://far-server-test-comm-server'
      ])
    })

    it('should clean up negotiating connections', () => {
      const [context] = createContext()
      context.currentPosition = makeCurrentPosition([0, 0, 0, 0, 0, 0, 0])
      const ws = new MockWebSocket('')
      ws.readyState = SocketReadyState.OPEN
      context.negotiatingConnection.add(ws as WebSocket)
      processConnections(context)
      expect(context.negotiatingConnection.size).to.be.equal(0)
    })

    it('should disconnect servers tracking no location', () => {
      const [context] = createContext()
      context.currentPosition = makeCurrentPosition([0, 0, 0, 0, 0, 0, 0])
      const conn = new ServerConnection(0, 'ws://far-server-test-comm-server', new V2(0, 0))
      conn.removeLocation(new V2(0, 0))
      context.connections = [conn]
      processConnections(context)
      expect(context.connections).to.have.lengthOf(0)
    })
  })

  describe('collectInfo()', () => {
    it('should return no peer info if nothing is tracked', () => {
      const [context] = createContext()
      context.currentPosition = makeCurrentPosition([0, 0, 0, 0, 0, 0, 0])
      const info = collectInfo(context)
      expect(info.peers).to.have.lengthOf(0)
    })

    it('should return all peers', () => {
      const [context] = createContext()
      context.currentPosition = makeCurrentPosition([0, 0, 0, 0, 0, 0, 0])

      let peerInfo = new PeerTrackingInfo()
      peerInfo.servers.set(0, Date.now())
      peerInfo.position = [0, 0, 0, 0, 0, 0, 0]
      peerInfo.profile = { displayName: 'test', publicKey: 'test', avatarType: 'fox' }
      context.peerData.set('peer-1', peerInfo)

      peerInfo = new PeerTrackingInfo()
      peerInfo.servers.set(0, Date.now())
      peerInfo.profile = { displayName: 'test', publicKey: 'test', avatarType: 'fox' }
      context.peerData.set('peer-2', peerInfo)

      peerInfo = new PeerTrackingInfo()
      peerInfo.servers.set(0, Date.now())
      peerInfo.position = [0, 0, 0, 0, 0, 0, 0]
      context.peerData.set('peer-3', peerInfo)

      // NOTE: outside comm area
      peerInfo = new PeerTrackingInfo()
      peerInfo.servers.set(0, Date.now())
      peerInfo.position = [2000, 0, 0, 0, 0, 0, 0]
      peerInfo.profile = { displayName: 'test', publicKey: 'test', avatarType: 'fox' }
      context.peerData.set('peer-4', peerInfo)

      peerInfo = new PeerTrackingInfo()
      peerInfo.servers.set(0, Date.now())
      peerInfo.position = [10, 10, 0, 0, 0, 0, 0]
      peerInfo.profile = { displayName: 'test', publicKey: 'test', avatarType: 'fox' }
      context.peerData.set('peer-5', peerInfo)

      peerInfo = new PeerTrackingInfo()
      peerInfo.position = [0, 0, 0, 0, 0, 0, 0]
      peerInfo.profile = { displayName: 'test', publicKey: 'test', avatarType: 'fox' }
      context.peerData.set('peer-6', peerInfo)

      const info = collectInfo(context)
      expect(info.peers).to.have.lengthOf(6)

      for (let peer of info.peers) {
        const { peerId } = peer
        if (peerId === 'peer-1' || peerId === 'peer-5') {
          expect(peer.position).to.not.be.null
          expect(peer.profile).to.not.be.null
        } else {
          expect(peer.position).to.be.null
          expect(peer.profile).to.be.null
        }
      }
    })

    it('should return all peers but limit visibility', () => {
      const [context] = createContext()
      context.currentPosition = makeCurrentPosition([0, 0, 0, 0, 0, 0, 0])

      for (let i = 0; i < 100; ++i) {
        let peerInfo = new PeerTrackingInfo()
        peerInfo.servers.set(0, Date.now())
        peerInfo.position = [0, 0, 0, 0, 0, 0, 0]
        peerInfo.profile = { displayName: 'test', publicKey: 'test', avatarType: 'fox' }
        context.peerData.set(`peer-${i}`, peerInfo)
      }

      const info = collectInfo(context)
      expect(info.peers).to.have.lengthOf(100)

      let visiblePeerCount = 0
      for (let peer of info.peers) {
        const { position, profile } = peer
        if (position || profile) {
          visiblePeerCount++
        }
      }

      expect(visiblePeerCount).to.equal(commConfigurations.maxVisiblePeers)
    })
  })

  describe('onCommDataLoaded()', () => {
    it('should do nothing in the info matches the current one', () => {
      const serverUrl = 'test-server-url'
      const conn = new ServerConnection(0, serverUrl, new V2(0, 0))

      const [context] = createContext()
      context.connections = [conn]
      context.commData.set('0,0', { commServerUrl: serverUrl })

      onCommDataLoaded(context, [{ commServerUrl: serverUrl, x: 0, y: 0 }])

      expect(context.connections).to.have.lengthOf(1)
      expect(context.connections[0]).to.deep.equal(conn)
    })

    it('should change connection if there is a new url for the parcel', () => {
      const oldServerUrl = 'test-server-url'
      const conn = new ServerConnection(0, oldServerUrl, new V2(0, 0))

      const [context] = createContext()
      context.connections = [conn]
      context.commData.set('0,0', { commServerUrl: oldServerUrl })

      const newServerUrl = 'new-test-server-url'
      onCommDataLoaded(context, [{ commServerUrl: newServerUrl, x: 0, y: 0 }])

      expect(context.connections).to.have.lengthOf(2)
      expect(context.connections[0].locations).to.have.lengthOf(0)
      expect(context.connections[1].url).to.equal(newServerUrl)
      expect(context.connections[1].locations).to.deep.equal([new V2(0, 0)])
    })
  })
})
