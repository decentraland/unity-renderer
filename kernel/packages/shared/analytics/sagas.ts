import { initializeAnalytics } from 'shared/analytics'
import { call } from 'redux-saga/effects'
import { WORLD_EXPLORER } from 'config'

export function* analyticsSaga() {
  if (WORLD_EXPLORER) {
    yield call(initializeAnalytics)
  }
}
