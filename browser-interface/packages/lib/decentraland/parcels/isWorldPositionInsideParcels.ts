import { worldToGrid } from './worldToGrid'
import { encodeParcelPosition } from './encodeParcelPosition'
import type { Vector3 } from 'lib/math/Vector3'

/**
 * Returns true if a world position is inside a group of parcels
 */
export function isWorldPositionInsideParcels(parcels: string[], testWorldPosition: Vector3): boolean {
  const parcel = encodeParcelPosition(worldToGrid(testWorldPosition))
  return parcels.includes(parcel)
}
