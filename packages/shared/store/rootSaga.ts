import { fork } from 'redux-saga/effects'
import { passportSaga } from '../passports/sagas'
import { authSaga } from '../auth/sagas'
import { Auth } from '../auth/Auth'
import { rendererSaga } from '../renderer/sagas'
import { rootProtocolSaga } from '../protocol/sagas'
import { metricSaga } from './metricSaga'

export function createRootSaga(auth: Auth) {
  return function* rootSaga() {
    yield fork(authSaga(auth))
    yield fork(passportSaga)
    yield fork(rendererSaga)
    yield fork(rootProtocolSaga)
    yield fork(metricSaga)
  }
}
