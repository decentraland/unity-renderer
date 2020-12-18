import { Profile } from '../types'
import { ParcelsWithAccess, ProfileForRenderer } from 'decentraland-ecs/src'
import { convertToRGBObject } from './convertToRGBObject'
import { dropDeprecatedWearables } from './processServerProfile'
import { ExplorerIdentity } from 'shared/session/types'
import { isURL } from 'atomicHelpers/isURL'

const profileDefaults = {
  tutorialStep: 0
}

export function profileToRendererFormat(
  profile: Profile,
  options?: { identity?: ExplorerIdentity; parcels?: ParcelsWithAccess }
): ProfileForRenderer {
  const { snapshots, ...rendererAvatar } = profile.avatar
  return {
    ...profileDefaults,
    ...profile,
    snapshots: prepareSnapshots(snapshots ?? profile.snapshots),
    hasConnectedWeb3: options?.identity ? options.identity.hasConnectedWeb3 : false,
    avatar: {
      ...rendererAvatar,
      wearables: profile.avatar.wearables.filter(dropDeprecatedWearables),
      eyeColor: convertToRGBObject(profile.avatar.eyeColor),
      hairColor: convertToRGBObject(profile.avatar.hairColor),
      skinColor: convertToRGBObject(profile.avatar.skinColor)
    },
    parcelsWithAccess: options?.parcels
  }
}

// Ensure all snapshots are URLs
function prepareSnapshots({
  face,
  face256,
  face128,
  body
}: {
  face: string
  face256: string
  face128: string
  body: string
}): {
  face: string
  face256: string
  face128: string
  body: string
} {
  function prepare(value: string) {
    if (value === '' || isURL(value) || value.startsWith('/images')) return value
    else return 'data:text/plain;base64,' + value
  }
  return { face: prepare(face), face128: prepare(face128), face256: prepare(face256), body: prepare(body) }
}
