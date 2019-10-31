import { expectSaga } from 'redux-saga-test-plan'
import { call, select } from 'redux-saga/effects'
import { getAccessToken } from 'shared/auth/selectors'
import { notifyNewInventoryItem, passportRequest, passportSuccess } from 'shared/passports/actions'
import {
  compareInventoriesAndTriggerNotification,
  handleFetchProfile,
  profileServerRequest
} from 'shared/passports/sagas'
import { getProfile, getProfileDownloadServer } from 'shared/passports/selectors'

describe('fetchProfile behavior', () => {
  it('behaves normally', () => {
    expectSaga(handleFetchProfile, passportRequest('userId'))
      .provide([
        [select(getProfileDownloadServer), 'server'],
        [select(getAccessToken), 'access-token'],
        [call(profileServerRequest, 'server', 'userId', 'access-token'), { data: 'profile' }]
      ])
      .put(passportSuccess('userId', 'profile' as any))
      .run()
  })
})

describe('notifications behavior', () => {
  const getReturnsNull = (_: any) => undefined
  const getReturnsYes = (_: any) => 'notified'
  const noopSave = (_: any, __: any) => undefined
  const profile = {}
  const userId = 'userId'
  it('triggers on new item', () => {
    expectSaga(compareInventoriesAndTriggerNotification, userId, [], ['newItem'], getReturnsNull, noopSave)
      .provide([[select(getProfile, userId), profile]])
      .put(notifyNewInventoryItem())
      .run()
  })
  it('does not trigger if already sent', () => {
    expectSaga(compareInventoriesAndTriggerNotification, userId, [], ['newItem'], getReturnsYes, noopSave)
      .provide([[select(getProfile, userId), profile]])
      .not.put(notifyNewInventoryItem())
      .run()
  })
  it('does not trigger multiple notifications if more than one item', () => {
    expectSaga(compareInventoriesAndTriggerNotification, userId, [], ['newItem', 'newItem2'], getReturnsYes, noopSave)
      .provide([[select(getProfile, userId), profile]])
      .put(notifyNewInventoryItem())
      .not.put(notifyNewInventoryItem())
      .run()
  })
})
