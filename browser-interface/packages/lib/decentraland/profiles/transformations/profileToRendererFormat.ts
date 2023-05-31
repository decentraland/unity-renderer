import { convertToRGBObject } from './convertToRGBObject'
import { isURL } from 'lib/javascript/isURL'
import { IPFSv2 } from '@dcl/schemas'
import type { Avatar, AvatarInfo, Snapshots } from '@dcl/schemas'
import { generateRandomUserProfile } from '../generateRandomUserProfile'
import { calculateDisplayName } from './processServerProfile'
import type { NewProfileForRenderer } from './types'
import { Color3 } from '@dcl/ecs-math'

export const genericAvatarSnapshots = {
  body: 'QmSav1o6QK37Jj1yhbmhYk9MJc6c2H5DWbWzPVsg9JLYfF',
  face256: 'QmSqZ2npVD4RLdqe17FzGCFcN29RfvmqmEd2FcQUctxaKk'
} as const

export function profileToRendererFormat(
  profile: Partial<Avatar>,
  options: {
    address?: string
    // TODO: when profiles are federated, we must change this to accept the profile's
    //       home server
    baseUrl: string
  }
): NewProfileForRenderer {
  const stage = { ...generateRandomUserProfile(profile.userId || options.address || 'noeth'), ...profile }

  return {
    ...stage,
    userId: stage.userId.toLowerCase(),
    name: calculateDisplayName(stage),
    description: stage.description || '',
    version: stage.version || -1,
    ethAddress: (stage.ethAddress || options.address || '0x0000000000000000000000000000000000000000').toLowerCase(),
    blocked: stage.blocked || [],
    muted: stage.muted || [],
    inventory: [],
    created_at: 0,
    updated_at: 0,
    // @deprecated
    email: '',
    hasConnectedWeb3: stage.hasConnectedWeb3 || false,
    hasClaimedName: stage.hasClaimedName ?? false,
    tutorialFlagsMask: 0,
    tutorialStep: stage.tutorialStep || 0,
    snapshots: prepareSnapshots(profile.avatar!.snapshots),
    baseUrl: options.baseUrl,
    avatar: prepareAvatar(profile.avatar)
  }
}

export function defaultProfile({
  userId,
  name,
  face256
}: {
  userId: string
  name: string
  face256: string
}): NewProfileForRenderer {
  const avatar = {
    userId,
    ethAddress: userId,
    name,
    avatar: {
      snapshots: prepareSnapshots({ face256, body: '' }),
      ...defaultAvatar()
    }
  }
  return profileToRendererFormat(avatar, { baseUrl: '' })
}

function defaultAvatar(): Omit<AvatarInfo, 'snapshots'> {
  return {
    bodyShape: '',
    eyes: { color: Color3.White() },
    hair: { color: Color3.White() },
    skin: { color: Color3.White() },
    wearables: []
  }
}

function prepareAvatar(avatar?: Partial<AvatarInfo>) {
  return {
    wearables: avatar?.wearables || [],
    forceRender: avatar?.forceRender || [],
    emotes: avatar?.emotes || [],
    bodyShape: avatar?.bodyShape || '',
    eyeColor: convertToRGBObject(avatar?.eyes?.color),
    hairColor: convertToRGBObject(avatar?.hair?.color),
    skinColor: convertToRGBObject(avatar?.skin?.color)
  }
}

// Ensure all snapshots are URLs
function prepareSnapshots({ face256, body }: Snapshots): Snapshots {
  // TODO: move this logic to unity-renderer
  function prepare(value: string) {
    if (value === null || value === undefined) {
      return null
    }
    if (
      value === '' ||
      isURL(value) ||
      value.startsWith('/images') ||
      value.startsWith('Qm') ||
      IPFSv2.validate(value)
    ) {
      return value
    }

    return 'data:text/plain;base64,' + value
  }

  const x = prepare(face256)
  return { body: prepare(body) || genericAvatarSnapshots.body, face256: x || genericAvatarSnapshots.face256 }
}
