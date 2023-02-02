import { getProfile } from './selectors'
import { profileRequest } from './actions'
import { ProfileType } from './types'
import { store } from 'shared/store/isolatedStore'
import { Avatar } from '@dcl/schemas'
import future from 'fp-future'

// This method creates a promise that makes sure that a profile was downloaded AND added to renderer's catalog
export async function ProfileAsPromise(userId: string, version?: number, profileType?: ProfileType): Promise<Avatar> {
  const fut = future<Avatar>()
  store.dispatch(profileRequest(userId, fut, profileType, version))
  return fut
}

export function getProfileIfExist(userId: string): Avatar | null {
  return getProfile(store.getState(), userId)
}
