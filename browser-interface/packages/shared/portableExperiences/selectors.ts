import { LoadableScene } from 'shared/types'
import { getDesiredLoadableWearablePortableExpriences } from 'shared/wearablesPortableExperience/selectors'
import { RootWearablesPortableExperienceState } from 'shared/wearablesPortableExperience/types'
import { RootPortableExperiencesState } from './types'

export const getPortableExperienceDenyList = (store: RootPortableExperiencesState) =>
  store.portableExperiences.deniedPortableExperiencesFromRenderer

export const getPortableExperiencesCreatedByScenes = (store: RootPortableExperiencesState): LoadableScene[] =>
  Object.values(store.portableExperiences.portableExperiencesCreatedByScenesList)

export const getKernelPortableExperiences = (store: RootPortableExperiencesState): LoadableScene[] =>
  Object.values(store.portableExperiences.kernelPortableExperiences)

export const getDesiredPortableExperiences = (
  store: RootPortableExperiencesState & RootWearablesPortableExperienceState
): LoadableScene[] => {
  if (store.portableExperiences.globalPortalExperienceShutDown) return []

  const denylist: string[] = getPortableExperienceDenyList(store)

  const allDesiredPortableExperiences: LoadableScene[] = dedup(
    [
      // ADD HERE ALL THE SOURCES OF DIFFERENT PORTABLE EXPERIENCES TO BE HANDLED BY KERNEL
      // ...getOnboardingPortableExperiences(store),
      // ...getSceneCreatedPortableExperiences(store),
      // ...getManuallyOpenPortableExperiences(store),
      ...getKernelPortableExperiences(store),
      ...getPortableExperiencesCreatedByScenes(store),
      ...getDesiredLoadableWearablePortableExpriences(store)
    ],
    (x) => x.id
  )

  const allFilteredPortableExperiences = allDesiredPortableExperiences.filter(($) => !denylist.includes($.id))

  // remove queryParams from the URN
  return allFilteredPortableExperiences.map(($) => ({
    ...$,
    id: $.id.replace(/(\?.*)/, '')
  }))
}

function dedup<T>(array: T[], filter: (param: T) => any): T[] {
  const map = new Map<any, T>()
  for (const elem of array) {
    const key = filter(elem)
    if (map.has(key)) continue
    map.set(key, elem)
  }
  return Array.from(map.values())
}
