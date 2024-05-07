import { AvatarInfo } from '@dcl/schemas'
import { rgbToHex } from 'lib/decentraland/profiles/transformations/convertToRGBObject'
import { AvatarForUserData } from './AvatarForUserData'

export function sceneRuntimeCompatibleAvatar(avatar: AvatarInfo): AvatarForUserData {
  return {
    ...avatar,
    bodyShape: avatar?.bodyShape,
    wearables: avatar?.wearables || [],
    forceRender: avatar?.forceRender || [],
    snapshots: {
      ...avatar.snapshots,
      face: avatar.snapshots.face256,
      face128: avatar.snapshots.face256
    } as any,
    eyeColor: rgbToHex(avatar.eyes.color),
    hairColor: rgbToHex(avatar.hair.color),
    skinColor: rgbToHex(avatar.skin.color)
  }
}
