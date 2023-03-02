import { AnyAction } from 'redux'
import { WearablesPortableExperienceState } from './types'
import {
  ADD_DESIRED_PORTABLE_EXPERIENCE,
  AddDesiredPortableExperienceAction,
  REMOVE_DESIRED_PORTABLE_EXPERIENCE,
  RemoveDesiredPortableExperienceAction
} from './actions'

const INITIAL_STATE: WearablesPortableExperienceState = {
  desiredWearablePortableExperiences: {}
}

export function wearablesPortableExperienceReducer(
  state?: WearablesPortableExperienceState,
  action?: AnyAction
): WearablesPortableExperienceState {
  if (!state) {
    return INITIAL_STATE
  }
  if (!action) {
    return state
  }

  switch (action.type) {
    case REMOVE_DESIRED_PORTABLE_EXPERIENCE: {
      const { payload } = action as RemoveDesiredPortableExperienceAction

      const desiredWearablePortableExperiences = { ...state.desiredWearablePortableExperiences }
      delete desiredWearablePortableExperiences[payload.id]
      return { ...state, desiredWearablePortableExperiences }
    }
    case ADD_DESIRED_PORTABLE_EXPERIENCE: {
      const { payload } = action as AddDesiredPortableExperienceAction
      return {
        ...state,
        desiredWearablePortableExperiences: {
          ...state.desiredWearablePortableExperiences,
          [payload.id]: payload.data
        }
      }
    }
  }

  return state
}
