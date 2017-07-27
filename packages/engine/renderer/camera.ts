import * as BABYLON from 'babylonjs'

import { vrHelper, scene } from './init'
import { playerConfigurations, visualConfigurations } from 'config'

const camera = vrHelper.deviceOrientationCamera

/// --- SIDE EFFECTS ---

{
  camera.ellipsoid = new BABYLON.Vector3(0.3, playerConfigurations.height / 2, 0.3)
  // Activate collisions
  camera.checkCollisions = true
  // Activate gravity !
  camera.applyGravity = true
  camera.position.y = playerConfigurations.height

  camera.speed = playerConfigurations.speed
  camera.inertia = playerConfigurations.inertia
  camera.angularSensibility = playerConfigurations.angularSensibility

  camera.maxZ = visualConfigurations.far * 1.6
  camera.fov = 0.8
  camera.minZ = visualConfigurations.near

  vrHelper.position = camera.position
}

/// --- EXPORTS ---

export { camera }

export function getCameraPosition(ref: BABYLON.Vector3) {
  return ref.copyFrom(scene.activeCamera.position)
}

export function getCameraLookingAt(position: BABYLON.Vector3, output: BABYLON.Vector3) {
  output.copyFrom(position)
  output.addInPlace(
    (scene.activeCamera as BABYLON.FreeCamera).rotation
      .toQuaternion()
      .toEulerAngles()
      .scale(10)
  )
  return output
}
