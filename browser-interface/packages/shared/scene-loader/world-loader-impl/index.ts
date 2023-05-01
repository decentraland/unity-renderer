import { getPortableExperienceFromUrn } from 'unity-interface/portableExperiencesUtils'
import { ISceneLoader } from '../types'

export async function createWorldLoader(options: { urns: string[] }): Promise<ISceneLoader> {
  const scenes = await Promise.all(options.urns.map((urn) => getPortableExperienceFromUrn(urn)))
  return {
    async fetchScenesByLocation(_parcels) {
      return { scenes }
    },
    async reportPosition(_positionReport) {
      return { scenes }
    },
    async stop() {}
  }
}
