import type { Avatar } from '@dcl/schemas'
import { profileSuccess } from 'shared/profiles/actions'
import { retrieveProfileFromCatalyst } from 'shared/profiles/retrieveProfile'
import { store } from 'shared/store/isolatedStore'

export async function ensureFriendProfile(userId: string): Promise<Avatar> {
  const profile = await retrieveProfileFromCatalyst(userId, undefined)
  store.dispatch(profileSuccess(profile))
  return profile
}
