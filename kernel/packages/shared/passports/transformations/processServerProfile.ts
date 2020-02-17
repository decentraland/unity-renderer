import { Profile, WearableId } from '../types'
import { colorString } from './colorString'
import { getServerConfigurations } from 'config'

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
  return function(wearableId: WearableId) {
    return wearableId.startsWith('dcl://base-avatars') || inventory.indexOf(wearableId) !== -1
  }
}
export function processServerProfile(userId: string, receivedProfile: any): Profile {
  const name = receivedProfile.name || 'Guest-' + userId.substr(2, 6)
  const wearables = receivedProfile.avatar.wearables
    .map(fixWearableIds)
    .filter(dropDeprecatedWearables)
    .filter(noExclusiveMismatches(receivedProfile.inventory))
  const snapshots = receivedProfile.snapshots ||
    (receivedProfile.avatar && receivedProfile.avatar.snapshots) || {
      face: getServerConfigurations().avatar.snapshotStorage + userId + `/face.png`,
      body: getServerConfigurations().avatar.snapshotStorage + userId + `/body.png`
    }
  snapshots.face = snapshots.face.replace('|', '%7C')
  snapshots.body = snapshots.body.replace('|', '%7C')
  return {
    userId,
    email: receivedProfile.email || '',
    name: receivedProfile.name || name,
    hasClaimedName: !!receivedProfile.name,
    description: receivedProfile.description || '',
    ethAddress: userId || 'noeth',
    version: receivedProfile.avatar.version || 1,
    avatar: {
      eyeColor: colorString(receivedProfile.avatar.eyes.color),
      hairColor: colorString(receivedProfile.avatar.hair.color),
      skinColor: colorString(receivedProfile.avatar.skin.color),
      bodyShape: fixWearableIds(receivedProfile.avatar.bodyShape),
      wearables,
      snapshots
    },
    inventory: receivedProfile.inventory || [],
    blocked: receivedProfile.blocked,
    tutorialStep: receivedProfile.tutorialStep
  }
}
