import { parcelLimits } from 'config'

export interface Vector2Component {
  x: number
  y: number
}

export interface Vector3Component {
  x: number
  y: number
  z: number
}

export type IParcelSceneLimits = {
  triangles: number
  entities: number
  bodies: number
  materials: number
  textures: number
  geometries: number
}

export function getParcelSceneLimits(parcelCount: number): IParcelSceneLimits {
  const log = Math.log2(parcelCount + 1)
  const lineal = parcelCount
  return {
    triangles: Math.floor(lineal * parcelLimits.triangles),
    bodies: Math.floor(lineal * parcelLimits.bodies),
    entities: Math.floor(lineal * parcelLimits.entities),
    materials: Math.floor(log * parcelLimits.materials),
    textures: Math.floor(log * parcelLimits.textures),
    geometries: Math.floor(log * parcelLimits.geometries)
  }
}

/**
 * Returns `true` if the given parcels are one next to each other
 */
export function isAdjacent(p1: Vector2Component, p2: Vector2Component): boolean {
  return (
    (p2.x === p1.x && (p2.y + 1 === p1.y || p2.y - 1 === p1.y)) ||
    (p2.y === p1.y && (p2.x + 1 === p1.x || p2.x - 1 === p1.x))
  )
}

export function isEqual(p1: Vector2Component, p2: Vector2Component): boolean {
  return p1.x === p2.x && p1.y === p2.y
}
