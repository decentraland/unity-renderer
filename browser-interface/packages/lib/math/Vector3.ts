export interface Vector3 {
  x: number
  y: number
  z: number
}

export function isEqual(a: Vector3, b: Vector3) {
  return a.x === b.x && a.y === b.y && a.z === b.z
}
