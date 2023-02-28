import { Avatar } from '@dcl/schemas'
import { sendProfileToRenderer } from 'shared/profiles/actions'
import { retrieveProfileFromCatalyst } from 'shared/profiles/retrieveProfile'
import { store } from 'shared/store/isolatedStore'

export async function ensureFriendProfile(userId: string): Promise<Avatar> {
  const profile = await retrieveProfileFromCatalyst(userId, undefined)
  store.dispatch(profileSuccess(profile))
  store.dispatch(sendProfileToRenderer(profile.userId))

  return profile
}
