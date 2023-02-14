import { AnyAction } from 'redux'
import { fork, put, race, select, take, takeEvery, takeLatest } from 'redux-saga/effects'

import { RENDERER_INITIALIZED_CORRECTLY } from 'shared/renderer/types'
import { CHANGE_LOGIN_STAGE, ChangeLoginStateAction } from 'shared/session/actions'
import { trackEvent } from '../analytics'
import { lastPlayerPosition } from '../world/positionThings'

import {
  informPendingScenes,
  SCENE_CHANGED,
  SCENE_FAIL,
  SCENE_LOAD,
  SCENE_START,
  SceneFail,
  SceneLoad
} from './actions'
import { experienceStarted, metricsAuthSuccessful, metricsUnityClientLoaded } from './types'
import { getCurrentUserId } from 'shared/session/selectors'
import { LoginState } from 'kernel-web-interface'
import { call } from 'redux-saga-test-plan/matchers'
import { RootState } from 'shared/store/rootTypes'
import { onLoginCompleted } from 'shared/session/onLoginCompleted'
import { getResourcesURL } from 'shared/location'
import { getSelectedNetwork } from 'shared/dao/selectors'
import { getAssetBundlesBaseUrl } from 'config'
import { loadedSceneWorkers } from 'shared/world/parcelSceneManager'
import { SceneWorkerReadyState } from 'shared/world/SceneWorker'
import { LoadableScene } from 'shared/types'
import { updateLoadingScreen } from '../loadingScreen/actions'
import { ACTIONS_FOR_LOADING } from '../loadingScreen/sagas'

export function* loadingSaga() {
  yield takeEvery(SCENE_LOAD, trackLoadTime)
  yield takeEvery(SCENE_FAIL, reportFailedScene)

  yield fork(translateActions)
  yield fork(initialSceneLoading)

  yield takeLatest([SCENE_FAIL, SCENE_LOAD, SCENE_START, SCENE_CHANGED], handleReportPendingScenes)
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
  yield takeEvery(RENDERER_INITIALIZED_CORRECTLY, triggerUnityClientLoaded)
  yield takeEvery(CHANGE_LOGIN_STAGE, triggerAuthSuccessful)
}

function* triggerAuthSuccessful(action: ChangeLoginStateAction) {
  if (action.payload.stage === LoginState.COMPLETED) {
    yield put(metricsAuthSuccessful())
  }
}

function* triggerUnityClientLoaded() {
  yield put(metricsUnityClientLoaded())
}

export function* trackLoadTime(action: SceneLoad): any {
  const start = new Date().getTime()
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
    elapsed: new Date().getTime() - start,
    success: !!result.start,
    sceneId: entityId,
    userId: userId
  })
}

function* waitForSceneLoads() {
  function shouldWaitForScenes(state: RootState) {
    if (!state.renderer.parcelLoadingStarted) {
      return true
    }

    // in the initial load, we should wait until we have *some* scene to load
    if (state.loading.initialLoad) {
      if (state.loading.pendingScenes !== 0 || state.loading.totalScenes === 0) {
        return true
      }
    }

    // otherwise only wait until pendingScenes == 0
    return state.loading.pendingScenes !== 0
  }

  while (yield select(shouldWaitForScenes)) {
    // these are the events that _may_ change the result of shouldWaitForScenes
    yield take(ACTIONS_FOR_LOADING)
  }

  // trigger the signal to apply the state in the renderer
  yield put(updateLoadingScreen())
}

function* initialSceneLoading() {
  yield call(onLoginCompleted)
  yield call(waitForSceneLoads)
  yield put(experienceStarted())
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

  yield put(informPendingScenes(pendingScenes.size, countableScenes))
}
