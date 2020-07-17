import { Profile, WearableId } from '../types'
import { colorString } from './colorString'
import { ALL_WEARABLES } from 'config'

export function fixWearableIds(wearableId: string) {
  return wearableId.replace('/male_body', '/BaseMale').replace('/female_body', '/BaseFemale')
}
export const deprecatedWearables = [
  'dcl://base-avatars/male_body',
  'dcl://base-avatars/female_body',
  'dcl://base-avatars/BaseMale',
  'dcl://base-avatars/BaseFemale',
  'dcl://base-avatars/00_EmptyEarring',
  'dcl://base-avatars/00_EmptyFacialHair',
  'dcl://base-avatars/00_bald'
]
export function dropDeprecatedWearables(wearableId: string): boolean {
  return deprecatedWearables.indexOf(wearableId) === -1
}
export function noExclusiveMismatches(inventory: WearableId[]) {
  return function (wearableId: WearableId) {
    if (ALL_WEARABLES) {
      return true
    }
    return wearableId.startsWith('dcl://base-avatars') || inventory.indexOf(wearableId) !== -1
  }
}
export function processServerProfile(userId: string, receivedProfile: any): Profile {
  const name = receivedProfile.name || 'Guest-' + userId.substr(2, 6)
  const wearables = receivedProfile.avatar.wearables
    .map(fixWearableIds)
    .filter(dropDeprecatedWearables)
    .filter(noExclusiveMismatches(receivedProfile.inventory))
  const snapshots = receivedProfile.avatar ? receivedProfile.avatar.snapshots : {}
  const eyeColor = flattenColorIfNecessary(receivedProfile.avatar.eyes.color)
  const hairColor = flattenColorIfNecessary(receivedProfile.avatar.hair.color)
  const skinColor = flattenColorIfNecessary(receivedProfile.avatar.skin.color)
  return {
    userId,
    email: receivedProfile.email || '',
    name: receivedProfile.name || name,
    hasClaimedName: !!receivedProfile.name,
    description: receivedProfile.description || '',
    ethAddress: userId || 'noeth',
    version: receivedProfile.avatar.version || 1,
    avatar: {
      eyeColor: colorString(eyeColor),
      hairColor: colorString(hairColor),
      skinColor: colorString(skinColor),
      bodyShape: fixWearableIds(receivedProfile.avatar.bodyShape),
      wearables,
      snapshots
    },
    inventory: receivedProfile.inventory || [],
    blocked: receivedProfile.blocked,
    tutorialStep: receivedProfile.tutorialStep || 0
  }
}

/**
 * Flattens the object with a color field to avoid having two nested color fields when profile comess messed from server.
 *
 * @param objectWithColor object to flatten if need be
 */
function flattenColorIfNecessary(objectWithColor: any) {
  return objectWithColor.color ? objectWithColor.color : objectWithColor
}
