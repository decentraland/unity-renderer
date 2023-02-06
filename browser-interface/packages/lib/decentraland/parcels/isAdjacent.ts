import type { Vector2 } from 'lib/math/Vector2'

/**
 * Returns `true` if the given parcels are one next to each other
 */
export function isAdjacent(p1: Vector2, p2: Vector2): boolean {
  return (
    (p2.x === p1.x && (p2.y + 1 === p1.y || p2.y - 1 === p1.y)) ||
    (p2.y === p1.y && (p2.x + 1 === p1.x || p2.x - 1 === p1.x))
  )
}
