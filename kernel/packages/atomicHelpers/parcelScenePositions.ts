import { parcelLimits } from 'config'
import { Vector2Component, Vector3Component, isEqual } from './landHelpers'

let auxVec3: Vector3Component = { x: 0, y: 0, z: 0 }

export interface BoundingInfo {
  maximum: Vector3Component
  minimum: Vector3Component
}

/**
 * Transforms a grid position into a world-relative 3d position
 */
export function gridToWorld(x: number, y: number, target: Vector3Component = { x: 0, y: 0, z: 0 }) {
  target.x = x * parcelLimits.parcelSize
  target.y = 0
  target.z = y * parcelLimits.parcelSize
  return target
}

/**
 * Transforms a world position into a grid position
 */
export function worldToGrid(vector: Vector3Component, target: Vector2Component = { x: 0, y: 0 }): Vector2Component {
  target.x = Math.floor(vector.x / parcelLimits.parcelSize)
  target.y = Math.floor(vector.z / parcelLimits.parcelSize)
  return target
}

const highDelta = parcelLimits.parcelSize + parcelLimits.centimeter
const lowDelta = parcelLimits.centimeter
/**
 * Returns true if a vector is inside a parcel
 */
export function isInParcel(test: Vector3Component, center: Vector3Component): boolean {
  return (
    test.x < center.x + highDelta &&
    test.x > center.x - lowDelta &&
    test.z < center.z + highDelta &&
    test.z > center.z - lowDelta
  )
}

/**
 * Returns true if a world position is inside a group of parcels
 */
export function isWorldPositionInsideParcels(parcels: string[], testWorldPosition: Vector3Component): boolean {
  let isInside = false

  parcels.some((parcel) => {
    const { x, y } = parseParcelPosition(parcel)
    isInside = isInParcel(testWorldPosition, gridToWorld(x, y))
    return isInside
  })

  return isInside
}

export function isOnLimits({ maximum, minimum }: BoundingInfo, parcels: Vector3Component[]): boolean {
  // Computes the world-axis-aligned bounding box of an object (including its children),
  // accounting for both the object's, and children's, world transforms

  let minInside = false
  let maxInside = false

  for (let i = 0; i < parcels.length && (!minInside || !maxInside); i++) {
    maxInside = maxInside || isInParcel(maximum, parcels[i])
    minInside = minInside || isInParcel(minimum, parcels[i])
  }

  // If the max&min points are inside some of the whitelisted areas, it is considered inside the parcel
  return minInside && maxInside
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

/**
 * Converts a position into a string { x: -1, y: 5 } => "-1,5"
 */
export function encodeParcelPosition(base: Vector2Component) {
  return `${base.x | 0},${base.y | 0}`
}
export function encodeParcelSceneBoundaries(base: Vector2Component, parcels: Vector2Component[]) {
  let str = encodeParcelPosition(base)

  for (let index = 0; index < parcels.length; index++) {
    const parcel = parcels[index]
    str = str + `;${encodeParcelPosition(parcel)}`
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
