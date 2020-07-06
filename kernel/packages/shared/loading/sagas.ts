import { AnyAction } from 'redux'
import { call, delay, fork, put, race, select, take, takeEvery, takeLatest } from 'redux-saga/effects'
import { queueTrackingEvent } from '../analytics'
import { getCurrentUser } from '../comms/peers'
import { lastPlayerPosition } from '../world/positionThings'
import { SceneLoad, SCENE_FAIL, SCENE_LOAD, SCENE_START } from './actions'
import { LoadingState } from './reducer'
import { EXPERIENCE_STARTED, helpTexts, rotateHelpText, TELEPORT_TRIGGERED } from './types'

const SECONDS = 1000

export const DELAY_BETWEEN_MESSAGES = 10 * SECONDS

export function* loadingSaga() {
  yield fork(initialSceneLoading)
  yield takeLatest(TELEPORT_TRIGGERED, teleportSceneLoading)

  yield takeEvery(SCENE_LOAD, trackLoadTime)
}

export function* trackLoadTime(action: SceneLoad): any {
  const start = new Date().getTime()
  const sceneId = action.payload
  const result = yield race({
    start: take((action: AnyAction) => action.type === SCENE_START && action.payload === sceneId),
    fail: take((action: AnyAction) => action.type === SCENE_FAIL && action.payload === sceneId)
  })
  const user = yield select(getCurrentUser)
  const position = lastPlayerPosition
  queueTrackingEvent('SceneLoadTimes', {
    position: { ...position },
    elapsed: new Date().getTime() - start,
    success: !!result.start,
    sceneId,
    userId: user.userId
  })
}

function* refreshTeleport() {
  while (true) {
    yield delay(DELAY_BETWEEN_MESSAGES)
    yield put(rotateHelpText())
  }
}
function* refreshTextInScreen() {
  while (true) {
    const status = yield select((state) => state.loading)
    yield call(() => updateTextInScreen(status))
    yield delay(200)
  }
}

export function* waitForSceneLoads() {
  while (true) {
    yield race({
      started: take(SCENE_START),
      failed: take(SCENE_FAIL)
    })
    if (yield select((state) => state.loading.pendingScenes === 0)) {
      break
    }
  }
}

export function* initialSceneLoading() {
  yield race({
    refresh: call(refreshTeleport),
    textInScreen: call(refreshTextInScreen),
    finish: call(function* () {
      yield take(EXPERIENCE_STARTED)
      yield take('Loading scene')
      yield call(waitForSceneLoads)
    })
  })
}

export function* teleportSceneLoading() {
  cleanSubTextInScreen()
  yield race({
    refresh: call(refreshTeleport),
    textInScreen: call(function* () {
      yield delay(2000)
      yield call(refreshTextInScreen)
    }),
    finish: call(waitForSceneLoads)
  })
}

export function updateTextInScreen(status: LoadingState) {
  const messages = document.getElementById('load-messages')
  if (messages) {
    messages.innerText = helpTexts[status.helpText]
  }
  const subMessages = document.getElementById('subtext-messages')
  if (subMessages) {
    subMessages.innerText =
      status.pendingScenes > 0
        ? status.message || "Loading scenes..."
        : status.status
  }
}

function cleanSubTextInScreen() {
  const subMessages = document.getElementById('subtext-messages')
  if (subMessages) {
    subMessages.innerText = "Loading scenes..."
  }
}
