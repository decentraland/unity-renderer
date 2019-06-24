import { Vector2 } from 'decentraland-ecs/src/decentraland/math'
import { initParcelSceneWorker } from 'decentraland-loader/lifecycle/manager'
import { worldToGrid } from 'atomicHelpers/parcelScenePositions'
import { ETHEREUM_NETWORK } from 'config'

import { positionObservable, teleportObservable } from './positionThings'
import { SceneWorker, ParcelSceneAPI } from './SceneWorker'
import { LoadableParcelScene, EnvironmentData, ILand, ILandToLoadableParcelScene } from '../types'
import { ScriptingTransport } from 'decentraland-rpc/lib/common/json-rpc/types'

export type EnableParcelSceneLoadingOptions = {
  parcelSceneClass: { new (x: EnvironmentData<LoadableParcelScene>): ParcelSceneAPI }
  shouldLoadParcelScene: (parcelToLoad: ILand) => boolean
  onSpawnpoint?: (initialLand: ILand) => void
  onLoadParcelScenes?(x: ILand[]): void
}

export const loadedParcelSceneWorkers = new Map<string, SceneWorker>()

const sceneWorkerByBaseCoordinate = new Map<string, SceneWorker>()
const userViewMatrixWorkers = new Set<SceneWorker>()
export const loadedSceneWorkers = new Set<SceneWorker>()

/**
 * Retrieve the Scene based on the Scene Root CID
 */
export function getSceneWorkerByRootCID(rootCID: string) {
  return loadedParcelSceneWorkers.get(rootCID)
}

/**
 * Returns the CID of the parcel scene
 */
export function getParcelSceneRootCID(parcelScene: ParcelSceneAPI) {
  return parcelScene.data.id
}

export function getSceneWorkerByBaseCoordinates(coordinates: string) {
  return sceneWorkerByBaseCoordinate.get(coordinates)
}

export function getBaseCoordinates(worker: SceneWorker) {
  return (
    worker &&
    worker.parcelScene &&
    worker.parcelScene.data &&
    worker.parcelScene.data.data &&
    worker.parcelScene.data.data.scene &&
    worker.parcelScene.data.data.scene.base
  )
}

export function stopParcelSceneWorker(worker: SceneWorker) {
  if (worker && !worker.persistent) {
    forceStopParcelSceneWorker(worker)
  }
}

export function forceStopParcelSceneWorker(worker: SceneWorker) {
  worker.dispose()
  const rootCID = getParcelSceneRootCID(worker.parcelScene)
  const parcelSceneWorker = loadedParcelSceneWorkers.get(rootCID)!
  loadedParcelSceneWorkers.delete(rootCID)
  const baseCoordinates = getBaseCoordinates(parcelSceneWorker)
  if (baseCoordinates) {
    sceneWorkerByBaseCoordinate.delete(baseCoordinates)
  }
  userViewMatrixWorkers.delete(worker)
  loadedSceneWorkers.delete(worker)
}

export function loadParcelScene(parcelScene: ParcelSceneAPI, transport?: ScriptingTransport) {
  const id = getParcelSceneRootCID(parcelScene)
  if (loadedParcelSceneWorkers.get(id)) {
    return loadedParcelSceneWorkers.get(id)!
  }
  const parcelSceneWorker = new SceneWorker(parcelScene, transport)

  if (parcelSceneWorker) {
    loadedParcelSceneWorkers.set(getParcelSceneRootCID(parcelScene), parcelSceneWorker)

    if ('sendUserViewMatrix' in parcelSceneWorker) {
      userViewMatrixWorkers.add(parcelSceneWorker)
    }
    loadedSceneWorkers.add(parcelSceneWorker)
    const baseCoordinates = getBaseCoordinates(parcelSceneWorker)
    if (baseCoordinates) {
      sceneWorkerByBaseCoordinate.set(baseCoordinates, parcelSceneWorker)
    }
  }
  return parcelSceneWorker
}

export async function enableParcelSceneLoading(network: ETHEREUM_NETWORK, options: EnableParcelSceneLoadingOptions) {
  const ret = await initParcelSceneWorker(network)
  const position = Vector2.Zero()

  ret.on('Scene.shouldPrefetch', async (opts: { sceneCID: string }) => {
    const parcelSceneToLoad = await ret.getParcelData(opts.sceneCID)
    if (!options.shouldLoadParcelScene(parcelSceneToLoad)) {
      return
    }
    if (!getSceneWorkerByRootCID(opts.sceneCID)) {
      const parcelScene = new options.parcelSceneClass(ILandToLoadableParcelScene(parcelSceneToLoad))
      loadParcelScene(parcelScene)
    }
    ret.notify('Scene.prefetchDone', opts)
  })

  ret.on('Scene.shouldStart', async (opts: { sceneCID: string }) => {
    if (options.onLoadParcelScenes) {
      options.onLoadParcelScenes([await ret.getParcelData(opts.sceneCID)])
    }
  })

  ret.on('Scene.shouldUnload', async (opts: { sceneCID: string }) => {
    const worker = loadedParcelSceneWorkers.get(opts.sceneCID)
    if (!worker) {
      return
    }
    stopParcelSceneWorker(worker)
  })

  ret.on('Position.settled', async (sceneCID: string) => {
    options.onSpawnpoint && options.onSpawnpoint(await ret.getParcelData(sceneCID))
  })

  teleportObservable.add((position: { x: number; y: number }) => {
    ret.notify('User.setPosition', { position })
  })

  positionObservable.add(obj => {
    worldToGrid(obj.position, position)
    ret.notify('User.setPosition', { position })
  })

  enablePositionReporting()
}

let isPositionReportingEnabled = false

export function enablePositionReporting() {
  if (isPositionReportingEnabled) return

  isPositionReportingEnabled = true
  const position = Vector2.Zero()

  positionObservable.add(obj => {
    worldToGrid(obj.position, position)
    for (let parcelSceneWorker of userViewMatrixWorkers) {
      parcelSceneWorker.sendUserViewMatrix(obj)
    }
  })
}
