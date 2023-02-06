import { Avatar } from '@dcl/schemas'
import { ProfileAsPromise } from 'shared/profiles/ProfileAsPromise'
import { ProfileType } from 'shared/profiles/types'

export function ensureFriendProfile(userId: string): Promise<Avatar> {
  return ProfileAsPromise(userId, undefined, ProfileType.DEPLOYED) // Friends are always deployed ATM
}
