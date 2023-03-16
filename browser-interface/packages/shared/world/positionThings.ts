import { Vector3, EcsMathReadOnlyVector3, EcsMathReadOnlyQuaternion, Quaternion } from '@dcl/ecs-math'
import { Observable } from 'mz-observable'
import { InstancedSpawnPoint } from '../types'
import { parseParcelPosition } from 'lib/decentraland/parcels/parseParcelPosition'
import { isWorldPositionInsideParcels } from 'lib/decentraland/parcels/isWorldPositionInsideParcels'
import { gridToWorld } from 'lib/decentraland/parcels/gridToWorld'
import { DEBUG, playerHeight } from 'config'
import { isInsideWorldLimits, Scene, SpawnPoint } from '@dcl/schemas'
import { parcelSize } from 'lib/decentraland/parcels/limits'

export type PositionReport = {
  /** Camera position, world space */
  position: EcsMathReadOnlyVector3
  /** Avatar rotation */
  quaternion: EcsMathReadOnlyQuaternion
  /** Avatar rotation, euler from quaternion */
  rotation: EcsMathReadOnlyVector3
  /** Camera height, relative to the feet of the avatar or ground */
  playerHeight: number
  /** Should this position be applied immediately */
  immediate: boolean
  /** Camera rotation */
  cameraQuaternion: EcsMathReadOnlyQuaternion
  /** Camera rotation, euler from quaternion */
  cameraEuler: EcsMathReadOnlyVector3
}

export const positionObservable = new Observable<Readonly<PositionReport>>()

export const lastPlayerPosition = new Vector3()
export let lastPlayerPositionReport: Readonly<PositionReport> | null = null

positionObservable.add((event) => {
  lastPlayerPosition.copyFrom(event.position)
  lastPlayerPositionReport = event
})

const positionEvent = {
  position: Vector3.Zero(),
  quaternion: Quaternion.Identity,
  rotation: Vector3.Zero(),
  playerHeight: playerHeight,
  mousePosition: Vector3.Zero(),
  immediate: false, // By default the renderer lerps avatars position
  cameraQuaternion: Quaternion.Identity,
  cameraEuler: Vector3.Zero()
}

export function receivePositionReport(
  position: ReadOnlyVector3,
  rotation?: ReadOnlyVector4,
  cameraRotation?: ReadOnlyVector4,
  playerHeight?: number
) {
  positionEvent.position.set(position.x, position.y, position.z)

  if (rotation) positionEvent.quaternion.set(rotation.x, rotation.y, rotation.z, rotation.w)
  positionEvent.rotation.copyFrom(positionEvent.quaternion.eulerAngles)
  if (playerHeight) positionEvent.playerHeight = playerHeight

  const cameraQuaternion = cameraRotation ?? rotation
  if (cameraQuaternion)
    positionEvent.cameraQuaternion.set(cameraQuaternion.x, cameraQuaternion.y, cameraQuaternion.z, cameraQuaternion.w)
  positionEvent.cameraEuler.copyFrom(positionEvent.cameraQuaternion.eulerAngles)

  positionObservable.notifyObservers(positionEvent)
}

// sets the initial state of the position based on the URL query params
export function getInitialPositionFromUrl(): ReadOnlyVector2 | undefined {
  // LOAD INITIAL POSITION IF SET TO ZERO
  const query = new URLSearchParams(location.search)
  const position = query.get('position')
  if (typeof position === 'string') {
    const { x, y } = parseParcelPosition(position)
    if (isInsideWorldLimits(x, y)) return { x, y }
  }
}

/**
 * Computes the spawn point based on a scene.
 *
 * The computation takes the spawning points defined in the scene document and computes the spawning point in the world based on the base parcel position.
 *
 * @param land Scene on which the player is spawning
 * @param loadPosition Parcel position on which the player is teleporting to
 */
export function pickWorldSpawnpoint(land: Scene, loadPosition: Vector3): InstancedSpawnPoint {
  
  const baseParcel = land.scene.base
  const [bx, by] = baseParcel.split(',')
  const basePosition = new Vector3()
  gridToWorld(parseInt(bx, 10), parseInt(by, 10), basePosition)

  const spawnpoint = pickSpawnpoint(land, loadPosition, basePosition)
  const { position, cameraTarget } = spawnpoint

  return {
    position: basePosition.add(position),
    cameraTarget: cameraTarget ? basePosition.add(cameraTarget) : undefined
  }
}

function pickSpawnpoint(land: Scene, targetParcelPosition: Vector3, basePosition: Vector3): InstancedSpawnPoint {
  let spawnPoints = land.spawnPoints
  if (!Array.isArray(spawnPoints) || spawnPoints.length === 0) {
    spawnPoints = [
      {
        position: {
          x: 1,
          y: 0,
          z: 1
        }
      }
    ]
  }
  if (!spawnPoints) {
    throw new Error(`Invalid spawnpoint definition`)
  }

  // 1 - default spawn points
  const defaults = spawnPoints.filter(($) => $.default)

  // 2 - if no default spawn points => all existing spawn points
  const eligiblePoints = defaults.length === 0 ? spawnPoints : defaults

  // 3 - get the closest spawn point
  const targetWorldPosition = new Vector3(targetParcelPosition.x * parcelSize, 0, targetParcelPosition.z * parcelSize)
  let closestIndex = 0
  let closestDist = Number.MAX_SAFE_INTEGER

  console.log("[TEST] Getting spawn points near " + targetWorldPosition.toString())
  // we compare world positions from the target parcel and the spawn points
  const spawnDistances = eligiblePoints.map( (value: SpawnPoint, index: number , array: SpawnPoint[]) => { 
    const pos = getSpawnPointWorldPosition(value, basePosition)
    const dist = Vector3.DistanceSquared(targetWorldPosition, pos) 
    console.log("[TEST] spawn point at " + pos + " with distance " + dist)
    return dist
  } )

  for(let i = 0; i < spawnDistances.length; i++)
  {
    if (spawnDistances[i] < closestDist)
    {
        closestDist = spawnDistances[i]
        closestIndex = i
    }
  }

  const { position, cameraTarget } = eligiblePoints[closestIndex]

  // 4 - generate random x, y, z components when in arrays
  const finalPosition = {
    x: computeComponentValue(position.x),
    y: computeComponentValue(position.y),
    z: computeComponentValue(position.z)
  }

  // 5 - If the final position is outside the scene limits, we zero it
  if (!DEBUG) {
    const finalWorldPosition = {
      x: basePosition.x + finalPosition.x,
      y: finalPosition.y,
      z: basePosition.z + finalPosition.z
    }

    console.log("[TEST] selected spawn point at index " + closestIndex + " with position " + finalPosition)

    if (!isWorldPositionInsideParcels(land.scene.parcels, finalWorldPosition)) {
      finalPosition.x = 1
      finalPosition.z = 1
      console.log("[TEST] spawn position out of bounds has been reset to 1,1");
    }
  }

  return {
    position: finalPosition,
    cameraTarget
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

function getSpawnPointWorldPosition(spawnPoint: SpawnPoint, basePosition: Vector3)
{
  const x = spawnPoint.position.x;
  const y = spawnPoint.position.y;
  const z = spawnPoint.position.z;

  if (typeof x === "number" && typeof y === "number" && typeof z === "number")
  {
    return new Vector3(x, y ,z).add(basePosition); 
  } else {
    return new Vector3(x[0], y[0], z[0]).add(basePosition); 
  }
}
