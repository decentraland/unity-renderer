export interface Vector3Component {
  x: number
  y: number
  z: number
}

export function isEqual(a: Vector3Component, b: Vector3Component) {
  return a.x === b.x && a.y === b.y && a.z === b.z
}
