import type { Vector2 } from '../../math/Vector2'
import type { Vector3 } from '../../math/Vector3'
import { parcelSize } from './limits'

/**
 * Transforms a world position into a grid position
 */

export function worldToGrid(vector: Vector3, target: Vector2 = { x: 0, y: 0 }): Vector2 {
  target.x = Math.floor(vector.x / parcelSize)
  target.y = Math.floor(vector.z / parcelSize)
  return target
}
