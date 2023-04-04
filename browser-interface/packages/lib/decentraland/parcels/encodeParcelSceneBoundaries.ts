import type { Vector2 } from 'lib/math/Vector2'
import { encodeParcelPosition } from './encodeParcelPosition'

export function encodeParcelSceneBoundaries(base: Vector2, parcels: Vector2[]) {
  let str = encodeParcelPosition(base)

  for (let index = 0; index < parcels.length; index++) {
    const parcel = parcels[index]
    str = str + `;${encodeParcelPosition(parcel)}`
  }

  return str
}
