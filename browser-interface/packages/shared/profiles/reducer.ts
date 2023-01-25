import { AnyAction } from 'redux'
import { ProfileState } from './types'
import {
  ADDED_PROFILE_TO_CATALOG,
  PROFILE_SUCCESS,
  PROFILE_FAILURE,
  PROFILE_REQUEST,
  ProfileSuccessAction,
  AddedProfilesToCatalog,
  ADDED_PROFILES_TO_CATALOG,
  ProfileFailureAction,
  ADD_PROFILE_TO_LAST_SENT_VERSION_AND_CATALOG,
  AddProfileToLastSentProfileVersionAndCatalog
} from './actions'

const INITIAL_PROFILES: ProfileState = {
  userInfo: {},
  lastSentProfileVersion: new Map()
}

export function profileReducer(state?: ProfileState, action?: AnyAction): ProfileState {
  if (!state) {
    return INITIAL_PROFILES
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case PROFILE_REQUEST:
      return {
        ...state,
        userInfo: {
          ...state.userInfo,
          [action.payload.userId.toLowerCase()]: { ...state.userInfo[action.payload.userId], status: 'loading' }
        }
      }
    case PROFILE_SUCCESS:
      const { profile } = (action as ProfileSuccessAction).payload
      return {
        ...state,
        userInfo: {
          ...state.userInfo,
          [profile.userId.toLowerCase()]: {
            ...state.userInfo[profile.userId.toLowerCase()],
            data: profile,
            status: 'ok'
          }
        }
      }
    case PROFILE_FAILURE:
      const { userId } = (action as ProfileFailureAction).payload

      return {
        ...state,
        userInfo: {
          ...state.userInfo,
          [userId.toLowerCase()]: { status: 'error', data: action.payload.error }
        }
      }
    case ADDED_PROFILE_TO_CATALOG:
      return {
        ...state,
        userInfo: {
          ...state.userInfo,
          [action.payload.userId.toLowerCase()]: {
            ...state.userInfo[action.payload.userId.toLowerCase()],
            addedToCatalog: true
          }
        }
      }
    case ADD_PROFILE_TO_LAST_SENT_VERSION_AND_CATALOG:
      const payload = (action as AddProfileToLastSentProfileVersionAndCatalog).payload
      return {
        ...state,
        userInfo: {
          ...state.userInfo,
          [payload.userId.toLowerCase()]: {
            ...state.userInfo[payload.userId.toLowerCase()],
            addedToCatalog: true
          }
        },
        lastSentProfileVersion: {
          ...state.lastSentProfileVersion,
          [payload.userId.toLowerCase()]: payload.version
        }
      }

    case ADDED_PROFILES_TO_CATALOG:
      const addedProfiles = (action as AddedProfilesToCatalog).payload.profiles
      const updatedProfilesState = {}
      for (const profile of addedProfiles) {
        updatedProfilesState[profile.userId.toLowerCase()] = {
          ...state.userInfo[profile.userId.toLowerCase()],
          addedToCatalog: true
        }
      }

      return {
        ...state,
        userInfo: {
          ...state.userInfo,
          ...updatedProfilesState
        }
      }
    default:
      return state
  }
}
