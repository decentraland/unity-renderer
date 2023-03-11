import { Vector3 } from '@dcl/ecs-math'
import { Scene } from '@dcl/schemas'
import { DEBUG } from 'config'
import { gridToWorld } from 'lib/decentraland/parcels/gridToWorld'
import { isWorldPositionInsideParcels } from 'lib/decentraland/parcels/isWorldPositionInsideParcels'
import { InstancedSpawnPoint } from '../types'

/**
 * Computes the spawn point based on a scene.
 *
 * The computation takes the spawning points defined in the scene document and computes the spawning point in the world based on the base parcel position.
 *
 * @param land Scene on which the player is spawning
 */
export function pickWorldSpawnpoint(land: Scene): InstancedSpawnPoint {
  const spawnpoint = pickSpawnpoint(land)

  const baseParcel = land.scene.base
  const [bx, by] = baseParcel.split(',')

  const basePosition = new Vector3()

  const { position, cameraTarget } = spawnpoint

  gridToWorld(parseInt(bx, 10), parseInt(by, 10), basePosition)

  return {
    position: basePosition.add(position),
    cameraTarget: cameraTarget ? basePosition.add(cameraTarget) : undefined
  }
}

const defaultSpawnPoints = [
  {
    position: {
      x: 1,
      y: 0,
      z: 1
    }
  }
]

function pickSpawnpoint(land: Scene): InstancedSpawnPoint {
  let spawnPoints = land.spawnPoints
  if (!Array.isArray(spawnPoints) || spawnPoints.length === 0) {
    spawnPoints = defaultSpawnPoints
  }
  if (!spawnPoints) {
    throw new Error(`Invalid spawnpoint definition`)
  }

  // 1 - Use the pool of all the default spawn points
  const defaults = spawnPoints.filter(($) => $.default)

  // 2 - If no default spawn points are defined, set the pool to be all the spawn points
  const eligiblePoints = defaults.length === 0 ? spawnPoints : defaults

  // 3 - Pick one at random between the eligible spawn points
  const { position, cameraTarget } = eligiblePoints[Math.floor(Math.random() * eligiblePoints.length)]

  // 4 - Resolve ranges into specific coordinates
  const finalPosition = resolveRanges(position)

  // 5 - If the final position is outside the scene limits, zero it
  const finalWorldPosition = checkBoundaries(land, finalPosition)
  return {
    position: finalWorldPosition,
    cameraTarget
  }
}

function checkBoundaries(land: Scene, finalPosition: ReadOnlyVector3): ReadOnlyVector3 {
  if (DEBUG) {
    return finalPosition
  }
  const sceneBaseParcelCoords = land.scene.base.split(',')
  const sceneBaseParcelWorldPos = gridToWorld(
    parseInt(sceneBaseParcelCoords[0], 10),
    parseInt(sceneBaseParcelCoords[1], 10)
  )
  const finalWorldPosition = {
    x: sceneBaseParcelWorldPos.x + finalPosition.x,
    y: finalPosition.y,
    z: sceneBaseParcelWorldPos.z + finalPosition.z
  }

  if (!isWorldPositionInsideParcels(land.scene.parcels, finalWorldPosition)) {
    finalWorldPosition.x = 1
    finalWorldPosition.z = 1
  }
  return finalWorldPosition
}

function resolveRanges(v: { x: number | number[]; y: number | number[]; z: number | number[] }): ReadOnlyVector3 {
  return {
    x: computeComponentValue(v.x),
    y: computeComponentValue(v.y),
    z: computeComponentValue(v.z)
  }
}

function computeComponentValue(x: number | number[]) {
  if (typeof x === 'number') {
    return x
  }

  const length = x.length
  if (length === 0) {
    return 0
  } else if (length < 2) {
    return x[0]
  } else if (length > 2) {
    x = [x[0], x[1]]
  }

  let [min, max] = x

  if (min === max) return max

  if (min > max) {
    const aux = min
    min = max
    max = aux
  }

  return Math.random() * (max - min) + min
}
