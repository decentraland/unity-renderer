import { fork } from 'redux-saga/effects'
import { atlasSaga } from '../atlas/sagas'
import { loadingSaga } from '../loading/sagas'
import { profileSaga } from '../profiles/sagas'
import { rootProtocolSaga } from '../protocol/sagas'
import { rendererSaga } from '../renderer/sagas'
import { metricSaga } from './metricSaga'
import { daoSaga } from '../dao/sagas'
import { metaSaga } from '../meta/sagas'
import { chatSaga } from '../chat/sagas'

export function createRootSaga() {
  return function* rootSaga() {
    yield fork(metaSaga)
    yield fork(profileSaga)
    yield fork(chatSaga)
    yield fork(atlasSaga)
    yield fork(daoSaga)
    yield fork(rendererSaga)
    yield fork(rootProtocolSaga)
    yield fork(metricSaga)
    yield fork(loadingSaga)
  }
}
