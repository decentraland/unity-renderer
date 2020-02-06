import { WEB3_INITIALIZED, catalystRealmInitialized, setCatalystRealm } from './actions'
import { call, put, takeEvery } from 'redux-saga/effects'
import { pickCatalystRealm } from './index'
import { Realm } from './types'

export function* daoSaga(): any {
  yield takeEvery(WEB3_INITIALIZED, loadCatalystRealm)
}

function* loadCatalystRealm() {
  const realm: Realm = yield call(pickCatalystRealm)

  yield put(setCatalystRealm(realm))

  yield put(catalystRealmInitialized())
}
