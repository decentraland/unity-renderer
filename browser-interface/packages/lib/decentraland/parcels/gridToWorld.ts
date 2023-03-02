import type { Vector3 } from 'lib/math/Vector3'
import { parcelSize } from './limits'

/**
 * Transforms a grid position into a world-relative 3d position
 */
export function gridToWorld(x: number, y: number, target: Vector3 = { x: 0, y: 0, z: 0 }) {
  target.x = x * parcelSize
  target.y = 0
  target.z = y * parcelSize
  return target
}
