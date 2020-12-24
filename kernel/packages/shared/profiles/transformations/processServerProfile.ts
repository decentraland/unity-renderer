import { Profile } from '../types'
import { WearableId } from 'shared/catalogs/types'
import { colorString } from './colorString'
import { ALL_WEARABLES } from 'config'
import { filterInvalidNameCharacters } from '../utils/names'
import { createFakeName } from '../utils/fakeName'

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

export function calculateDisplayName(userId: string, profile: any): string {
  if (profile && profile.name && profile.hasClaimedName) {
    return profile.name
  }

  if (profile && profile.unclaimedName) {
    return `${filterInvalidNameCharacters(profile.unclaimedName)}#${userId.slice(-4)}`
  }

  return `${createFakeName()}#${userId.slice(-4)}`
}
export function processServerProfile(userId: string, receivedProfile: any): Profile {
  const name = calculateDisplayName(userId, receivedProfile)
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
    name: name,
    hasClaimedName:
      typeof receivedProfile.hasClaimedName === 'undefined' ? !!receivedProfile.name : receivedProfile.hasClaimedName,
    description: receivedProfile.description || '',
    ethAddress: receivedProfile.ethAddress || 'noeth',
    version: receivedProfile.version ?? receivedProfile.avatar.version ?? 1,
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
    muted: receivedProfile.muted,
    tutorialStep: receivedProfile.tutorialStep || 0,
    interests: receivedProfile.interests || [],
    unclaimedName: receivedProfile.unclaimedName
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
