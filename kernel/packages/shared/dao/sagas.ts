import {
  WEB3_INITIALIZED,
  catalystRealmInitialized,
  initCatalystRealm,
  setCatalystCandidates,
  setAddedCatalystCandidates,
  setContentWhitelist,
  INIT_CATALYST_REALM,
  SET_CATALYST_REALM,
  InitCatalystRealm,
  SetCatalystRealm
} from './actions'
import { call, put, takeEvery, select, fork } from 'redux-saga/effects'
import { WORLD_EXPLORER, REALM } from 'config'
import { waitForMetaConfigurationInitialization } from '../meta/sagas'
import { Candidate, Realm, ServerConnectionStatus } from './types'
import { fecthCatalystRealms, fetchCatalystStatuses, pickCatalystRealm, getRealmFromString } from '.'
import { getAddedServers, getContentWhitelist } from 'shared/meta/selectors'
import { getAllCatalystCandidates } from './selectors'
import { saveToLocalStorage, getFromLocalStorage } from '../../atomicHelpers/localStorage'
import { ping } from './index'

const CACHE_KEY = 'realm'

export function* daoSaga(): any {
  yield takeEvery(WEB3_INITIALIZED, loadCatalystRealms)

  yield takeEvery([INIT_CATALYST_REALM, SET_CATALYST_REALM], cacheCatalystRealm)
}

function* loadCatalystRealms() {
  yield call(waitForMetaConfigurationInitialization)

  if (WORLD_EXPLORER) {
    const loadedRealm = getFromLocalStorage(CACHE_KEY)

    let realm: Realm
    if (loadedRealm && (yield checkValidRealm(loadedRealm))) {
      yield fork(initializeCatalystCandidates)

      realm = loadedRealm
    } else {
      yield call(initializeCatalystCandidates)

      const allCandidates: Candidate[] = yield select(getAllCatalystCandidates)

      const whitelist: string[] = yield select(getContentWhitelist)
      let whitelistedCandidates = allCandidates.filter(candidate => whitelist.includes(candidate.domain))
      if (whitelistedCandidates.length === 0) {
        // if intersection is empty (no whitelisted or not in our candidate set) => whitelist all candidates
        whitelistedCandidates = allCandidates
      }

      yield put(setContentWhitelist(whitelistedCandidates))

      realm = yield call(getConfiguredRealm, allCandidates)
      if (!realm) {
        realm = yield call(pickCatalystRealm, allCandidates)
      }
    }

    yield put(initCatalystRealm(realm))
  } else {
    yield put(setCatalystCandidates([]))
    yield put(setAddedCatalystCandidates([]))
    yield put(setContentWhitelist([]))
    yield put(
      initCatalystRealm({
        domain: window.location.origin,
        catalystName: 'localhost',
        layer: 'stub',
        lighthouseVersion: '0.1'
      })
    )
  }

  yield put(catalystRealmInitialized())
}

function getConfiguredRealm(candidates: Candidate[]) {
  if (REALM) {
    return getRealmFromString(REALM, candidates)
  }
}

function* initializeCatalystCandidates() {
  const candidates: Candidate[] = yield call(fecthCatalystRealms)

  yield put(setCatalystCandidates(candidates))

  const added: string[] = yield select(getAddedServers)
  const addedCandidates: Candidate[] = yield call(fetchCatalystStatuses, added.map(url => ({ domain: url })))

  yield put(setAddedCatalystCandidates(addedCandidates))
}

async function checkValidRealm(realm: Realm) {
  return (
    realm.domain &&
    realm.catalystName &&
    realm.layer &&
    (await ping(`${realm.domain}/comms/status`)).status === ServerConnectionStatus.OK
  )
}

function* cacheCatalystRealm(action: InitCatalystRealm & SetCatalystRealm) {
  saveToLocalStorage(CACHE_KEY, action.payload)
}
