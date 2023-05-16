import { Vector2, Vector3 } from '@dcl/ecs-math'
import { ENABLE_EMPTY_SCENES, isRunningTest } from 'config'
import { encodeParcelPosition } from 'lib/decentraland/parcels/encodeParcelPosition'
import { gridToWorld } from 'lib/decentraland/parcels/gridToWorld'
import { worldToGrid } from 'lib/decentraland/parcels/worldToGrid'
import { waitFor } from 'lib/redux'
import { apply, call, delay, fork, put, race, select, take, takeEvery, takeLatest } from 'redux-saga/effects'
import { BEFORE_UNLOAD } from 'shared/meta/actions'
import { trackEvent } from 'shared/analytics/trackEvent'
import { SceneStart, SceneUnload, SCENE_START, SCENE_UNLOAD } from 'shared/loading/actions'
import { getResourcesURL } from 'shared/location'
import { getAllowedContentServer } from 'shared/meta/selectors'
import { SetRealmAdapterAction, SET_REALM_ADAPTER } from 'shared/realm/actions'
import {
  getFetchContentServerFromRealmAdapter,
  isPreviousAdapterWorld,
  isWorldLoaderActive
} from 'shared/realm/selectors'
import { IRealmAdapter } from 'shared/realm/types'
import { signalParcelLoadingStarted } from 'shared/renderer/actions'
import { waitForRendererInstance } from 'shared/renderer/sagas-helper'
import { CHANGE_LOGIN_STAGE } from 'shared/session/actions'
import { isLoginCompleted } from 'shared/session/selectors'
import { store } from 'shared/store/isolatedStore'
import { RootState } from 'shared/store/rootTypes'
import { LoadableScene } from 'shared/types'
import { getSceneWorkerBySceneID, setDesiredParcelScenes } from 'shared/world/parcelSceneManager'
import { pickWorldSpawnpoint, positionObservable, receivePositionReport } from 'shared/world/positionThings'
import { sceneEvents, SceneWorker } from 'shared/world/SceneWorker'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import {
  positionSettled,
  PositionSettled,
  positionUnsettled,
  POSITION_SETTLED,
  POSITION_UNSETTLED,
  setParcelPosition,
  setSceneLoader,
  SET_PARCEL_POSITION,
  SET_SCENE_LOADER,
  SET_WORLD_LOADING_RADIUS,
  teleportToAction,
  TeleportToAction,
  TELEPORT_TO
} from './actions'
import { createGenesisCityLoader } from './genesis-city-loader-impl'
import {
  getLoadingRadius,
  getParcelPosition,
  getPositionSettled,
  getPositionSpawnPointAndScene,
  getSceneLoader,
  isPositionSettled
} from './selectors'
import { ISceneLoader, SceneLoaderPositionReport, SetDesiredScenesCommand } from './types'
import { createWorldLoader } from './world-loader-impl'
import { Entity } from '@dcl/schemas'
import { getContentService } from '../dao/selectors'

export function* sceneLoaderSaga() {
  yield takeLatest(SET_REALM_ADAPTER, setSceneLoaderOnSetRealmAction)
  yield takeEvery([POSITION_SETTLED, POSITION_UNSETTLED], onPositionSettled)
  yield takeLatest(TELEPORT_TO, teleportHandler)
  yield takeLatest(SET_SCENE_LOADER, unsettlePositionOnSceneLoader)
  yield fork(rendererPositionSettler)
  yield fork(onWorldPositionChange)
  yield fork(positionSettler)

  yield call(initSceneStateListener)
  yield call(initPositionListener)
}

// this function listens for position changes of the player and dispatches an
// action every time the current parcel changes.
function initPositionListener() {
  const lastPlayerParcel: Vector2 = new Vector2(Infinity, Infinity)
  const currentParcel = Vector2.Zero()

  positionObservable.add((event) => {
    worldToGrid(event.position, currentParcel)

    // if the position is not settled it may mean that we may be teleporting or
    // loading the world (without an esablished spawn point) and for that reason,
    // we shouldn't update the currentParcel, which would trigger unwanted teleports
    const positionSettled = getPositionSettled(store.getState())

    if (!currentParcel.equals(lastPlayerParcel) && positionSettled) {
      queueMicrotask(() => {
        store.dispatch(setParcelPosition(currentParcel))
        lastPlayerParcel.copyFrom(currentParcel)
      })
    }
  })
}

// this function listens for all the scene events and sends them to the store to
// be processed by sagas
function initSceneStateListener() {
  sceneEvents.on('*', (_type, action) => {
    // This action needs to run in a microTask to decouple the execution when
    // the event is triggered inside a saga. Sagas cannot invoke the store.dispatch
    // directly
    queueMicrotask(() => {
      store.dispatch(action)
    })
  })
}

const waitForSceneLoader = waitFor(getSceneLoader, SET_SCENE_LOADER)

// We teleport the user to its current position on every change of scene loader
// to unsettle the position.
function* unsettlePositionOnSceneLoader() {
  const fromWorld = yield select(isPreviousAdapterWorld)
  const unsettledPosition: Vector2 = fromWorld ? new Vector2(0, 0) : yield select(getParcelPosition)

  yield put(teleportToAction({ position: gridToWorld(unsettledPosition.x, unsettledPosition.y) }))
}

/**
 * Position settling algorithm:
 * - If the user teleports to a scene that is not present or not loaded
 *   AND the target scene exists, then UnsettlePosition(targetScene)
 * - If the user teleports to a scene that is loaded
 *   THEN SettlePosition(spawnPoint(scene))
 * - If the position is unsettled, and the scene that unsettled the position loads or fails loading
 *   THEN SettlePosition(spawnPoint(scene))
 *
 * A scene can fail loading due to an error or timeout.
 */
function* teleportHandler(action: TeleportToAction) {
  yield put(setParcelPosition(worldToGrid(action.payload.position)))

  const sceneLoader: ISceneLoader = yield call(waitForSceneLoader)
  try {
    // look for the target scene
    const pointer = encodeParcelPosition(worldToGrid(action.payload.position))
    const command: SetDesiredScenesCommand = yield apply(sceneLoader, sceneLoader.fetchScenesByLocation, [[pointer]])

    // is a target scene, then it will be used to settle the position
    if (command && command.scenes && command.scenes.length) {
      // pick always the first scene to unsettle the position once loaded
      const settlerScene = command.scenes[0].id

      const scene: SceneWorker | undefined = yield call(getSceneWorkerBySceneID, settlerScene)

      const spawnPoint =
        pickWorldSpawnpoint(
          scene?.metadata || command.scenes[0].entity.metadata,
          new Vector3(action.payload.position.x, action.payload.position.y, action.payload.position.z)
        ) || action.payload
      if (scene?.isStarted()) {
        // if the scene is loaded then there is no unsettlement of the position
        // we teleport directly to that scene
        yield put(positionSettled(spawnPoint))
      } else {
        // set the unsettler once again using the proper ID
        yield put(positionUnsettled(settlerScene, spawnPoint))
      }
    } else {
      // if there is no scene to load at the target position, then settle the position
      // to activate the renderer. otherwise there will be no event to activate the renderer
      yield put(positionSettled(action.payload))
    }
  } catch (err: any) {
    trackEvent('error', {
      context: 'teleportHandler',
      message: err.message,
      stack: err.stack
    })
  }
}

const waitForUserAuthenticated = waitFor(isLoginCompleted, CHANGE_LOGIN_STAGE)

function* rendererPositionSettler() {
  // wait for renderer
  yield call(waitForRendererInstance)
  // wait for signup to be finished (teleporting interferes with the signup logic on the renderer)
  yield call(waitForUserAuthenticated)

  while (true) {
    const isSettled: boolean = yield select(isPositionSettled)
    const spawnPointAndScene: ReturnType<typeof getPositionSpawnPointAndScene> = yield select(
      getPositionSpawnPointAndScene
    )

    if (!isSettled && !!spawnPointAndScene.sceneId) {
      // Then set the parcel position for the scene loader
      receivePositionReport(spawnPointAndScene.spawnPoint.position)
    }
    // then update the position in the engine
    getUnityInstance().Teleport(spawnPointAndScene.spawnPoint)
    yield take([POSITION_SETTLED, POSITION_UNSETTLED])
  }
}

function* onPositionSettled(action: PositionSettled | PositionSettled) {
  // set the parcel position for the scene loader
  yield put(setParcelPosition(worldToGrid(action.payload.spawnPoint.position)))
}

// This saga reacts to new realms/bff and creates the proper scene loader
function* setSceneLoaderOnSetRealmAction(action: SetRealmAdapterAction) {
  const adapter: IRealmAdapter | undefined = action.payload

  if (!adapter) {
    yield put(setSceneLoader(undefined))
  } else {
    // if the /about endpoint returns scenesUrn(s) then those need to be loaded
    // and the genesis city should not start
    const loadFixedWorld = isWorldLoaderActive(adapter)

    if (loadFixedWorld) {
      // TODO: disable green blockers here

      const loader: ISceneLoader = yield call(createWorldLoader, {
        urns: adapter!.about.configurations!.scenesUrn
      })
      yield put(setSceneLoader(loader))
    } else {
      const enableEmptyParcels = ENABLE_EMPTY_SCENES && isRunningTest

      const emptyParcelsBaseUrl = enableEmptyParcels
        ? getResourcesURL('.') // resolve explorer path to look for empty parcels
        : undefined

      const contentServer: string = yield select(
        getAllowedContentServer,
        adapter.about.configurations?.cityLoaderContentServer || getFetchContentServerFromRealmAdapter(adapter)
      )

      const loader: ISceneLoader = yield call(createGenesisCityLoader, {
        contentServer,
        emptyParcelsBaseUrl
      })
      yield put(setSceneLoader(loader))
    }

    yield put(signalParcelLoadingStarted())
  }
}

/**
 * This saga listens for scene loading messages (load, start, fail) and if there
 * is one scene that is going to settle our position, the event is used to dispach
 * the positionSettled(spawnPoint(scene)) action. Which is used to deactivate the
 * loading screen.
 */
function* positionSettler() {
  while (true) {
    const reason: SceneStart | SceneUnload = yield take([SCENE_START, SCENE_UNLOAD])

    const sceneId: string = reason.payload?.id

    if (!sceneId) {
      throw new Error(
        'Error in the logic of positionSettler saga:\n' +
          'Someone sent a SCENE_START, SCENE_FAIL, or SCENE_UNLOAD message without an `id` field!'
      )
    }

    const settled: boolean = yield select(isPositionSettled)
    const spawnPointAndScene: ReturnType<typeof getPositionSpawnPointAndScene> = yield select(
      getPositionSpawnPointAndScene
    )

    if (!settled && sceneId === spawnPointAndScene?.sceneId) {
      if (reason.type === SCENE_START) {
        yield delay(100)
      }
      yield put(positionSettled(spawnPointAndScene.spawnPoint))
    }
  }
}

// This saga reacts to every parcel position change and signals the scene loader
// about it
function* onWorldPositionChange() {
  // wait for user authenticated before start loading
  yield call(waitForUserAuthenticated)

  // start the loop to load scenes
  while (true) {
    const { sceneLoader, position, loadingRadius } = (yield select(getPositionChangeInfo)) as ReturnType<
      typeof getPositionChangeInfo
    >

    if (sceneLoader) {
      const report: SceneLoaderPositionReport = {
        loadingRadius,
        position,
        teleported: false
      }
      try {
        const command: SetDesiredScenesCommand = yield apply(sceneLoader, sceneLoader.reportPosition, [report])

        const map = new Map<string, LoadableScene>()

        for (const scene of command.scenes) {
          map.set(scene.id, scene)
        }

        yield call(setDesiredParcelScenes, map)
      } catch (err: any) {
        trackEvent('error', {
          context: 'onWorldPositionChange',
          message: err.message,
          stack: err.stack
        })
      }
    }

    const { unload } = yield race({
      timeout: delay(5000),
      newSceneLoader: take(SET_SCENE_LOADER),
      newParcel: take(SET_PARCEL_POSITION),
      SCENE_START: take(SCENE_START),
      newLoadingRadius: take(SET_WORLD_LOADING_RADIUS),
      unload: take(BEFORE_UNLOAD)
    })

    if (unload) return
  }
}
function getPositionChangeInfo(state: RootState) {
  return {
    sceneLoader: getSceneLoader(state),
    position: getParcelPosition(state),
    loadingRadius: getLoadingRadius(state)
  }
}

export async function fetchScenesByLocation(positions: string[]): Promise<LoadableScene[]> {
  const sceneLoader = getSceneLoader(store.getState())
  if (!sceneLoader) return []
  const { scenes } = await sceneLoader.fetchScenesByLocation(positions)
  return scenes
}

export async function fetchActiveSceneInWorldContext(position: string[]): Promise<Array<Entity>> {
  const response = await fetch(getContentService(store.getState()) + '/entities/active', {
    method: 'post',
    headers: { 'content-type': 'application/json' },
    body: JSON.stringify({ pointers: position })
  })
  if (!response.ok) {
    throw new Error(`Failed to fetch active scene: ${response.statusText}`)
  }
  return await response.json()
}
