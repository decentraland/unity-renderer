import { action } from 'typesafe-actions'
import { Profile, ProfileType } from './types'
import { ProfileForRenderer } from '../../decentraland-ecs/src/decentraland/Types'

// Profile fetching

export const PROFILE_REQUEST = '[Request] Profile fetch'
export const PROFILE_SUCCESS = '[Success] Profile fetch'
export const PROFILE_FAILURE = '[Failure] Profile fetch'
export const PROFILE_RANDOM = '[?] Profile randomized'

export const profileRequest = (userId: string, profileType?: ProfileType) =>
  action(PROFILE_REQUEST, { userId, profileType })
export const profileSuccess = (userId: string, profile: Profile, hasConnectedWeb3: boolean = false) =>
  action(PROFILE_SUCCESS, { userId, profile, hasConnectedWeb3 })
export const profileFailure = (userId: string, error: any) => action(PROFILE_FAILURE, { userId, error })
export const profileRandom = (userId: string, profile: Profile) => action(PROFILE_RANDOM, { userId, profile })

export type ProfileRequestAction = ReturnType<typeof profileRequest>
export type ProfileSuccessAction = ReturnType<typeof profileSuccess>
export type ProfileFailureAction = ReturnType<typeof profileFailure>
export type ProfileRandomAction = ReturnType<typeof profileRandom>

// Profile update

export const SAVE_PROFILE_REQUEST = '[Request] Save Profile'
export const SAVE_PROFILE_SUCCESS = '[Success] Save Profile'
export const SAVE_PROFILE_FAILURE = '[Failure] Save Profile'

export const saveProfileRequest = (profile: Partial<Profile>, userId?: string) =>
  action(SAVE_PROFILE_REQUEST, { userId, profile })
export const saveProfileSuccess = (userId: string, version: number, profile: Profile) =>
  action(SAVE_PROFILE_SUCCESS, { userId, version, profile })
export const saveProfileFailure = (userId: string, error: any) => action(SAVE_PROFILE_FAILURE, { userId, error })

export type SaveProfileRequest = ReturnType<typeof saveProfileRequest>
export type SaveProfileSuccess = ReturnType<typeof saveProfileSuccess>
export type SaveProfileFailure = ReturnType<typeof saveProfileFailure>

export const DEPLOY_PROFILE_SUCCESS = '[Success] Deploy Profile'
export const DEPLOY_PROFILE_REQUEST = '[Request] Deploy Profile'
export const DEPLOY_PROFILE_FAILURE = '[Failure] Deploy Profile'
export const deployProfile = (profile: Profile) => action(DEPLOY_PROFILE_REQUEST, { profile })
export const deployProfileSuccess = (userId: string, version: number, profile: Profile) =>
  action(DEPLOY_PROFILE_SUCCESS, { userId, version, profile })
export const deployProfileFailure = (userId: string, profile: Profile, error: any) =>
  action(DEPLOY_PROFILE_FAILURE, { userId, profile, error })

export type DeployProfileSuccess = ReturnType<typeof deployProfileSuccess>
export type DeployProfile = ReturnType<typeof deployProfile>

export const PROFILE_SAVED_NOT_DEPLOYED = 'Profile not deployed'
export const profileSavedNotDeployed = (userId: string, version: number, profile: Profile) =>
  action(PROFILE_SAVED_NOT_DEPLOYED, { userId, version, profile })
export type ProfileSavedNotDeployed = ReturnType<typeof profileSavedNotDeployed>

export const ADDED_PROFILE_TO_CATALOG = '[Success] Added profile to catalog'
export const addedProfileToCatalog = (userId: string, profile: ProfileForRenderer) =>
  action(ADDED_PROFILE_TO_CATALOG, { userId, profile })
export type AddedProfileToCatalog = ReturnType<typeof addedProfileToCatalog>

// Profiles over comms
export const LOCAL_PROFILE_RECEIVED = 'Local Profile Received'
export const localProfileReceived = (userId: string, profile: Profile) =>
  action(LOCAL_PROFILE_RECEIVED, { userId, profile })
export type LocalProfileReceived = ReturnType<typeof localProfileReceived>
