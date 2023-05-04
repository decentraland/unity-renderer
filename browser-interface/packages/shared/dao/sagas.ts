import {
  ETHEREUM_NETWORK,
  PIN_CATALYST,
  PREVIEW,
  rootURLPreviewMode
} from 'config'
import { getFromPersistentStorage, saveToPersistentStorage } from 'lib/browser/persistentStorage'
import defaultLogger from 'lib/logger'
import { waitFor } from 'lib/redux'
import { call, put, select, take, takeEvery } from 'redux-saga/effects'
import { SET_ROOM_CONNECTION } from 'shared/comms/actions'
import { getCommsRoom } from 'shared/comms/selectors'
import { BringDownClientAndReportFatalError } from 'shared/loading/ReportFatalError'
import {
  getAddedServers,
  getCatalystNodesEndpoint,
  getDisabledCatalystConfig,
  getPickRealmsAlgorithmConfig
} from 'shared/meta/selectors'
import { SET_REALM_ADAPTER, setOnboardingState } from 'shared/realm/actions'
import { candidateToRealm, urlWithProtocol } from 'shared/realm/resolver'
import {
  getFetchContentServerFromRealmAdapter,
  getOnboardingState,
  getProfilesContentServerFromRealmAdapter
} from 'shared/realm/selectors'
import { IRealmAdapter, OnboardingState } from 'shared/realm/types'
import { waitForRealm } from 'shared/realm/waitForRealmAdapter'
import { getParcelPosition } from 'shared/scene-loader/selectors'
import { USER_AUTHENTICATED } from 'shared/session/actions'
import { getCurrentIdentity } from 'shared/session/selectors'
import { RootState } from 'shared/store/rootTypes'
import { CatalystNode } from 'lib/web3/fetchCatalystNodesFromContract'
import { changeRealm, fetchCatalystRealms, fetchCatalystStatuses } from '.'
import { waitForMetaConfigurationInitialization, waitForNetworkSelected } from '../meta/sagas'
import {
  catalystRealmsScanRequested,
  setCatalystCandidates,
  SetCatalystCandidates,
  SET_CATALYST_CANDIDATES
} from './actions'
import { defaultChainConfig } from './pick-realm-algorithm/defaults'
import { createAlgorithm } from './pick-realm-algorithm/index'
import { getAllCatalystCandidates, getCatalystCandidatesReceived } from './selectors'
import { Candidate, PingResult, Realm, ServerConnectionStatus } from './types'
import { ask, ping } from './utils/ping'
import { saveProfileDelta } from '../profiles/actions'

const waitForExplorerIdentity = waitFor(getCurrentIdentity, USER_AUTHENTICATED)

function getLastRealmCacheKey(network: ETHEREUM_NETWORK) {
  return 'last_realm_string_' + network
}
function getLastRealmCandidatesCacheKey(network: ETHEREUM_NETWORK) {
  return 'last_realm_string_candidates_' + network
}

export function* daoSaga(): any {
  yield takeEvery(SET_REALM_ADAPTER, cacheCatalystRealm)
  yield takeEvery(SET_CATALYST_CANDIDATES, cacheCatalystCandidates)
}

function* pickCatalystRealm() {
  const { candidates, currentUserParcel, config } = (yield select(getInformationForCatalystPicker)) as ReturnType<
    typeof getInformationForCatalystPicker
  >
  if (candidates.length === 0) return undefined

  const algorithm = createAlgorithm(config)

  const realm: Realm = yield call(
    candidateToRealm,
    algorithm.pickCandidate(candidates, [currentUserParcel.x, currentUserParcel.y])
  )

  return urlWithProtocol(realm.hostname)
}

function getInformationForCatalystPicker(state: RootState) {
  const config = getPickRealmsAlgorithmConfig(state)
  return {
    candidates: getAllCatalystCandidates(state),
    currentUserParcel: getParcelPosition(state),
    config: !config || !config.length ? defaultChainConfig : config
  }
}

function qsRealm() {
  const qs = new URLSearchParams(document.location.search)
  return qs.get('realm')
}

function clearQsRealm() {
  const q = new URLSearchParams(globalThis.location.search)
  q.delete('realm')
  globalThis.history.replaceState({}, 'realm', `?${q.toString()}`)
}

function* tryConnectRealm() {
  const realm: string | undefined = yield call(selectRealm)

  if (realm) {
    yield call(waitForExplorerIdentity)
    yield call(changeRealm, realm, true)
  } else {
    throw new Error("Couldn't select a suitable realm to join.")
  }
}

/**
 * This method will try to load the candidates as well as the selected realm.
 *
 * The strategy to select the realm in terms of priority is:
 * 1- Realm configured in the URL and cached candidate for that realm (uses cache, forks async candidate initialization)
 * 2- Realm configured in the URL but no corresponding cached candidate (implies sync candidate initialization)
 * 3- Last cached realm (uses cache, forks async candidate initialization)
 * 4- Best pick from candidate scan (implies sync candidate initialization)
 */
export function* selectAndReconnectRealm() {
  try {
    yield call(tryConnectRealm)
    // if no realm was selected, then do the whole initialization dance
  } catch (e: any) {
    // if it failed, try changing the queryString
    clearQsRealm()
    try {
      // and try again
      yield call(tryConnectRealm)
    } catch (e: any) {
      debugger
      BringDownClientAndReportFatalError(e, 'comms#init')
      throw e
    }
  }
}

function* waitForCandidates() {
  while (!(yield select(getCatalystCandidatesReceived))) {
    yield take(SET_CATALYST_CANDIDATES)
  }
}

function* selectRealm() {
  const network: ETHEREUM_NETWORK = yield call(waitForNetworkSelected)

  yield call(initializeCatalystCandidates)

  const candidatesReceived = yield select(getCatalystCandidatesReceived)

  if (!candidatesReceived) {
    yield call(waitForCandidates)
  }

  const realm: string | undefined =
    // query param (dao candidates & cached)
    (yield call(qsRealm)) ||
    // preview mode
    (PREVIEW ? rootURLPreviewMode() : null) ||
    // CATALYST from url parameter
    PIN_CATALYST ||
    // fetch catalysts and select one using the load balancing
    (yield call(pickCatalystRealm)) ||
    // cached in local storage
    (yield call(getRealmFromLocalStorage, network))

  if (!realm) {
    BringDownClientAndReportFatalError(
      new Error('Could not connect to any catalyst servers. Please check your internet connection and try again.'),
      'comms#init'
    )
  }

  console.log(`Trying to connect to realm `, realm)

  return realm
}

// load realm from local storage
async function getRealmFromLocalStorage(network: ETHEREUM_NETWORK) {
  const key = getLastRealmCacheKey(network)
  try {
    const realm: string = await getFromPersistentStorage(key)
    if (typeof realm === 'string' && realm && (await checkValidRealm(realm))) {
      return realm
    }
  } catch {
    await saveToPersistentStorage(key, null)
  }
}

function* initializeCatalystCandidates() {
  yield call(waitForMetaConfigurationInitialization)
  yield put(catalystRealmsScanRequested())

  const { catalystsNodesEndpointURL, added, denylistedCatalysts } = (yield select(
    getInformationForInitializeCatalystCandidates
  )) as ReturnType<typeof getInformationForInitializeCatalystCandidates>

  const nodes: CatalystNode[] = yield call(fetchCatalystRealms, catalystsNodesEndpointURL)

  const candidates: Candidate[] = yield call(
    fetchCatalystStatuses,
    added.map((url) => ({ domain: url })).concat(nodes),
    denylistedCatalysts,
    ask
  )

  yield put(setCatalystCandidates(candidates))
}

function getInformationForInitializeCatalystCandidates(state: RootState) {
  return {
    catalystsNodesEndpointURL: getCatalystNodesEndpoint(state),
    added: getAddedServers(state),
    denylistedCatalysts: getDisabledCatalystConfig(state) || []
  }
}

export async function checkValidRealm(baseUrl: string): Promise<PingResult | null> {
  const pingResult = await ping(baseUrl + '/about')
  const acceptingUsers = pingResult.result && (pingResult.result.acceptingUsers ?? true)
  if (pingResult.status === ServerConnectionStatus.OK && acceptingUsers) {
    return pingResult
  }
  return null
}

function* cacheCatalystRealm() {
  const network: ETHEREUM_NETWORK = yield call(waitForNetworkSelected)
  // PRINT DEBUG INFO
  const dao: string = yield select((state) => state.dao)
  const realmAdapter: IRealmAdapter = yield call(waitForRealm)
  const onboardingInfo: OnboardingState = yield select(getOnboardingState)
  if (
    onboardingInfo.isInOnboarding &&
    realmAdapter.about.configurations?.realmName !== onboardingInfo.onboardingRealm
  ) {
    yield put(saveProfileDelta({ tutorialStep: 256 }))
    yield put(setOnboardingState({ isInOnboarding: false }))
  }

  if (realmAdapter) {
    yield call(saveToPersistentStorage, getLastRealmCacheKey(network), realmAdapter.baseUrl)
  }

  const fetchContentServer: string = getFetchContentServerFromRealmAdapter(realmAdapter)
  const updateContentServer: string = getProfilesContentServerFromRealmAdapter(realmAdapter)

  defaultLogger.info(`Using Catalyst configuration: `, {
    original: dao,
    calculated: {
      fetchContentServer,
      updateContentServer
    }
  })
}

function* cacheCatalystCandidates(_action: SetCatalystCandidates) {
  const allCandidates: Candidate[] = yield select(getAllCatalystCandidates)
  const network: ETHEREUM_NETWORK = yield call(waitForNetworkSelected)
  yield call(saveToPersistentStorage, getLastRealmCandidatesCacheKey(network), allCandidates)
}

export const waitForRoomConnection = waitFor(getCommsRoom, SET_ROOM_CONNECTION)
