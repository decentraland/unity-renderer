import { analizeColorPart, stripAlpha } from './analizeColorPart'
import { isValidBodyShape } from './isValidBodyShape'
export function ensureServerFormat(avatar: any, currentVersion: number) {
  const eyes = stripAlpha(analizeColorPart(avatar, 'eyeColor', 'eyes'))
  const hair = stripAlpha(analizeColorPart(avatar, 'hairColor', 'hair'))
  const skin = stripAlpha(analizeColorPart(avatar, 'skin', 'skinColor'))
  if (
    !avatar.wearables ||
    !Array.isArray(avatar.wearables) ||
    !avatar.wearables.reduce(
      (prev: boolean, next: any) => prev && typeof next === 'string' && next.startsWith('dcl://'),
      true
    )
  ) {
    throw new Error('Invalid Wearables array! Received: ' + JSON.stringify(avatar))
  }
  if (
    !avatar.snapshots ||
    !avatar.snapshots.face ||
    !avatar.snapshots.body ||
    !avatar.snapshots.face.startsWith('https') ||
    !avatar.snapshots.body.startsWith('https')
  ) {
    throw new Error('Invalid snapshot data:' + JSON.stringify(avatar.snapshots))
  }
  if (!avatar.bodyShape || !isValidBodyShape(avatar.bodyShape)) {
    throw new Error('Invalid BodyShape! Received: ' + JSON.stringify(avatar))
  }
  return {
    bodyShape: avatar.bodyShape,
    snapshots: avatar.snapshots,
    eyes: { color: eyes },
    hair: { color: hair },
    skin: { color: skin },
    wearables: avatar.wearables,
    version: currentVersion + 1
  }
}
