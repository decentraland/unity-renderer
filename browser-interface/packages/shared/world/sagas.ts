import { encodeParcelPosition } from 'lib/decentraland/parcels/encodeParcelPosition'
import { apply, call, fork, put, race, select, take, takeLatest } from 'redux-saga/effects'
import { SceneStart, SCENE_CHANGED, SCENE_START } from 'shared/loading/actions'
import { SetParcelPosition, SET_PARCEL_POSITION } from 'shared/scene-loader/actions'
import { getParcelPositionAsString } from 'shared/scene-loader/selectors'
import { getCurrentUserId } from 'shared/session/selectors'
import { RendererSignalSceneReady, RENDERER_SIGNAL_SCENE_READY, setCurrentScene } from './actions'
import {
  getLoadedParcelSceneByParcel,
  getSceneWorkerBySceneID,
  getSceneWorkerBySceneNumber,
  loadedSceneWorkers
} from './parcelSceneManager'
import { positionObservable } from './positionThings'
import { SceneWorker } from './SceneWorker'

export function* worldSagas() {
  // FIRST bind all sagas
  yield fork(dispatchSelfEnterAndLeaveSceneSignals)
  yield fork(updateUrlPositionInBrowser)
  yield fork(anounceOnEnterOnSceneStart)
  yield fork(anounceOnReadyOnSceneReady)
  yield fork(sendViewMatrixToScenes)
}

// This saga only updates the URL with the current parcel. It only works in the
// browser
function* updateUrlPositionInBrowser() {
  if (!globalThis.history || !globalThis.location) return

  let lastTime: number = performance.now()

  function replaceQueryStringPosition(x: any, y: any) {
    const currentPosition = encodeParcelPosition({ x, y })

    const q = new URLSearchParams(globalThis.location.search)
    q.set('position', currentPosition)

    globalThis.history.replaceState({ position: currentPosition }, 'position', `?${q.toString()}`)
  }

  function updateUrlPosition(action: SetParcelPosition) {
    const newParcel = action.payload.position
    // Update position in URI every second
    if (performance.now() - lastTime > 1000) {
      replaceQueryStringPosition(newParcel.x, newParcel.y)
      lastTime = performance.now()
    }
  }

  // react to all position changes
  yield takeLatest(SET_PARCEL_POSITION, updateUrlPosition)
}

// This saga reacts to every parcel position change and worker load/unload
// to always emit the correct message when we are either standing, entering or
// leaving a scene.
// It also calls the .onEnter and .onLeave of the SceneWorker(s)
function* dispatchSelfEnterAndLeaveSceneSignals() {
  let lastPlayerSceneId: string | undefined = undefined

  while (true) {
    yield race({
      position: take(SET_PARCEL_POSITION),
      sceneChanged: take(SCENE_CHANGED)
    })

    const parcelString: string = yield select(getParcelPositionAsString)
    const loadedLastPlayerScene: SceneWorker | undefined = lastPlayerSceneId
      ? getSceneWorkerBySceneID(lastPlayerSceneId)
      : undefined

    if (lastPlayerSceneId && !loadedLastPlayerScene) {
      // clear the lastPlayerScene if the scene was unloaded, there is no need to
      // call (null).onLeave when the scene was unloaded
      lastPlayerSceneId = undefined
    }

    const isStillInLastScene = loadedLastPlayerScene?.metadata.scene?.parcels?.includes(parcelString) || false

    const isInDifferentScene = !lastPlayerSceneId || !isStillInLastScene

    if (isInDifferentScene) {
      // find which scene we are standing in
      const newScene: SceneWorker | undefined = yield call(getLoadedParcelSceneByParcel, parcelString)
      const newSceneId = newScene?.loadableScene.id
      const userId: string = yield select(getCurrentUserId)

      // if the scene is the same as before then we continue
      if (newScene === loadedLastPlayerScene) continue

      // notify new scene and store it
      yield put(setCurrentScene(newSceneId, lastPlayerSceneId))

      // send signals of enter and leave to the scenes
      if (newScene) yield apply(newScene, newScene.onEnter, [userId])
      if (loadedLastPlayerScene) yield apply(loadedLastPlayerScene, loadedLastPlayerScene.onLeave, [userId, true])

      // new state for next iteration
      lastPlayerSceneId = newScene?.loadableScene.id
    }
  }
}

// This saga sends an onEnter signal to the scenes upon SCENE_START event.
// - when the scene is a portable experience, it works at any position in the world
// - when the scene is a parcel-scene it only triggers the event when inside a parcel
export function* anounceOnEnterOnSceneStart() {
  while (true) {
    const event: SceneStart = yield take(SCENE_START)
    const parcelString: string = yield select(getParcelPositionAsString)
    const scene: SceneWorker | undefined = yield call(getLoadedParcelSceneByParcel, parcelString)

    const shouldAnnounceOnEnter =
      // if the scene that loads is the one we are standing in right now
      scene?.loadableScene.id === event.payload.id ||
      // or if its a portable experience
      scene?.rpcContext.sceneData.isPortableExperience

    if (shouldAnnounceOnEnter) {
      const userId: string = yield select(getCurrentUserId)
      yield apply(scene, scene.onEnter, [userId])
    }
  }
}

// This saga sends an onReady signal to the scenes upon RENDERER_SIGNAL_SCENE_READY event.
// This single function plays a key role in the "loading screen logic" and to settle
// the position and spawn points of scenes. Follow the code in scene.onReady for more
// details
export function* anounceOnReadyOnSceneReady() {
  while (true) {
    const event: RendererSignalSceneReady = yield take(RENDERER_SIGNAL_SCENE_READY)
    const scene: SceneWorker | undefined = event.payload.sceneNumber
      ? yield call(getSceneWorkerBySceneNumber, event.payload.sceneNumber)
      : yield call(getSceneWorkerBySceneID, event.payload.sceneId)

    if (scene) {
      yield apply(scene, scene.onReady, [])
    }
  }
}

// This saga sends the view matrix to the scenes
function* sendViewMatrixToScenes() {
  positionObservable.add((obj) => {
    for (const [, scene] of loadedSceneWorkers) {
      scene.sendUserViewMatrix(obj)
    }
  })
}
