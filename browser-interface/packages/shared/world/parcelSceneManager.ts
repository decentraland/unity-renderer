import { scenesChanged } from '../loading/actions'
import { LoadableScene } from '../types'
import { SceneWorker } from './SceneWorker'
import { store } from 'shared/store/isolatedStore'
import { Observable } from 'mz-observable'
import { ParcelSceneLoadingState } from './types'
import { getFeatureFlagVariantValue } from 'shared/meta/selectors'
import { defaultParcelPermissions } from 'shared/apis/host/Permissions'
import { getClient } from 'shared/renderer/selectors'

declare const globalThis: any

const PARCEL_DENY_LISTED_FEATURE_FLAG = 'parcel-denylist'
export function isParcelDenyListed(coordinates: string[]) {
  const denylist = getFeatureFlagVariantValue(store.getState(), PARCEL_DENY_LISTED_FEATURE_FLAG) as string

  const setOfCoordinates = new Set(coordinates)

  if (denylist) {
    return denylist.split(/[\s\r\n]+/gm).some(($) => setOfCoordinates.has($.trim()))
  }

  return false
}

export function generateBannedLoadableScene(entity: LoadableScene): LoadableScene {
  return {
    ...entity,
    entity: {
      ...entity.entity,
      content: []
    }
  }
}

export const onLoadParcelScenesObservable = new Observable<LoadableScene[]>()
export const loadedSceneWorkers = new Map<string, SceneWorker>()
globalThis['sceneWorkers'] = loadedSceneWorkers

/**
 * Retrieve the Scene based on it's ID, usually RootCID
 */
export function getSceneWorkerBySceneID(sceneId: string) {
  return loadedSceneWorkers.get(sceneId)
}

/**
 * Retrieve the Scene based on it's Scene Number
 */
export function getSceneWorkerBySceneNumber(sceneNumber: number) {
  // TODO: Optimize this fetch
  for (const sceneWorker of loadedSceneWorkers.values()) {
    if (sceneWorker.rpcContext.sceneData.sceneNumber === sceneNumber) {
      return sceneWorker
    }
  }
}

export function forceStopScene(sceneId: string) {
  const worker = loadedSceneWorkers.get(sceneId)
  if (worker) {
    worker.dispose()
    loadedSceneWorkers.delete(sceneId)
    store.dispatch(scenesChanged())
  }
}

// finds a parcel scene by parcel position (that is not a portable experience)
export function getLoadedParcelSceneByParcel(parcelPosition: string) {
  for (const [, w] of loadedSceneWorkers) {
    if (
      !w.rpcContext.sceneData.isPortableExperience &&
      !w.rpcContext.sceneData.isGlobalScene &&
      w.metadata.scene?.parcels?.includes(parcelPosition)
    ) {
      return w
    }
  }
}

/**
 * Creates a worker for the ParcelSceneAPI
 */
export async function loadParcelSceneWorker(loadableScene: LoadableScene) {
  const sceneId = loadableScene.id
  let parcelSceneWorker = loadedSceneWorkers.get(sceneId)

  if (!parcelSceneWorker) {
    const rpcClient = getClient(store.getState())
    if (!rpcClient) throw new Error('Cannot create a scene because there is no rpcClient')

    parcelSceneWorker = await SceneWorker.createSceneWorker(loadableScene, rpcClient)
    setNewParcelScene(parcelSceneWorker)
    queueMicrotask(() => store.dispatch(scenesChanged()))
  }

  return parcelSceneWorker
}

/**
 * idempotent.
 *
 * accepts a new worker, stops the previous one if there was any collision with
 * the same ID
 */
function setNewParcelScene(worker: SceneWorker) {
  const sceneId = worker.loadableScene.id
  // NOTE: getSceneWorkerBySceneID is not used because when the change to
  //       sceneNumber happens we still need to look for ID collissions

  // unload all the conflicting workers
  for (const [id, w] of loadedSceneWorkers) {
    if (worker !== w && w.loadableScene.id === sceneId) {
      forceStopScene(id)
    }
  }

  loadedSceneWorkers.set(sceneId, worker)
}

// @internal
export const parcelSceneLoadingState: ParcelSceneLoadingState = {
  isWorldLoadingEnabled: true,
  desiredParcelScenes: new Map()
}

/**
 *  @internal
 * Returns a set of Set<SceneId>
 */
export function getDesiredParcelScenes(): Map<string, LoadableScene> {
  return new Map(parcelSceneLoadingState.desiredParcelScenes)
}

/**
 * Receives a set of Set<SceneId>
 */
export async function setDesiredParcelScenes(desiredParcelScenes: Map<string, LoadableScene>) {
  const previousSet = new Set(parcelSceneLoadingState.desiredParcelScenes)
  const newSet = (parcelSceneLoadingState.desiredParcelScenes = desiredParcelScenes)

  // react to changes
  for (const [oldSceneId] of previousSet) {
    if (!newSet.has(oldSceneId) && loadedSceneWorkers.has(oldSceneId)) {
      // destroy old scene
      unloadParcelSceneById(oldSceneId)
    }
  }

  for (const [newSceneId, entity] of newSet) {
    if (!loadedSceneWorkers.has(newSceneId)) {
      // create new scene
      await loadParcelSceneByIdIfMissing(newSceneId, entity)
    }
  }
}

export async function reloadScene(sceneId: string) {
  unloadParcelSceneById(sceneId)
  await setDesiredParcelScenes(getDesiredParcelScenes())
}

export function unloadParcelSceneById(sceneId: string) {
  const worker = loadedSceneWorkers.get(sceneId)
  if (!worker) {
    return
  }
  forceStopScene(sceneId)
}

/**
 * @internal
 **/
async function loadParcelSceneByIdIfMissing(sceneId: string, entity: LoadableScene) {
  // create the worker if don't exis
  if (!getSceneWorkerBySceneID(sceneId)) {
    // If we are running in isolated mode and it is builder mode, we create a stateless worker instead of a normal worker
    const denyListed = isParcelDenyListed(entity.entity.metadata.scene.parcels)
    const usedEntity = denyListed ? generateBannedLoadableScene(entity) : entity

    const worker = await loadParcelSceneWorker({
      ...usedEntity,
      // and enablle FPS throttling, it will lower the frame-rate based on the distance
      useFPSThrottling: true
    })

    // add default permissions for Parcel based scenes
    defaultParcelPermissions.forEach(($) => worker.rpcContext.permissionGranted.add($))

    setNewParcelScene(worker)
  }
}

export type AllScenesEvents<T extends IEventNames> = {
  eventType: T
  payload: IEvents[T]
}

export function allScenesEvent<T extends IEventNames>(data: AllScenesEvents<T>) {
  for (const [, scene] of loadedSceneWorkers) {
    scene.rpcContext.sendSceneEvent(data.eventType, data.payload)
  }
  TEST_OBJECT_ObservableAllScenesEvent.notifyObservers(data)
}

export const TEST_OBJECT_ObservableAllScenesEvent = new Observable<AllScenesEvents<any>>()
