import { call, fork, put, select, takeEvery } from 'redux-saga/effects'
import {
  getAddedServers,
  getCatalystNodesEndpoint,
  getDisabledCatalystConfig,
  getPickRealmsAlgorithmConfig
} from 'shared/meta/selectors'
import { candidateToRealm } from 'shared/realm/resolver'
import { getParcelPosition } from 'shared/scene-loader/selectors'
import { CatalystNode } from 'shared/types'
import { waitForMetaConfigurationInitialization } from '../meta/sagas'
import {
  catalystRealmsScanFinished,
  catalystRealmsScanRequested,
  CATALYST_REALMS_SCAN_REQUESTED,
  setCatalystCandidates
} from './actions'
import { fetchCatalystNodes } from './fetch/nodes'
import { ask } from './fetch/ping'
import { fetchCatalystStatuses } from './fetch/statuses'
import { defaultChainConfig } from './pickAlgorithm/defaults'
import { createAlgorithm } from './pickAlgorithm/index'
import { AlgorithmChainConfig } from './pickAlgorithm/types'
import { getCatalystCandidates } from './selectors'
import { Candidate, Realm } from './types'
import { waitForCatalystCandidatesReceived } from './waitFor/candidates'

export function* catalystSelectionSaga(): any {
  yield fork(initializeCatalystCandidates)
  yield takeEvery(CATALYST_REALMS_SCAN_REQUESTED, pickCatalyst)
}

export function* pickCatalyst() {
  yield call(waitForCatalystCandidatesReceived)
  const candidates: Candidate[] = yield select(getCatalystCandidates)
  if (candidates.length === 0) return undefined
  const currentUserParcel: ReadOnlyVector2 = yield select(getParcelPosition)

  let config: AlgorithmChainConfig | undefined = yield select(getPickRealmsAlgorithmConfig)

  if (!config || config.length === 0) {
    config = defaultChainConfig
  }

  const algorithm = createAlgorithm(config)

  const realm: Realm = yield call(
    candidateToRealm,
    algorithm.pickCandidate(candidates, [currentUserParcel.x, currentUserParcel.y])
  )
  yield put(catalystRealmsScanFinished(realm))
  return realm.serverName
}

function* initializeCatalystCandidates() {
  yield call(waitForMetaConfigurationInitialization)

  const catalystsNodesEndpointURL: string | undefined = yield select(getCatalystNodesEndpoint)

  const nodes: CatalystNode[] = yield call(fetchCatalystNodes, catalystsNodesEndpointURL)
  const added: string[] = yield select(getAddedServers)

  const denylistedCatalysts: string[] = (yield select(getDisabledCatalystConfig)) ?? []

  const candidates: Candidate[] = yield call(
    fetchCatalystStatuses,
    added.map((url) => ({ domain: url })).concat(nodes),
    denylistedCatalysts,
    ask
  )

  yield put(setCatalystCandidates(candidates))
  yield put(catalystRealmsScanRequested())
}
