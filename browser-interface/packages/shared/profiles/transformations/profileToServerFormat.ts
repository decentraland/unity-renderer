import { analizeColorPart, stripAlpha } from './analizeColorPart'
import { isValidBodyShape } from './isValidBodyShape'
import { Avatar, AvatarInfo, Profile } from '@dcl/schemas'
import { validateAvatar } from '../schemaValidation'
import { trackEvent } from 'shared/analytics'
import defaultLogger from 'shared/logger'
import { AvatarForUserData } from 'shared/apis/host/Players'
import { backupProfile } from '../generateRandomUserProfile'

type OldAvatar = Omit<Avatar, 'avatar'> & {
  avatar: AvatarForUserData
}

export function ensureAvatarCompatibilityFormat(profile: Readonly<Avatar | OldAvatar>): Avatar {
  const avatarInfo: AvatarInfo = {} as any

  const avatar: AvatarForUserData | AvatarInfo = profile.avatar || backupProfile(profile.userId).avatar

  // These mappings from legacy id are here just in case they still have the legacy id in local storage
  avatarInfo.bodyShape = mapLegacyIdToUrn(avatar?.bodyShape) || 'urn:decentraland:off-chain:base-avatars:BaseFemale'
  avatarInfo.wearables = (avatar?.wearables || []).map(mapLegacyIdToUrn).filter(Boolean) as string[]
  avatarInfo.emotes = avatar?.emotes
  avatarInfo.snapshots = avatar?.snapshots

  if (avatar && 'eyeColor' in avatar) {
    const eyes = stripAlpha(analizeColorPart(avatar, 'eyeColor', 'eyes'))
    const hair = stripAlpha(analizeColorPart(avatar, 'hairColor', 'hair'))
    const skin = stripAlpha(analizeColorPart(avatar, 'skinColor', 'skin'))
    avatarInfo.eyes = { color: eyes }
    avatarInfo.hair = { color: hair }
    avatarInfo.skin = { color: skin }
  } else {
    avatarInfo.eyes = avatar.eyes
    avatarInfo.hair = avatar.hair
    avatarInfo.skin = avatar.skin
  }

  const invalidWearables =
    !avatarInfo.wearables ||
    !Array.isArray(avatarInfo.wearables) ||
    !avatarInfo.wearables.reduce((prev: boolean, next: any) => prev && typeof next === 'string', true)

  if (invalidWearables) {
    throw new Error('Invalid Wearables array! Received: ' + JSON.stringify(avatarInfo))
  }
  const snapshots = avatarInfo.snapshots as any
  if ('face' in snapshots && !snapshots.face256) {
    snapshots.face256 = snapshots.face!
    delete snapshots['face']
  }
  if (
    !avatarInfo.snapshots ||
    typeof avatarInfo.snapshots.face256 !== 'string' ||
    typeof avatarInfo.snapshots.body !== 'string'
  ) {
    throw new Error('Invalid snapshot data:' + JSON.stringify(avatarInfo.snapshots))
  }
  if (!avatarInfo.bodyShape || !isValidBodyShape(avatarInfo.bodyShape)) {
    throw new Error('Invalid BodyShape! Received: ' + JSON.stringify(avatarInfo))
  }

  const ret: Avatar = {
    ...profile,
    name: profile.name || (profile as any).unclaimedName,
    avatar: avatarInfo
  }

  if (!validateAvatar(ret)) {
    defaultLogger.error('error validating schemas', validateAvatar.errors)
    trackEvent('invalid_schema', { schema: 'avatar', payload: ret, errors: (validateAvatar.errors ?? []).join(',') })
  }

  return ret
}

function mapLegacyIdToUrn(wearableId: string): string | null {
  if (typeof wearableId !== 'string') return null
  if (!wearableId.startsWith('dcl://')) {
    return wearableId
  }
  if (wearableId.startsWith('dcl://base-avatars')) {
    const name = wearableId.substring(wearableId.lastIndexOf('/') + 1)
    return `urn:decentraland:off-chain:base-avatars:${name}`
  } else {
    const [collectionName, wearableName] = wearableId.replace('dcl://', '').split('/')
    return `urn:decentraland:ethereum:collections-v1:${collectionName}:${wearableName}`
  }
}

export function buildServerMetadata(profile: Avatar): Profile {
  const newProfile = ensureAvatarCompatibilityFormat(profile)
  const metadata = { avatars: [newProfile] }
  return metadata
}
