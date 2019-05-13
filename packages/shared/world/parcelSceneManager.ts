import { initParcelSceneWorker } from '../../decentraland-loader/lifecycle/manager'
import { ETHEREUM_NETWORK } from '../../config'
import { positionObservable, teleportObservable } from './positionThings'
import { worldToGrid } from '../../atomicHelpers/parcelScenePositions'
import { SceneWorker, ParcelSceneAPI } from './SceneWorker'
import { LoadableParcelScene, EnvironmentData, ILand, ILandToLoadableParcelScene } from '../types'
import { Vector2 } from 'decentraland-ecs/src/decentraland/math'

export type EnableParcelSceneLoadingOptions = {
  parcelSceneClass: { new (x: EnvironmentData<LoadableParcelScene>): ParcelSceneAPI }
  shouldLoadParcelScene: (parcelToLoad: ILand) => boolean
  onSpawnpoint?: (initialLand: ILand) => void
  onLoadParcelScenes?(x: ILand[]): void
}

export const loadedParcelSceneWorkers: Set<SceneWorker> = new Set()

/**
 * This function receives the list of { type: string, data: ILand } from a remote worker.
 * It loads and unloads the ParcelScenes from the world
 */
export function getParcelByCID(id: string) {
  for (let parcelSceneWorker of loadedParcelSceneWorkers) {
    if (parcelSceneWorker.parcelScene.data.id === id) {
      return parcelSceneWorker
    }
  }
  return null
}

/**
 * This function receives the list of { type: string, data: ILand } from a remote worker.
 * It loads and unloads the ParcelScenes from the world
 */
export function getParcelById(id: string) {
  for (let parcelSceneWorker of loadedParcelSceneWorkers) {
    if (parcelSceneWorker.parcelScene.data.data.id === id) {
      return parcelSceneWorker
    }
  }
  return null
}

export async function enableParcelSceneLoading(network: ETHEREUM_NETWORK, options: EnableParcelSceneLoadingOptions) {
  const ret = await initParcelSceneWorker(network)
  const position = Vector2.Zero()

  ret.on('Scene.shouldPrefetch', async (opts: { sceneCID: string }) => {
    const parcelSceneToLoad = await ret.getParcelData(opts.sceneCID)
    if (!options.shouldLoadParcelScene(parcelSceneToLoad)) {
      return
    }
    if (!getParcelById(parcelSceneToLoad.mappingsResponse.root_cid)) {
      const parcelScene = new options.parcelSceneClass(ILandToLoadableParcelScene(parcelSceneToLoad))

      const parcelSceneWorker = new SceneWorker(parcelScene)

      if (parcelSceneWorker) {
        loadedParcelSceneWorkers.add(parcelSceneWorker)
      }
    }
    ret.notify('Scene.prefetchDone', opts)
  })

  ret.on('Scene.shouldStart', async (opts: { sceneCID: string }) => {
    if (options.onLoadParcelScenes) {
      options.onLoadParcelScenes([await ret.getParcelData(opts.sceneCID)])
    }
  })

  ret.on('Scene.shouldUnload', async (sceneCID: string) => {
    const parcelSceneToUnload = await ret.getParcelData(sceneCID)
    loadedParcelSceneWorkers.forEach($ => {
      if (!$.persistent && $.parcelScene.data.id === parcelSceneToUnload.mappingsResponse.root_cid) {
        $.dispose()
        loadedParcelSceneWorkers.delete($)
      }
    })
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
    for (let parcelSceneWorker of loadedParcelSceneWorkers) {
      if (parcelSceneWorker && 'sendUserViewMatrix' in parcelSceneWorker) {
        parcelSceneWorker.sendUserViewMatrix(obj)
      }
    }
  })
}
