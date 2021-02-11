import { analizeColorPart, stripAlpha } from './analizeColorPart'
import { isValidBodyShape } from './isValidBodyShape'
import { Profile, Snapshots } from '../types'
import { WearableId } from 'decentraland-ecs/src'

export function ensureServerFormat(profile: Profile): ServerFormatProfile {
  const { avatar } = profile
  const eyes = stripAlpha(analizeColorPart(avatar, 'eyeColor', 'eyes'))
  const hair = stripAlpha(analizeColorPart(avatar, 'hairColor', 'hair'))
  const skin = stripAlpha(analizeColorPart(avatar, 'skin', 'skinColor'))
  const invalidWearables =
    !avatar.wearables ||
    !Array.isArray(avatar.wearables) ||
    !avatar.wearables.reduce((prev: boolean, next: any) => prev && typeof next === 'string', true)
  if (invalidWearables) {
    throw new Error('Invalid Wearables array! Received: ' + JSON.stringify(avatar))
  }
  if (!avatar.snapshots || !avatar.snapshots.face || !avatar.snapshots.body) {
    throw new Error('Invalid snapshot data:' + JSON.stringify(avatar.snapshots))
  }
  if (!avatar.bodyShape || !isValidBodyShape(avatar.bodyShape)) {
    throw new Error('Invalid BodyShape! Received: ' + JSON.stringify(avatar))
  }
  const { userId, hasClaimedName, ...serverFormat } = {
    ...profile,
    avatar: {
      bodyShape: avatar.bodyShape,
      snapshots: avatar.snapshots,
      eyes: { color: eyes },
      hair: { color: hair },
      skin: { color: skin },
      wearables: avatar.wearables
    }
  }
  return serverFormat
}

export function buildServerMetadata(profile: Profile) {
  const newProfile = ensureServerFormat(profile)
  const metadata = { avatars: [newProfile] }
  return metadata
}

type ServerFormatProfile = Omit<Profile, 'inventory' | 'userId' | 'hasClaimedName' | 'avatar'> & {
  avatar: ServerProfileAvatar
}

type Color3 = {
  r: number
  g: number
  b: number
}

type ServerProfileAvatar = {
  bodyShape: WearableId
  eyes: { color: Color3 }
  hair: { color: Color3 }
  skin: { color: Color3 }
  wearables: WearableId[]
  snapshots: Snapshots
}
