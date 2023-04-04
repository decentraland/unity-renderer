import { LoadableScene } from 'shared/types'

export type WearablesPortableExperienceState = {
  desiredWearablePortableExperiences: Record<string, LoadableScene | null>
}

export type RootWearablesPortableExperienceState = {
  wearablesPortableExperiences: WearablesPortableExperienceState
}
