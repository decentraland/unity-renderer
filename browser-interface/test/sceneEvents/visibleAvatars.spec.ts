import * as sinon from 'sinon'
import { expect } from 'chai'
import { receiveUserVisible } from 'shared/comms/peers'
import * as peers from 'shared/comms/peers'
import { TEST_OBJECT_ObservableAllScenesEvent } from 'shared/world/parcelSceneManager'
import { buildStore } from 'shared/store/store'
import { Color3 } from '@dcl/ecs-math'

function prepareAvatar(address: string) {
  peers.receivePeerUserData(
    {
      ethAddress: address,
      hasClaimedName: false,
      name: address,
      version: 1,
      description: address,
      tutorialStep: 0,
      userId: address,
      avatar: {
        bodyShape: 'body',
        wearables: [],
        emotes: [],
        eyes: { color: Color3.Green() },
        hair: { color: Color3.Green() },
        skin: { color: Color3.Green() },
        snapshots: {
          body: 'body',
          face256: 'face256'
        }
      }
    },
    location.origin
  )
  receiveUserVisible(address, false)
}

describe('Avatar observable', () => {
  const userA = '0xa00000000000000000000000000000000000000a'
  const userB = '0xb00000000000000000000000000000000000000b'
  const userC = '0xc00000000000000000000000000000000000000c'

  let lastEvent: any = null

  before(() => {
    TEST_OBJECT_ObservableAllScenesEvent.add((x) => {
      lastEvent = x
    })
  })

  beforeEach('start store', () => {
    const { store } = buildStore()
    globalThis.globalStore = store

    prepareAvatar(userA)
    prepareAvatar(userB)
    prepareAvatar(userC)

    lastEvent = null
  })

  function getVisibleAvatarsUserId() {
    return Array.from(peers.getAllPeers().values())
      .filter(($) => $.visible)
      .map(($) => $.ethereumAddress)
  }

  afterEach(() => {
    // clear visible avatars cache
    peers.removeAllPeers()

    sinon.restore()
    sinon.reset()
  })

  it('should return user A and B that are visible at the scene', () => {
    peers.receiveUserVisible(userA, true)

    expect(lastEvent).to.deep.eq({ eventType: 'playerConnected', payload: { userId: userA } })

    peers.receiveUserVisible(userB, true)
    expect(lastEvent).to.deep.eq({ eventType: 'playerConnected', payload: { userId: userB } })
    lastEvent = null
    peers.receiveUserVisible(userC, false)
    expect(lastEvent).to.eq(null)
    expect(getVisibleAvatarsUserId()).to.eql([userA, userB])
  })

  it('if should remove user when he leaves the scene', () => {
    peers.receiveUserVisible(userA, true)
    expect(lastEvent).to.deep.eq({ eventType: 'playerConnected', payload: { userId: userA } })

    peers.receiveUserVisible(userB, true)
    expect(lastEvent).to.deep.eq({ eventType: 'playerConnected', payload: { userId: userB } })
    expect(getVisibleAvatarsUserId()).to.eql([userA, userB])

    peers.receiveUserVisible(userA, false)
    expect(lastEvent).to.deep.eq({ eventType: 'playerDisconnected', payload: { userId: userA } })
    expect(getVisibleAvatarsUserId()).to.eql([userB])
  })

  it('should remove the user from the cache if we receieve an USER_REMOVED action', () => {
    peers.receiveUserVisible(userA, true)
    peers.receiveUserVisible(userB, true)
    expect(getVisibleAvatarsUserId()).to.eql([userA, userB])
    peers.removePeerByAddress(userA)
    expect(lastEvent).to.deep.eq({ eventType: 'playerDisconnected', payload: { userId: userA } })
    expect(getVisibleAvatarsUserId()).to.eql([userB])
  })
})
