import type { Vector2 } from 'lib/math/Vector2'

/**
 * Converts a position into a string { x: -1, y: 5 } => "-1,5"
 */
export function encodeParcelPosition(base: Vector2) {
  return `${base.x | 0},${base.y | 0}`
}
