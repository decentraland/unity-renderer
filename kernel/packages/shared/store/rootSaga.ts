import { fork } from 'redux-saga/effects'
import { atlasSaga } from '../atlas/sagas'
import { loadingSaga } from '../loading/sagas'
import { passportSaga } from '../passports/sagas'
import { rootProtocolSaga } from '../protocol/sagas'
import { rendererSaga } from '../renderer/sagas'
import { metricSaga } from './metricSaga'

export function createRootSaga() {
  return function* rootSaga() {
    yield fork(atlasSaga)
    yield fork(passportSaga)
    yield fork(rendererSaga)
    yield fork(rootProtocolSaga)
    yield fork(metricSaga)
    yield fork(loadingSaga)
  }
}
