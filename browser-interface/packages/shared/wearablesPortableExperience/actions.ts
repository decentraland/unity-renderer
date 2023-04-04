import { LoadableScene } from 'shared/types'
import { action } from 'typesafe-actions'

export const PROCESS_WEARABLES = '[Process] Wearables'
export const processWearables = (wearable: LoadableScene) => action(PROCESS_WEARABLES, { wearable })
export type ProcessWearablesAction = ReturnType<typeof processWearables>

export const ADD_DESIRED_PORTABLE_EXPERIENCE = '[WearablesPX] Add desired PX'
export const addDesiredPortableExperience = (id: string, data: LoadableScene | null) =>
  action(ADD_DESIRED_PORTABLE_EXPERIENCE, { id, data })
export type AddDesiredPortableExperienceAction = ReturnType<typeof addDesiredPortableExperience>

export const REMOVE_DESIRED_PORTABLE_EXPERIENCE = '[WearablesPX] Remove desired PX'
export const removeDesiredPortableExperience = (id: string) => action(REMOVE_DESIRED_PORTABLE_EXPERIENCE, { id })
export type RemoveDesiredPortableExperienceAction = ReturnType<typeof removeDesiredPortableExperience>
