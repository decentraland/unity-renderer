import { fork } from 'redux-saga/effects'
import { atlasSaga } from '../atlas/sagas'
import { Auth } from '../auth/Auth'
import { authSaga } from '../auth/sagas'
import { loadingSaga } from '../loading/sagas'
import { passportSaga } from '../passports/sagas'
import { rootProtocolSaga } from '../protocol/sagas'
import { rendererSaga } from '../renderer/sagas'
import { metricSaga } from './metricSaga'

export function createRootSaga(auth: Auth) {
  return function* rootSaga() {
    yield fork(atlasSaga)
    yield fork(authSaga(auth))
    yield fork(passportSaga)
    yield fork(rendererSaga)
    yield fork(rootProtocolSaga)
    yield fork(metricSaga)
    yield fork(loadingSaga)
  }
}
