const qs = require('query-string')

import { initParcelSceneWorker } from '../../decentraland-loader/worker'
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
export function getParcelById(id: string) {
  for (let parcelSceneWorker of loadedParcelSceneWorkers) {
    if (parcelSceneWorker.parcelScene.data.id === id) {
      return parcelSceneWorker
    }
  }
  return null
}

export async function enableParcelSceneLoading(network: ETHEREUM_NETWORK, options: EnableParcelSceneLoadingOptions) {
  const ret = await initParcelSceneWorker(network)
  const position = Vector2.Zero()

  function setParcelScenes(parcelScenes: ILand[]) {
    const completeListOfParcelsThatShouldBeLoaded = parcelScenes.map($ => $.mappingsResponse.root_cid)

    for (let i = 0; i < parcelScenes.length; i++) {
      const parcelSceneToLoad = parcelScenes[i]

      if (!getParcelById(parcelSceneToLoad.mappingsResponse.root_cid)) {
        const parcelScene = new options.parcelSceneClass(ILandToLoadableParcelScene(parcelSceneToLoad))

        const parcelSceneWorker = new SceneWorker(parcelScene)

        if (parcelSceneWorker) {
          loadedParcelSceneWorkers.add(parcelSceneWorker)
        }
      }
    }

    loadedParcelSceneWorkers.forEach($ => {
      if (!completeListOfParcelsThatShouldBeLoaded.includes($.parcelScene.data.id)) {
        $.dispose()
        loadedParcelSceneWorkers.delete($)
      }
    })
  }

  let initialized = false
  let spawnpointLand = qs.parse(location.search).position

  teleportObservable.add(position => {
    initialized = false
    spawnpointLand = `${position.x},${position.y}`
  })

  teleportObservable.add((position: { x: number; y: number }) => {
    ret.server.notify('User.setPosition', { position })
  })
  positionObservable.add(obj => {
    worldToGrid(obj.position, position)
    ret.server.notify('User.setPosition', { position })
  })

  enablePositionReporting()

  ret.server.on('ParcelScenes.notify', (data: { parcelScenes: ILand[] }) => {
    setParcelScenes(data.parcelScenes.filter(land => options.shouldLoadParcelScene(land)))
    if (!initialized && options.onSpawnpoint) {
      const initialLand = data.parcelScenes.find(land => land.scene.scene.base === spawnpointLand)
      if (initialLand) {
        options.onSpawnpoint(initialLand)
        initialized = true
      }
    }

    if (options.onLoadParcelScenes) {
      options.onLoadParcelScenes(data.parcelScenes)
    }
  })

  return ret
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
