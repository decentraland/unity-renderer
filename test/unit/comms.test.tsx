import { expect } from 'chai'
import { setLocalProfile, peerMap, removeById } from 'dcl/comms/peers'

import { v4 } from 'uuid'

describe('comms', function() {
  const localId = v4()
  it('set local user and remove it', () => {
    setLocalProfile(localId, { status: 'test' })

    expect(peerMap.has(localId)).to.eq(true)

    const peer = peerMap.get(localId)

    expect(peer.user.status).to.eq('test')

    expect(peer.avatar).to.eq(undefined)

    removeById(localId)

    expect(peerMap.has(localId)).to.eq(false)
  })
})
