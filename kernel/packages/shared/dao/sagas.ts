import { WEB3_INITIALIZED, catalystRealmInitialized, initCatalystRealm, setCatalystCandidates } from './actions'
import { call, put, takeEvery } from 'redux-saga/effects'
import { pickCatalystRealm, fecthCatalystRealms } from './index'
import { Realm, Candidate } from './types'

export function* daoSaga(): any {
  yield takeEvery(WEB3_INITIALIZED, loadCatalystRealms)
}

function* loadCatalystRealms() {
  const candidates: Candidate[] = yield call(fecthCatalystRealms)

  yield put(setCatalystCandidates(candidates))

  const realm: Realm = yield call(pickCatalystRealm, candidates)

  yield put(initCatalystRealm(realm))

  yield put(catalystRealmInitialized())
}
