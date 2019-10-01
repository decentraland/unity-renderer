import { expectSaga } from 'redux-saga-test-plan'
import { call, select } from 'redux-saga/effects'
import { profileServerRequest, handleFetchProfile } from 'shared/passports/sagas'
import { getProfileDownloadServer } from 'shared/passports/selectors'
import { getAccessToken } from 'shared/auth/selectors'
import { passportRequest, passportSuccess } from 'shared/passports/actions'

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
