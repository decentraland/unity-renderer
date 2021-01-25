import { expectSaga } from 'redux-saga-test-plan'
import { call, select } from 'redux-saga/effects'
import * as matchers from 'redux-saga-test-plan/matchers'
import { profileRequest, profileSuccess } from 'shared/profiles/actions'
import { handleFetchProfile, profileServerRequest, fetchInventoryItemsByAddress } from 'shared/profiles/sagas'
import { getCurrentUserId } from 'shared/session/selectors'
import { profileSaga } from '../../packages/shared/profiles/sagas'
import { processServerProfile } from '../../packages/shared/profiles/transformations/processServerProfile'
import { dynamic } from 'redux-saga-test-plan/providers'
import { expect } from 'chai'
import { inventorySuccess, PROFILE_SUCCESS } from '../../packages/shared/profiles/actions'
import { isRealmInitialized, getResizeService } from '../../packages/shared/dao/selectors'
import { getServerConfigurations } from 'config'
import { sleep } from 'atomicHelpers/sleep'

const profile = { data: 'profile' }

const delayed = (result: any) =>
  dynamic(async () => {
    await sleep(1)
    return result
  })

const delayedProfile = delayed({ avatars: [profile] })

describe('fetchProfile behavior', () => {
  it('completes once for more than one request of same user', () => {
    return expectSaga(profileSaga)
      .put(profileSuccess('user|1', 'passport' as any, true))
      .not.put(profileSuccess('user|1', 'passport' as any, true))
      .dispatch(profileRequest('user|1'))
      .dispatch(profileRequest('user|1'))
      .dispatch(profileRequest('user|1'))
      .provide([
        [select(isRealmInitialized), true],
        [call(profileServerRequest, 'user|1'), delayedProfile],
        [call(fetchInventoryItemsByAddress, 'user|1'), []],
        [select(getCurrentUserId), 'myid'],
        [call(processServerProfile, 'user|1', profile), 'passport']
      ])
      .run()
  })

  it('runs one request for each user', () => {
    return expectSaga(profileSaga)
      .put(profileSuccess('user|1', 'passport1' as any, true))
      .put(profileSuccess('user|2', 'passport2' as any, true))
      .not.put(profileSuccess('user|1', 'passport1' as any))
      .not.put(profileSuccess('user|2', 'passport2' as any))
      .dispatch(profileRequest('user|1'))
      .dispatch(profileRequest('user|1'))
      .dispatch(profileRequest('user|2'))
      .dispatch(profileRequest('user|2'))
      .provide([
        [select(isRealmInitialized), true],
        [call(profileServerRequest, 'user|1'), delayedProfile],
        [select(getCurrentUserId), 'myid'],
        [call(processServerProfile, 'user|1', profile), 'passport1'],
        [call(profileServerRequest, 'user|2'), delayedProfile],
        [call(fetchInventoryItemsByAddress, 'user|1'), []],
        [call(processServerProfile, 'user|2', profile), 'passport2'],
        [call(fetchInventoryItemsByAddress, 'user|2'), []]
      ])
      .run()
  })

  it('fetches inventory for corresponding user', () => {
    const profile1 = { ...profile }
    const profile2 = { ...profile }
    return expectSaga(profileSaga)
      .put(profileSuccess('user|1', 'passport1' as any, true))
      .put(profileSuccess('user|2', 'passport2' as any, true))
      .dispatch(profileRequest('user|1'))
      .dispatch(profileRequest('user|2'))
      .provide([
        [select(isRealmInitialized), true],
        [select(getCurrentUserId), 'myid'],
        [call(profileServerRequest, 'user|1'), delayed({ avatars: [profile1] })],
        [call(profileServerRequest, 'user|2'), delayed({ avatars: [profile2] })],
        [call(fetchInventoryItemsByAddress, 'user|1'), ['dcl://base-exclusive/wearable1/1']],
        [call(fetchInventoryItemsByAddress, 'user|2'), ['dcl://base-exclusive/wearable2/2']],
        [call(processServerProfile, 'user|1', profile1), 'passport1'],
        [call(processServerProfile, 'user|2', profile2), 'passport2']
      ])
      .run()
      .then((result) => {
        const inventory1 = (profile1 as any).inventory
        expect(inventory1).to.have.length(1)
        expect(inventory1[0]).to.equal('dcl://base-exclusive/wearable1')

        const inventory2 = (profile2 as any).inventory
        expect(inventory2).to.have.length(1)
        expect(inventory2[0]).to.equal('dcl://base-exclusive/wearable2')
      })
  })

  it('ignores inventory for another user', () => {
    const profile1 = { ...profile, ethAddress: 'eth1' }
    return expectSaga(handleFetchProfile, profileRequest('user|1'))
      .put(profileSuccess('user|1', 'passport1' as any, true))
      .dispatch(inventorySuccess('user|2', ['dcl://base-exclusive/wearable2/2']))
      .dispatch(inventorySuccess('user|1', ['dcl://base-exclusive/wearable1/1']))
      .provide([
        [select(getCurrentUserId), 'myid'],
        [call(profileServerRequest, 'user|1'), delayed({ avatars: [profile1] })],
        [call(processServerProfile, 'user|1', profile1), 'passport1']
      ])
      .run()
      .then((result) => {
        const inventory1 = (profile1 as any).inventory
        expect(inventory1).to.have.length(1)
        expect(inventory1[0]).to.equal('dcl://base-exclusive/wearable1')
      })
  })

  it('generates scaled face snapshots', () => {
    const profileWithNoSnapshots = { avatar: { snapshots: { face: 'http://fake.url/contents/facehash' } } }
    const profile1 = { ...profileWithNoSnapshots, ethAddress: 'eth1' }
    return expectSaga(handleFetchProfile, profileRequest('user|1'))
      .dispatch(inventorySuccess('user|1', []))
      .provide([
        [select(getCurrentUserId), 'myid'],
        [select(getResizeService), 'http://fake/resizeurl'],
        [matchers.call.fn(fetch), dynamic(() => ({ ok: true }))],
        [call(profileServerRequest, 'user|1'), delayed({ avatars: [profile1] })],
        [call(processServerProfile, 'user|1', profile1), dynamic((effect) => effect.args[1])]
      ])
      .run()
      .then((result) => {
        const putEffects = result.effects.put
        const lastPut = putEffects[putEffects.length - 1].payload.action
        expect(lastPut.type).to.eq(PROFILE_SUCCESS)

        const { face, face128, face256 } = lastPut.payload.profile.avatar.snapshots
        expect(face).to.eq('http://fake.url/contents/facehash')
        expect(face128).to.eq('http://fake/resizeurl/facehash/128')
        expect(face256).to.eq('http://fake/resizeurl/facehash/256')
      })
  })

  it('detects and fixes corrupted scaled snapshots', () => {
    const profileWithCorruptedSnapshots = {
      avatar: { snapshots: { face: 'http://fake.url/contents/facehash', face128: '128', face256: '256' } }
    }
    const profile1 = { ...profileWithCorruptedSnapshots, ethAddress: 'eth1' }
    return expectSaga(handleFetchProfile, profileRequest('user|1'))
      .dispatch(inventorySuccess('user|1', []))
      .provide([
        [select(getCurrentUserId), 'myid'],
        [select(getResizeService), 'http://fake/resizeurl'],
        [matchers.call.fn(fetch), dynamic(() => ({ ok: true }))],
        [call(profileServerRequest, 'user|1'), delayed({ avatars: [profile1] })],
        [call(processServerProfile, 'user|1', profile1), dynamic((effect) => effect.args[1])]
      ])
      .run()
      .then((result) => {
        const putEffects = result.effects.put
        const lastPut = putEffects[putEffects.length - 1].payload.action
        expect(lastPut.type).to.eq(PROFILE_SUCCESS)

        const { face, face128, face256 } = lastPut.payload.profile.avatar.snapshots
        expect(face).to.eq('http://fake.url/contents/facehash')
        expect(face128).to.eq('http://fake/resizeurl/facehash/128')
        expect(face256).to.eq('http://fake/resizeurl/facehash/256')
      })
  })

  it('falls back when resize not working in current server', () => {
    const profileWithCorruptedSnapshots = {
      avatar: { snapshots: { face: 'http://fake.url/contents/facehash' } }
    }
    const profile1 = { ...profileWithCorruptedSnapshots, ethAddress: 'eth1' }
    return expectSaga(handleFetchProfile, profileRequest('user|1'))
      .dispatch(inventorySuccess('user|1', []))
      .provide([
        [select(getCurrentUserId), 'myid'],
        [select(getResizeService), 'http://fake/resizeurl'],
        [matchers.call.fn(fetch), dynamic((call) => ({ ok: !call.args[0].startsWith('http://fake/resizeurl') }))],
        [call(profileServerRequest, 'user|1'), delayed({ avatars: [profile1] })],
        [call(processServerProfile, 'user|1', profile1), dynamic((effect) => effect.args[1])]
      ])
      .run()
      .then((result) => {
        const putEffects = result.effects.put
        const lastPut = putEffects[putEffects.length - 1].payload.action
        expect(lastPut.type).to.eq(PROFILE_SUCCESS)

        const { face, face128, face256 } = lastPut.payload.profile.avatar.snapshots
        expect(face).to.eq('http://fake.url/contents/facehash')
        expect(face128).to.eq(`${getServerConfigurations().fallbackResizeServiceUrl}/facehash/128`)
        expect(face256).to.eq(`${getServerConfigurations().fallbackResizeServiceUrl}/facehash/256`)
      })
  })
})
