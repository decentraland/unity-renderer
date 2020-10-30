import { ProfileAsPromise } from 'shared/profiles/ProfileAsPromise'
import { ProfileType } from 'shared/profiles/types'

export function ensureFriendProfile(userId: string) {
  return ProfileAsPromise(userId, undefined, ProfileType.DEPLOYED) // Friends are always deployed ATM
}
