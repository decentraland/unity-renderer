import { Avatar } from '@dcl/schemas'
import { IFuture } from 'fp-future'
import { action } from 'typesafe-actions'
import { ProfileType } from './types'

// Profile fetching

export const PROFILE_REQUEST = '[PROFILE] Fetch request'
export const PROFILE_SUCCESS = '[PROFILE] Fetch succeeded'
export const PROFILE_FAILURE = '[PROFILE] Fetch failed'

export const SAVE_PROFILE = 'SAVE_PROFILE'
export const SAVE_PROFILE_FAILURE = 'SAVE_PROFILE_FAILURE'

export const DEPLOY_PROFILE_SUCCESS = 'DEPLOY_PROFILE_SUCCESS'
export const DEPLOY_PROFILE_REQUEST = 'DEPLOY_PROFILE_REQUEST'
export const DEPLOY_PROFILE_FAILURE = 'DEPLOY_PROFILE_FAILURE'

export const SEND_PROFILE_TO_RENDERER = 'SEND_PROFILE_TO_RENDERER'

export const profileRequest = (userId: string, future: IFuture<Avatar>, profileType?: ProfileType, version?: number) =>
  action(PROFILE_REQUEST, { userId, future, profileType, version })

/**
 * profileSuccess stores locally a profile and sends it to the renderer.
 * It can be the result of either a profileRequest or local profile loading/editing
 */
export const profileSuccess = (profile: Avatar) => action(PROFILE_SUCCESS, { profile })
export const profileFailure = (userId: string, error: any) => action(PROFILE_FAILURE, { userId, error })

export type ProfileRequestAction = ReturnType<typeof profileRequest>
export type ProfileSuccessAction = ReturnType<typeof profileSuccess>
export type ProfileFailureAction = ReturnType<typeof profileFailure>

// Profile update

export const saveProfileDelta = (profile: Partial<Avatar>) => action(SAVE_PROFILE, { profile })
export const sendProfileToRenderer = (userId: string) => action(SEND_PROFILE_TO_RENDERER, { userId })
export const saveProfileFailure = (userId: string, error: any) => action(SAVE_PROFILE_FAILURE, { userId, error })

export type SaveProfileDelta = ReturnType<typeof saveProfileDelta>
export type SendProfileToRenderer = ReturnType<typeof sendProfileToRenderer>
export type SaveProfileFailure = ReturnType<typeof saveProfileFailure>

export const deployProfile = (profile: Avatar) => action(DEPLOY_PROFILE_REQUEST, { profile })
export const deployProfileSuccess = (userId: string, version: number, profile: Avatar) =>
  action(DEPLOY_PROFILE_SUCCESS, { userId, version, profile })
export const deployProfileFailure = (userId: string, profile: Avatar, error: any) =>
  action(DEPLOY_PROFILE_FAILURE, { userId, profile, error })

export type DeployProfileSuccess = ReturnType<typeof deployProfileSuccess>
export type DeployProfile = ReturnType<typeof deployProfile>

export const ADDED_PROFILE_TO_CATALOG = '[Success] Added profile to catalog'
export const addedProfileToCatalog = (userId: string, profile: Avatar) =>
  action(ADDED_PROFILE_TO_CATALOG, { userId, profile })
export type AddedProfileToCatalog = ReturnType<typeof addedProfileToCatalog>

export const ADDED_PROFILES_TO_CATALOG = '[Success] Added profiles to catalog'
export const addedProfilesToCatalog = (profiles: Avatar[]) => action(ADDED_PROFILES_TO_CATALOG, { profiles })
export type AddedProfilesToCatalog = ReturnType<typeof addedProfilesToCatalog>

// Profiles over comms
export const PROFILE_RECEIVED_OVER_COMMS = 'PROFILE_RECEIVED_OVER_COMMS'
export const profileReceivedOverComms = (profile: Avatar) => action(PROFILE_RECEIVED_OVER_COMMS, { profile })
export type ProfileReceivedOverComms = ReturnType<typeof profileReceivedOverComms>
