import { getAssetBundlesBaseUrl } from 'config'
import { LoginState } from 'kernel-web-interface'
import { now } from 'lib/javascript/now'
import { waitFor } from 'lib/redux'
import type { AnyAction } from 'redux'
import { call } from 'redux-saga-test-plan/matchers'
import { fork, put, race, select, take, takeEvery, takeLatest } from 'redux-saga/effects'
import { trackEvent } from 'shared/analytics/trackEvent'
import { getSelectedNetwork } from 'shared/catalystSelection/selectors'
import { getResourcesURL } from 'lib/decentraland/url/resourcesUrl'
import { RENDERER_INITIALIZED_CORRECTLY } from 'shared/renderer/types'
import { ChangeLoginStateAction, CHANGE_LOGIN_STAGE } from 'shared/session/actions'
import { onLoginCompleted } from 'shared/session/onLoginCompleted'
import { getCurrentUserId } from 'shared/session/selectors'
import type { RootState } from 'shared/store/rootTypes'
import type { LoadableScene } from 'shared/types'
import { loadedSceneWorkers } from 'shared/world/parcelSceneManager'
import { lastPlayerPosition } from 'shared/world/positionThings'
import { SceneWorkerReadyState } from 'shared/world/SceneWorker'
import {
  informPendingScenes,
  PENDING_SCENES,
  SceneFail,
  SceneLoad,
  SCENE_CHANGED,
  SCENE_FAIL,
  SCENE_LOAD,
  SCENE_START
} from './actions'
import { experienceStarted, metricsAuthSuccessful, metricsUnityClientLoaded } from './types'

export function* loadingSaga() {
  yield fork(function* initialSceneLoading() {
    yield call(onLoginCompleted)
    yield call(waitForSceneLoads)
    yield put(experienceStarted())
  })

  yield takeLatest([SCENE_FAIL, SCENE_LOAD, SCENE_START, SCENE_CHANGED], handleReportPendingScenes)

  yield takeEvery(SCENE_FAIL, reportFailedScene)

  // Required for reporting
  yield fork(translateActions)
  yield takeEvery(SCENE_LOAD, trackLoadTime)
}

function* waitForSceneLoads() {
  function scenesLoaded(state: RootState) {
    if (!state.renderer.parcelLoadingStarted) {
      return true
    }
    if (!state.loading.totalScenes) {
      return true
    }
    const { pendingScenes } = state.loading
    return pendingScenes > 0
  }
  yield call(waitFor, scenesLoaded, PENDING_SCENES)
}

function* reportFailedScene(action: SceneFail) {
  const { id, baseUrl } = action.payload
  const fullRootUrl = getResourcesURL('.')

  trackEvent('scene_loading_failed', {
    sceneId: id,
    contentServer: baseUrl,
    contentServerBundles: getAssetBundlesBaseUrl(yield select(getSelectedNetwork)) + '/',
    rootUrl: fullRootUrl
  })
}

function* translateActions() {
  yield takeEvery(RENDERER_INITIALIZED_CORRECTLY, function* triggerUnityClientLoaded() {
    yield put(metricsUnityClientLoaded())
  })
  yield takeEvery(CHANGE_LOGIN_STAGE, function* triggerAuthSuccessful(action: ChangeLoginStateAction) {
    if (action.payload.stage === LoginState.COMPLETED) {
      yield put(metricsAuthSuccessful())
    }
  })
}

/**
 * Reports the number of loading parcel scenes to unity to handle the loading states
 */
function* handleReportPendingScenes() {
  const pendingScenes = new Set<string>()

  let countableScenes = 0
  for (const [sceneId, sceneWorker] of loadedSceneWorkers) {
    const isPending = (sceneWorker.ready & SceneWorkerReadyState.STARTED) === 0
    const failedLoading = (sceneWorker.ready & SceneWorkerReadyState.LOADING_FAILED) !== 0

    countableScenes++

    if (isPending && !failedLoading) {
      pendingScenes.add(sceneId)
    }
  }

  yield put(informPendingScenes(pendingScenes.size, countableScenes, now()))
}

/**
 * Tracking of Scene loading times
 */
function* trackLoadTime(action: SceneLoad): any {
  const start = now()
  const { id } = action.payload
  const entityId = id
  const result = yield race({
    start: take(
      (action: AnyAction) => action.type === SCENE_START && (action.payload as LoadableScene).id === entityId
    ),
    fail: take((action: AnyAction) => action.type === SCENE_FAIL && (action.payload as LoadableScene).id === entityId)
  })
  const userId = yield select(getCurrentUserId)
  const position = lastPlayerPosition
  trackEvent('SceneLoadTimes', {
    position: { ...position },
    elapsed: now() - start,
    success: !!result.start,
    sceneId: entityId,
    userId: userId
  })
}
