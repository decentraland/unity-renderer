import { Vector2 } from '@dcl/ecs-math'
import { encodeParcelPosition } from 'lib/decentraland/parcels/encodeParcelPosition'
import { ISceneLoader, SetDesiredScenesCommand } from '../types'
import { SceneDataDownloadManager } from './downloadManager'
import { EmptyParcelController } from './emptyParcelController'

export function createGenesisCityLoader(options: {
  contentServer: string
  emptyParcelsBaseUrl?: string
}): ISceneLoader {
  const emptyParcelController = options.emptyParcelsBaseUrl
    ? new EmptyParcelController({ rootUrl: options.emptyParcelsBaseUrl })
    : undefined

  const downloadManager = new SceneDataDownloadManager({ ...options, emptyParcelController })

  const listeners = new Set<(elem: SetDesiredScenesCommand) => void>()

  const lastPosition: Vector2 = new Vector2()
  let lastLoadingRadius: number = 0

  async function fetchCurrentPosition() {
    const parcels: string[] = []
    for (let x = lastPosition.x - lastLoadingRadius; x < lastPosition.x + lastLoadingRadius; x++) {
      for (let y = lastPosition.y - lastLoadingRadius; y < lastPosition.y + lastLoadingRadius; y++) {
        const v = new Vector2(x, y)
        if (v.subtract(lastPosition).length() < lastLoadingRadius) {
          parcels.push(encodeParcelPosition(v))
        }
      }
    }
    const scenes = await downloadManager.resolveEntitiesByPointer(parcels)

    const message: SetDesiredScenesCommand = {
      scenes: Array.from(scenes)
    }

    return message
  }

  return {
    async fetchScenesByLocation(parcels) {
      const results = await downloadManager.resolveEntitiesByPointer(parcels)
      return {
        scenes: Array.from(results)
      }
    },
    async reportPosition(positionReport) {
      lastPosition.copyFrom(positionReport.position)
      lastLoadingRadius = positionReport.loadingRadius

      return fetchCurrentPosition()
    },
    async stop() {
      listeners.clear()
    }
  }
}
