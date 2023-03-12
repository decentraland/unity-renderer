import { getAssetBundlesBaseUrl } from 'config'
import { LoginState } from 'kernel-web-interface'
import { now } from 'lib/javascript/now'
import { waitFor } from 'lib/redux'
import { AnyAction } from 'redux'
import { call, fork, put, race, select, take, takeEvery, takeLatest } from 'redux-saga/effects'
import { trackEvent } from 'shared/analytics/trackEvent'
import { getSelectedNetwork } from 'shared/dao/selectors'
import { getResourcesURL } from 'shared/location'
import { RENDERER_INITIALIZED_CORRECTLY } from 'shared/renderer/types'
import { ChangeLoginStateAction, CHANGE_LOGIN_STAGE } from 'shared/session/actions'
import { onLoginCompleted } from 'shared/session/onLoginCompleted'
import { getCurrentUserId } from 'shared/session/selectors'
import { LoadableScene } from 'shared/types'
import { loadedSceneWorkers } from 'shared/world/parcelSceneManager'
import { lastPlayerPosition } from 'shared/world/positionThings'
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
import { getPendingAndCountableScenes } from './lib/getPendingAndCountableScenes'
import { scenesLoaded } from './selectors'
import { experienceStarted, metricsAuthSuccessful, metricsUnityClientLoaded } from './types'

const SCENE_ACTIONS = [SCENE_FAIL, SCENE_LOAD, SCENE_START, SCENE_CHANGED]
export function* loadingSaga() {
  yield fork(function* initialSceneLoading() {
    yield call(onLoginCompleted)
    yield call(waitForSceneLoads)
    yield put(experienceStarted())
  })

  yield takeLatest(SCENE_ACTIONS, reportPendingScenesToUnity)

  yield takeEvery(SCENE_FAIL, reportFailedScene)

  // Required for reporting
  yield fork(translateActions)
  yield takeEvery(SCENE_LOAD, trackLoadTime)
}

const waitForSceneLoads = waitFor(scenesLoaded, SCENE_ACTIONS)

/**
 * Reports the number of loading parcel scenes to Unity to handle the loading states
 */
function* reportPendingScenesToUnity() {
  const { pendingScenes, countableScenes } = getPendingAndCountableScenes(loadedSceneWorkers)
  yield put(informPendingScenes(pendingScenes.size, countableScenes, now()))
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
