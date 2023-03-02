import type { Vector2 } from 'lib/math/Vector2'

/**
 * Converts a string position "-1,5" => { x: -1, y: 5 }
 */
export function parseParcelPosition(position: string): Vector2 {
  const [x, y] = position
    .trim()
    .split(/\s*,\s*/)
    .map(($) => parseInt($, 10))
  return { x, y }
}
