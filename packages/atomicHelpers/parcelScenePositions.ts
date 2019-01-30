import { parcelLimits } from 'config'
import { Vector2Component, Vector3Component, isEqual } from './landHelpers'

let auxVec3: Vector3Component = { x: 0, y: 0, z: 0 }
let auxVec2: Vector2Component = { x: 0, y: 0 }

export interface BoundingInfo {
  maximum: Vector3Component
  minimum: Vector3Component
}

/**
 * Transforms a grid position into a world-relative 3d position
 */
export function gridToWorld(x: number, y: number, target: Vector3Component) {
  target.x = x * parcelLimits.parcelSize
  target.y = 0
  target.z = y * parcelLimits.parcelSize
}

/**
 * Transforms a world position into a grid position
 */
export function worldToGrid(vector: Vector3Component, target: Vector2Component): void {
  target.x = Math.floor(vector.x / parcelLimits.parcelSize)
  target.y = Math.floor(vector.z / parcelLimits.parcelSize)
}

/**
 * Returns true if value is on grid limit
 */
export function isOnLimit(value: number): boolean {
  return Number.isInteger(value / parcelLimits.parcelSize)
}

export function isOnLimits({ maximum, minimum }: BoundingInfo, verificationText: string): boolean {
  // Computes the world-axis-aligned bounding box of an object (including its children),
  // accounting for both the object's, and children's, world transforms

  auxVec3.x = minimum.x
  auxVec3.z = minimum.z
  worldToGrid(auxVec3, auxVec2)
  if (!verificationText.includes(`${auxVec2.x},${auxVec2.y}`)) {
    return false
  }

  auxVec3.x = isOnLimit(maximum.x) ? minimum.x : maximum.x
  auxVec3.z = isOnLimit(maximum.z) ? minimum.z : maximum.z
  worldToGrid(auxVec3, auxVec2)
  if (!verificationText.includes(`${auxVec2.x},${auxVec2.y}`)) {
    return false
  }

  auxVec3.x = minimum.x
  auxVec3.z = isOnLimit(maximum.z) ? minimum.z : maximum.z
  worldToGrid(auxVec3, auxVec2)
  if (!verificationText.includes(`${auxVec2.x},${auxVec2.y}`)) {
    return false
  }

  auxVec3.x = isOnLimit(maximum.x) ? minimum.x : maximum.x
  auxVec3.z = minimum.z
  worldToGrid(auxVec3, auxVec2)
  if (!verificationText.includes(`${auxVec2.x},${auxVec2.y}`)) {
    return false
  }

  return true
}

/**
 * Transforms a world position into a parcel-relative 3d position
 */
export function gridToParcel(base: Vector2Component, x: number, y: number, target: Vector3Component) {
  gridToWorld(base.x, base.y, auxVec3)
  gridToWorld(x, y, target)
  target.x -= auxVec3.x
  target.y -= auxVec3.y
  target.z -= auxVec3.z
}

export function decodeParcelSceneBoundaries(boundaries: string) {
  const [base, ...parcels] = boundaries.split(/\s*;\s*/).map($ => parseParcelPosition($))
  return { base, parcels }
}

export function encodeParcelSceneBoundaries(base: Vector2Component, parcels: Vector2Component[]) {
  let str = `${base.x | 0},${base.y | 0}`

  for (let index = 0; index < parcels.length; index++) {
    const parcel = parcels[index]
    str = str + `;${parcel.x | 0},${parcel.y | 0}`
  }

  return str
}

/**
 * Converts a string position "-1,5" => { x: -1, y: 5 }
 */
export function parseParcelPosition(position: string) {
  const [x, y] = position
    .trim()
    .split(/\s*,\s*/)
    .map($ => parseInt($, 10))
  return { x, y }
}

/**
 * Returns `true` if the given parcels are connected (no separations between them)
 */
export function isValidParcelSceneShape(parcels: Vector2Component[]): boolean {
  return areConnected(parcels) // && !hasHoles(parcels) ?
}

/**
 * Returns true if the given parcels array are connected
 */
export function areConnected(parcels: Vector2Component[]): boolean {
  if (parcels.length === 0) {
    return false
  }
  const visited = visitParcel(parcels[0], parcels)
  return visited.length === parcels.length
}

function visitParcel(
  parcel: Vector2Component,
  allParcels: Vector2Component[] = [parcel],
  visited: Vector2Component[] = []
): Vector2Component[] {
  let isVisited = visited.some(visitedParcel => isEqual(visitedParcel, parcel))
  if (!isVisited) {
    visited.push(parcel)
    let neighbours = getNeighbours(parcel.x, parcel.y, allParcels)
    neighbours.forEach(neighbours => visitParcel(neighbours, allParcels, visited))
  }
  return visited
}

function getIsNeighbourMatcher(x: number, y: number) {
  return (coords: Vector2Component) =>
    (coords.x === x && (coords.y + 1 === y || coords.y - 1 === y)) ||
    (coords.y === y && (coords.x + 1 === x || coords.x - 1 === x))
}

function getNeighbours(x: number, y: number, parcels: Vector2Component[]): Vector2Component[] {
  return parcels.filter(getIsNeighbourMatcher(x, y))
}
