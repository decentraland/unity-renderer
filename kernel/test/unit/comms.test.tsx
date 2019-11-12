import { expect } from 'chai'
import { setLocalProfile, peerMap, removeById } from 'shared/comms/peers'
import { uuid } from 'atomicHelpers/math'

describe('comms', function() {
  const localId = uuid()
  it('set local user and remove it', () => {
    setLocalProfile(localId, { status: 'test' })

    expect(peerMap.has(localId)).to.eq(true)

    const peer = peerMap.get(localId)

    expect(peer!.user!.status).to.eq('test')

    removeById(localId)

    expect(peerMap.has(localId)).to.eq(false)
  })
})
