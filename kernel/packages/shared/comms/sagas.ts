import { put, takeEvery, select, call, takeLatest } from 'redux-saga/effects'

import { STATIC_WORLD } from 'config'

import { establishingComms } from 'shared/loading/types'
import { USER_AUTHENTIFIED } from 'shared/session/actions'
import { getCurrentIdentity } from 'shared/session/selectors'
import { setWorldContext } from 'shared/protocol/actions'
import { ensureRealmInitialized, selectRealm } from 'shared/dao/sagas'
import { getRealm } from 'shared/dao/selectors'
import { CATALYST_REALMS_SCAN_SUCCESS, setCatalystRealm } from 'shared/dao/actions'
import { Realm } from 'shared/dao/types'
import { realmToString } from 'shared/dao/utils/realmToString'
import { createLogger } from 'shared/logger'

import { connect } from '.'

const DEBUG = false
const logger = createLogger('comms: ')

export function* commsSaga() {
  yield takeEvery(USER_AUTHENTIFIED, establishCommunications)
  yield takeLatest(CATALYST_REALMS_SCAN_SUCCESS, changeRealm)
}

function* establishCommunications() {
  if (STATIC_WORLD) {
    return
  }

  yield call(ensureRealmInitialized)

  const identity = yield select(getCurrentIdentity)

  yield put(establishingComms())
  const context = yield connect(identity.address)
  if (context !== undefined) {
    yield put(setWorldContext(context))
  }
}

function* changeRealm() {
  const currentRealm: ReturnType<typeof getRealm> = yield select(getRealm)
  if (!currentRealm) {
    DEBUG && logger.info(`No realm set, wait for actual DAO initialization`)
    // if not realm is set => wait for actual dao initialization
    return
  }

  const otherRealm = yield call(selectRealm)

  if (!sameRealm(currentRealm, otherRealm)) {
    logger.info(`Changing realm from ${realmToString(currentRealm)} to ${realmToString(otherRealm)}`)
    yield put(setCatalystRealm(otherRealm))
  } else {
    DEBUG && logger.info(`Realm already set ${realmToString(currentRealm)}`)
  }
}

function sameRealm(realm1: Realm, realm2: Realm) {
  return realm1.catalystName === realm2.catalystName && realm1.layer === realm2.layer
}
