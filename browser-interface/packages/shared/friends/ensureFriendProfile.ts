import type { Avatar } from '@dcl/schemas'
import { retrieveProfileFromCatalyst } from 'shared/profiles/retrieveProfile'

export function ensureFriendProfile(userId: string): Promise<Avatar> {
  return retrieveProfileFromCatalyst(userId, undefined)
}
