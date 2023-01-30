import { LoadableScene } from 'shared/types'
import { RootWearablesPortableExperienceState } from './types'

export const getDesiredWearablePortableExpriences = (store: RootWearablesPortableExperienceState) =>
  store.wearablesPortableExperiences.desiredWearablePortableExperiences

export const getDesiredLoadableWearablePortableExpriences = (
  store: RootWearablesPortableExperienceState
): LoadableScene[] =>
  Object.values(store.wearablesPortableExperiences.desiredWearablePortableExperiences).filter(
    Boolean
  ) as LoadableScene[]
