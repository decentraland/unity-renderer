import { expect } from 'chai'
import { v4 } from 'uuid'
import { setLocalProfile, peerMap, removeById } from 'shared/comms/peers'

describe('comms', function() {
  const localId = v4()
  it('set local user and remove it', () => {
    setLocalProfile(localId, { status: 'test' })

    expect(peerMap.has(localId)).to.eq(true)

    const peer = peerMap.get(localId)

    expect(peer.user.status).to.eq('test')

    removeById(localId)

    expect(peerMap.has(localId)).to.eq(false)
  })
})
