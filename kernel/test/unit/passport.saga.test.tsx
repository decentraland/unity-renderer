import { expectSaga } from 'redux-saga-test-plan'
import { call, select } from 'redux-saga/effects'
import { notifyNewInventoryItem, profileRequest, profileSuccess } from 'shared/profiles/actions'
import {
  compareInventoriesAndTriggerNotification,
  handleFetchProfile,
  profileServerRequest,
  fetchInventoryItemsByAddress,
  getCurrentUserId
} from 'shared/profiles/sagas'
import { getProfile, getProfileDownloadServer } from 'shared/profiles/selectors'
import { profileSaga, delay } from '../../packages/shared/profiles/sagas'
import { processServerProfile } from '../../packages/shared/profiles/transformations/processServerProfile'
import { dynamic } from 'redux-saga-test-plan/providers'
import { expect } from 'chai'
import { inventorySuccess } from '../../packages/shared/profiles/actions'
import { isRealmInitialized } from '../../packages/shared/dao/selectors'

const profile = { data: 'profile' }

const delayed = (result: any) =>
  dynamic(async () => {
    await delay(1)
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
        [select(getProfileDownloadServer), 'server'],
        [call(profileServerRequest, 'server', 'user|1'), delayedProfile],
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
        [select(getProfileDownloadServer), 'server'],
        [call(profileServerRequest, 'server', 'user|1'), delayedProfile],
        [select(getCurrentUserId), 'myid'],
        [call(processServerProfile, 'user|1', profile), 'passport1'],
        [call(profileServerRequest, 'server', 'user|2'), delayedProfile],
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
        [select(getProfileDownloadServer), 'server'],
        [call(profileServerRequest, 'server', 'user|1'), delayed({ avatars: [profile1] })],
        [call(profileServerRequest, 'server', 'user|2'), delayed({ avatars: [profile2] })],
        [call(fetchInventoryItemsByAddress, 'user|1'), ['dcl://base-exclusive/wearable1/1']],
        [call(fetchInventoryItemsByAddress, 'user|2'), ['dcl://base-exclusive/wearable2/2']],
        [call(processServerProfile, 'user|1', profile1), 'passport1'],
        [call(processServerProfile, 'user|2', profile2), 'passport2']
      ])
      .run()
      .then(result => {
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
        [select(getProfileDownloadServer), 'server'],
        [call(profileServerRequest, 'server', 'user|1'), delayed({ avatars: [profile1] })],
        [call(processServerProfile, 'user|1', profile1), 'passport1']
      ])
      .run()
      .then(result => {
        const inventory1 = (profile1 as any).inventory
        expect(inventory1).to.have.length(1)
        expect(inventory1[0]).to.equal('dcl://base-exclusive/wearable1')
      })
  })
})

describe('notifications behavior', () => {
  const getReturnsNull = (_: any) => undefined
  const getReturnsYes = (_: any) => 'notified'
  const noopSave = (_: any, __: any) => undefined
  const profile = {}
  const userId = 'userId'
  // TODO - fix tests - moliva - 2019-11-02
  xit('triggers on new item', () => {
    return expectSaga(compareInventoriesAndTriggerNotification, userId, [], ['newItem'], getReturnsNull, noopSave)
      .provide([[select(getProfile, userId), profile]])
      .put(notifyNewInventoryItem())
      .run()
  })
  xit('does not trigger if already sent', () => {
    return expectSaga(compareInventoriesAndTriggerNotification, userId, [], ['newItem'], getReturnsYes, noopSave)
      .provide([[select(getProfile, userId), profile]])
      .not.put(notifyNewInventoryItem())
      .run()
  })
  xit('does not trigger multiple notifications if more than one item', () => {
    return expectSaga(
      compareInventoriesAndTriggerNotification,
      userId,
      [],
      ['newItem', 'newItem2'],
      getReturnsYes,
      noopSave
    )
      .provide([[select(getProfile, userId), profile]])
      .put(notifyNewInventoryItem())
      .not.put(notifyNewInventoryItem())
      .run()
  })
})
