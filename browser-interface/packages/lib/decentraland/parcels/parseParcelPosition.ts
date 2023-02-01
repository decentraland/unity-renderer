/**
 * Converts a string position "-1,5" => { x: -1, y: 5 }
 */
export function parseParcelPosition(position: string) {
  const [x, y] = position
    .trim()
    .split(/\s*,\s*/)
    .map(($) => parseInt($, 10))
  return { x, y }
}
