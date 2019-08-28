const qs: any = require('query-string')
import { worldToGrid, gridToWorld, parseParcelPosition } from 'atomicHelpers/parcelScenePositions'
import {
  Vector3,
  ReadOnlyVector3,
  ReadOnlyQuaternion,
  Vector2,
  ReadOnlyVector2
} from 'decentraland-ecs/src/decentraland/math'
import { Observable } from 'decentraland-ecs/src/ecs/Observable'
import { ILand } from 'shared/types'
import { Vector3Component } from '../../atomicHelpers/landHelpers'

declare var location: any
declare var history: any

export type PositionReport = {
  /** Camera position, world space */
  position: ReadOnlyVector3
  /** Camera rotation */
  quaternion: ReadOnlyQuaternion
  /** Camera rotation, euler from quaternion */
  rotation: ReadOnlyVector3
  /** Camera height, relative to the feet of the avatar or ground */
  playerHeight: number
}

export const positionObservable = new Observable<Readonly<PositionReport>>()

export const teleportObservable = new Observable<ReadOnlyVector2>()

export const lastPlayerPosition = new Vector3()

positionObservable.add(event => {
  lastPlayerPosition.copyFrom(event.position)
})

export function initializeUrlPositionObserver() {
  let lastTime: number = performance.now()

  let previousPosition: string | null = null
  const gridPosition = Vector2.Zero()

  function updateUrlPosition(cameraVector: ReadOnlyVector3) {
    // Update position in URI every second
    if (performance.now() - lastTime > 1000) {
      worldToGrid(cameraVector, gridPosition)
      const currentPosition = `${gridPosition.x | 0},${gridPosition.y | 0}`

      if (previousPosition !== currentPosition) {
        const stateObj = { position: currentPosition }
        previousPosition = currentPosition

        const q = qs.parse(location.search)
        q.position = currentPosition

        history.replaceState(stateObj, 'position', `?${qs.stringify(q)}`)
      }

      lastTime = performance.now()
    }
  }

  positionObservable.add(event => {
    updateUrlPosition(event.position)
  })

  if (lastPlayerPosition.equalsToFloats(0, 0, 0)) {
    // LOAD INITIAL POSITION IF SET TO ZERO
    const query = qs.parse(location.search)

    if (query.position) {
      const parcelCoords = query.position.split(',')
      gridToWorld(parseFloat(parcelCoords[0]), parseFloat(parcelCoords[1]), lastPlayerPosition)
    } else {
      lastPlayerPosition.x = Math.round(Math.random() * 10) - 5
      lastPlayerPosition.z = 0
    }
  }
}

/**
 * Computes the spawn point based on a scene.
 *
 * The computation takes the spawning points defined in the scene document and computes the spawning point in the world based on the base parcel position.
 *
 * @param land Scene on which the player is spawning
 */
export function getWorldSpawnpoint(land: ILand): { position: Vector3Component; cameraTarget?: Vector3Component } {
  const spawnpoint = getSpawnpoint(land)

  if (!spawnpoint) {
    return { position: lastPlayerPosition }
  }

  const baseParcel = land.scene.scene.base
  const [bx, by] = baseParcel.split(',')

  const basePosition = new Vector3()

  const { position, cameraTarget } = spawnpoint

  gridToWorld(parseInt(bx, 10), parseInt(by, 10), basePosition)

  return {
    position: basePosition.add(position),
    cameraTarget: cameraTarget ? basePosition.add(cameraTarget) : undefined
  }
}

function getSpawnpoint(land: ILand): { position: Vector3Component; cameraTarget?: Vector3Component } | undefined {
  if (!land.scene || !land.scene.spawnPoints || land.scene.spawnPoints.length === 0) {
    return undefined
  }

  // 1 - default spawn points
  const defaults = land.scene.spawnPoints.filter($ => $.default)

  // 2 - if no default spawn points => all existing spawn points
  const eligiblePoints = defaults.length === 0 ? land.scene.spawnPoints : defaults

  // 3 - pick randomly between spawn points
  const { position, cameraTarget } = eligiblePoints[Math.floor(Math.random() * eligiblePoints.length)]

  // 4 - generate random x, y, z components when in arrays
  return {
    position: {
      x: computeComponentValue(position.x),
      y: computeComponentValue(position.y),
      z: computeComponentValue(position.z)
    },
    cameraTarget
  }
}

function computeComponentValue(x: number | number[]) {
  if (typeof x === 'number') {
    return x
  }
  if (x.length !== 2) {
    throw new Error(`array must have two values ${JSON.stringify(x)}`)
  }
  const [min, max] = x
  if (max <= min) {
    throw new Error(`max value (${max}) must be greater than min value (${min})`)
  }
  return Math.random() * (max - min) + min
}

export function getLandBase(land: ILand): { x: number; y: number } {
  if (
    land.scene &&
    land.scene.scene &&
    land.scene.scene.base &&
    typeof (land.scene.scene.base as string | void) === 'string'
  ) {
    return parseParcelPosition(land.scene.scene.base)
  } else {
    return parseParcelPosition(land.mappingsResponse.parcel_id)
  }
}
