export interface Vector2 {
  x: number
  y: number
}

export function isEqual(a: Vector2, b: Vector2) {
  return a.x === b.x && a.y === b.y
}
