import { PortableExperiencesState } from './types'
import {
  ACTIVATE_ALL_PORTABLE_EXPERIENCES,
  ADD_KERNEL_PX,
  ADD_SCENE_PX,
  DENY_PORTABLE_EXPERIENCES,
  PortableExperienceActions,
  RELOAD_SCENE_PX,
  REMOVE_SCENE_PX,
  SHUTDOWN_ALL_PORTABLE_EXPERIENCES
} from './actions'
import { REMOVE_DESIRED_PORTABLE_EXPERIENCE, RemoveDesiredPortableExperienceAction } from '../wearablesPortableExperience/actions'

const INITIAL_STATE: PortableExperiencesState = {
  deniedPortableExperiencesFromRenderer: [],
  portableExperiencesCreatedByScenesList: {},
  kernelPortableExperiences: {},
  globalPortalExperienceShutDown: false
}

export function portableExperienceReducer(
  state?: PortableExperiencesState,
  action?: PortableExperienceActions | RemoveDesiredPortableExperienceAction
): PortableExperiencesState {
  if (!state) {
    return INITIAL_STATE
  }

  if (!action) {
    return state
  }

  switch (action.type) {
    case REMOVE_DESIRED_PORTABLE_EXPERIENCE: {
      return {
        ...state,
        deniedPortableExperiencesFromRenderer: state.deniedPortableExperiencesFromRenderer.filter(
          ($) => $ !== action.payload.id
        )
      }
    }
    case SHUTDOWN_ALL_PORTABLE_EXPERIENCES: {
      return { ...state, globalPortalExperienceShutDown: true }
    }
    case ACTIVATE_ALL_PORTABLE_EXPERIENCES: {
      return { ...state, globalPortalExperienceShutDown: false }
    }
    case DENY_PORTABLE_EXPERIENCES: {
      const { payload } = action
      return { ...state, deniedPortableExperiencesFromRenderer: payload.urnList }
    }
    case ADD_KERNEL_PX: {
      const { payload } = action
      return {
        ...state,
        kernelPortableExperiences: {
          ...state.kernelPortableExperiences,
          [payload.data.id]: payload.data
        }
      }
    }
    case ADD_SCENE_PX: {
      const { payload } = action
      return {
        ...state,
        portableExperiencesCreatedByScenesList: {
          ...state.portableExperiencesCreatedByScenesList,
          [payload.data.id]: payload.data
        }
      }
    }
    case RELOAD_SCENE_PX: {
      const { payload } = action
      return {
        ...state,
        portableExperiencesCreatedByScenesList: {
          ...state.portableExperiencesCreatedByScenesList,
          [payload.data.id]: payload.data
        }
      }
    }
    case REMOVE_SCENE_PX: {
      const { payload } = action
      const newState: PortableExperiencesState = {
        ...state,
        portableExperiencesCreatedByScenesList: { ...state.portableExperiencesCreatedByScenesList },
        kernelPortableExperiences: { ...state.kernelPortableExperiences }
      }
      delete newState.portableExperiencesCreatedByScenesList[payload.urn]
      delete newState.kernelPortableExperiences[payload.urn]
      return newState
    }
  }

  return state
}
