import type { Avatar } from '@dcl/schemas'
import { store } from 'shared/store/isolatedStore'
import { sagaMiddleware } from 'shared/store/store'
import { profileRequest } from './actions'
import { fetchCatalystProfile } from './sagas/content'
import { fetchProfile } from './sagas/fetchProfile'
import { getProfile } from './selectors'

/**
 * Retrieves a Profile with a version equal or higher than the provided one (if any)
 * - If the `userId` is the current user, look in local storage
 * - Otherwise, tries to fetch it from the in-memory redux store registry of profiles
 * - Next, tries to fetch it from comms using an ad-hoc "RPC"
 *   * if userId is a connected peer, send a request to fetch the profile from them
 * - Finally, look for a deployed profile on the catalyst
 */
export async function retrieveProfile(userId: string, minimumVersion?: number): Promise<Avatar> {
  const action = profileRequest(userId, minimumVersion)
  const profileResult = sagaMiddleware.run(fetchProfile, action)
  store.dispatch(action)
  return profileResult.toPromise() as any
}

/**
 * Forces fetch of a profile over catalyst
 */
export async function retrieveProfileFromCatalyst(userId: string, minimumVersion?: number): Promise<Avatar> {
  return sagaMiddleware.run(fetchCatalystProfile, userId, minimumVersion).toPromise()
}

export function getProfileIfExists(userId: string): Avatar | null {
  return getProfile(store.getState(), userId)
}
