import { getPortableExperienceFromUrn } from 'unity-interface/portableExperiencesUtils'
import { ISceneLoader } from '../types'
import { encodeParcelPosition } from '../../../lib/decentraland'
import { Vector2 } from '@dcl/ecs-math'
import { getResourcesURL } from '../../location'
import { EmptyParcelController } from '../genesis-city-loader-impl/emptyParcelController'
import { LoadableScene } from '../../types'

function getParcels(position: Vector2, loadingRadius: number): string[] {
  const parcels: string[] = []
  for (let x = position.x - loadingRadius; x < position.x + loadingRadius; x++) {
    for (let y = position.y - loadingRadius; y < position.y + loadingRadius; y++) {
      const v = new Vector2(x, y)
      if (v.subtract(position).length() < loadingRadius) {
        parcels.push(encodeParcelPosition(v))
      }
    }
  }
  return parcels
}

export async function createWorldLoader(options: { urns: string[] }): Promise<ISceneLoader> {
  const mappingScene = new Map<string, LoadableScene>()
  const scenes = await Promise.all(options.urns.map((urn) => getPortableExperienceFromUrn(urn)))

  for (const parcel of scenes[0].entity.metadata.scene.parcels) {
    mappingScene.set(parcel, scenes[0])
  }

  const emptyParcelController = new EmptyParcelController({ rootUrl: getResourcesURL('.') })

  return {
    async fetchScenesByLocation(_parcels) {
      return { scenes }
    },
    async reportPosition(positionReport) {
      const newScenes: Set<LoadableScene> = new Set()
      for await (const parcel of getParcels(
        new Vector2(positionReport.position.x, positionReport.position.y),
        positionReport.loadingRadius
      )) {
        if (mappingScene.has(parcel)) {
          newScenes.add(mappingScene.get(parcel)!)
        } else {
          const scene = await emptyParcelController.createFakeEntity(parcel)

          mappingScene.set(parcel, scene)
          newScenes.add(scene)
        }
      }
      return { scenes: Array.from(newScenes) }
    },
    async stop() {},
    invalidateCache(_) {}
  }
}
