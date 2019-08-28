import { Vector2 } from 'decentraland-ecs/src/decentraland/math'
import { initParcelSceneWorker } from 'decentraland-loader/lifecycle/manager'
import { worldToGrid } from 'atomicHelpers/parcelScenePositions'

import { positionObservable, teleportObservable } from './positionThings'
import { SceneWorker, ParcelSceneAPI } from './SceneWorker'
import { LoadableParcelScene, EnvironmentData, ILand, ILandToLoadableParcelScene } from '../types'
import { ScriptingTransport } from 'decentraland-rpc/lib/common/json-rpc/types'
import { sceneLifeCycleObservable } from '../../decentraland-loader/lifecycle/controllers/scene'
import { worldRunningObservable } from './worldState'

export type EnableParcelSceneLoadingOptions = {
  parcelSceneClass: { new (x: EnvironmentData<LoadableParcelScene>): ParcelSceneAPI }
  preloadScene: (parcelToLoad: ILand) => Promise<any>
  onPositionSettled?: (initialLand?: ILand) => void
  onLoadParcelScenes?(x: ILand[]): void
  onUnloadParcelScenes?(x: ILand[]): void
  onPositionUnsettled?(): void
}

export const loadedSceneWorkers = new Map<string, SceneWorker>()

/**
 * Retrieve the Scene based on it's ID, usually RootCID
 */
export function getSceneWorkerBySceneID(sceneId: string) {
  return loadedSceneWorkers.get(sceneId)
}

/**
 * Returns the id of the scene, usually the RootCID
 */
export function getParcelSceneID(parcelScene: ParcelSceneAPI) {
  return parcelScene.data.sceneId
}

/** Stops non-persistent scenes (i.e UI scene) */
export function stopParcelSceneWorker(worker: SceneWorker) {
  if (worker && !worker.persistent) {
    forceStopParcelSceneWorker(worker)
  }
}

export function forceStopParcelSceneWorker(worker: SceneWorker) {
  worker.dispose()
}

export function loadParcelScene(parcelScene: ParcelSceneAPI, transport?: ScriptingTransport) {
  const sceneId = getParcelSceneID(parcelScene)

  let parcelSceneWorker = loadedSceneWorkers.get(sceneId)

  if (!parcelSceneWorker) {
    parcelSceneWorker = new SceneWorker(parcelScene, transport)

    loadedSceneWorkers.set(sceneId, parcelSceneWorker)

    parcelSceneWorker.onDisposeObservable.addOnce(() => {
      loadedSceneWorkers.delete(sceneId)
    })
  }

  return parcelSceneWorker
}

export async function enableParcelSceneLoading(options: EnableParcelSceneLoadingOptions) {
  const ret = await initParcelSceneWorker()
  const position = Vector2.Zero()

  ret.on('Scene.shouldPrefetch', async (opts: { sceneId: string }) => {
    const parcelSceneToLoad = await ret.getParcelData(opts.sceneId)

    // start and await prefetch
    await options.preloadScene(parcelSceneToLoad)

    // continue with the loading
    ret.notify('Scene.prefetchDone', opts)
  })

  ret.on('Scene.shouldStart', async (opts: { sceneId: string }) => {
    const sceneId = opts.sceneId
    const parcelSceneToStart = await ret.getParcelData(sceneId)

    // create the worker if don't exist
    if (!getSceneWorkerBySceneID(sceneId)) {
      const parcelScene = new options.parcelSceneClass(ILandToLoadableParcelScene(parcelSceneToStart))
      loadParcelScene(parcelScene)
    }

    const observer = sceneLifeCycleObservable.add(sceneStatus => {
      if (sceneStatus.sceneId === sceneId) {
        ret.notify('Scene.status', sceneStatus)
      }
      sceneLifeCycleObservable.remove(observer)
    })

    // tell the engine to load the parcel scene
    if (options.onLoadParcelScenes) {
      options.onLoadParcelScenes([await ret.getParcelData(sceneId)])
    }

    setTimeout(() => {
      const worker = getSceneWorkerBySceneID(sceneId)
      if (worker && !worker.sceneStarted) {
        sceneLifeCycleObservable.remove(observer)
        ret.notify('Scene.status', { sceneId, status: 'failed' })
      }
    }, 30000)
  })

  ret.on('Scene.shouldUnload', async (opts: { sceneId: string }) => {
    const worker = loadedSceneWorkers.get(opts.sceneId)
    if (!worker) {
      return
    }
    stopParcelSceneWorker(worker)
    if (options.onUnloadParcelScenes) {
      options.onUnloadParcelScenes([await ret.getParcelData(opts.sceneId)])
    }
  })

  ret.on('Position.settled', async (opts: { sceneId: string }) => {
    if (options.onPositionSettled) {
      options.onPositionSettled(opts.sceneId ? await ret.getParcelData(opts.sceneId) : undefined)
    }
  })

  ret.on('Position.unsettled', () => {
    if (options.onPositionUnsettled) {
      options.onPositionUnsettled()
    }
    worldRunningObservable.notifyObservers(false)
  })

  teleportObservable.add((position: { x: number; y: number }) => {
    ret.notify('User.setPosition', { position, teleported: true })
  })

  positionObservable.add(obj => {
    worldToGrid(obj.position, position)
    ret.notify('User.setPosition', { position, teleported: false })
  })
}
