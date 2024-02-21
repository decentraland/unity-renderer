import { getPortableExperienceFromUrn } from 'unity-interface/portableExperiencesUtils'
import { ISceneLoader } from '../types'
import { LoadableScene } from '../../types'

export async function createWorldLoader(options: { urns: string[] }): Promise<ISceneLoader> {
  const mappingScene = new Map<string, LoadableScene>()
  const scenes = await Promise.all(options.urns.map((urn) => getPortableExperienceFromUrn(urn)))
  for (const scene of scenes) {
    for (const parcel of scene.entity.metadata.scene.parcels) {
      mappingScene.set(parcel, scene)
    }
  }

  return {
    async fetchScenesByLocation(_parcels) {
      return { scenes }
    },
    async reportPosition() {
      return { scenes }
    },
    async stop() {},
    invalidateCache(_) {}
  }
}
