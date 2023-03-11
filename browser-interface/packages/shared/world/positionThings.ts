import { EcsMathReadOnlyQuaternion, EcsMathReadOnlyVector3, Quaternion, Vector3 } from '@dcl/ecs-math'
import { isInsideWorldLimits, Scene } from '@dcl/schemas'
import { DEBUG, playerHeight } from 'config'
import { gridToWorld } from 'lib/decentraland/parcels/gridToWorld'
import { isWorldPositionInsideParcels } from 'lib/decentraland/parcels/isWorldPositionInsideParcels'
import { parseParcelPosition } from 'lib/decentraland/parcels/parseParcelPosition'
import type { Vector2 } from 'lib/math/Vector2'
import { Observable } from 'mz-observable'
import type { InstancedSpawnPoint } from '../types'

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