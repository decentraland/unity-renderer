import * as BABYLON from 'babylonjs'

import { vrHelper, scene, engine } from './init'
import { playerConfigurations, visualConfigurations, parcelLimits } from 'config'
import { gridToWorld } from 'atomicHelpers/parcelScenePositions'
import { teleportObservable } from 'shared/world/positionThings'

const vrCamera = vrHelper.deviceOrientationCamera as BABYLON.DeviceOrientationCamera
export const DEFAULT_CAMERA_ZOOM = 32
export const MAX_CAMERA_ZOOM = 100

const arcCamera = new BABYLON.ArcRotateCamera(
  'arc-camera',
  -Math.PI / 4,
  Math.PI / 3,
  DEFAULT_CAMERA_ZOOM,
  new BABYLON.Vector3(5, 0, 5),
  scene,
  true
)

/// --- SIDE EFFECTS ---

{
  vrCamera.ellipsoid = new BABYLON.Vector3(0.3, playerConfigurations.height / 2, 0.3)
  // Activate collisions
  vrCamera.checkCollisions = true
  // Activate gravity !
  vrCamera.applyGravity = true
  vrCamera.position.y = playerConfigurations.height

  vrCamera.speed = playerConfigurations.speed
  vrCamera.inertia = playerConfigurations.inertia
  vrCamera.angularSensibility = playerConfigurations.angularSensibility

  vrCamera.maxZ = visualConfigurations.far * 1.6
  vrCamera.fov = 0.8
  vrCamera.minZ = visualConfigurations.near

  vrHelper.position = vrCamera.position

  arcCamera.upperBetaLimit = Math.PI / 2
  arcCamera.allowUpsideDown = false
  arcCamera.upperRadiusLimit = arcCamera.panningDistanceLimit = MAX_CAMERA_ZOOM
  arcCamera.pinchPrecision = 150
  arcCamera.wheelPrecision = 150
  arcCamera.lowerRadiusLimit = 5

  setCamera(false)
}

/// --- EXPORTS ---

export { vrCamera, arcCamera }

export function setCamera(thirdPerson: boolean) {
  if (thirdPerson && scene.activeCamera === arcCamera) return
  if (!thirdPerson && scene.activeCamera === vrCamera) return

  if (thirdPerson) {
    arcCamera.target.copyFrom(scene.activeCamera!.position)
    scene.switchActiveCamera(arcCamera)
    scene.cameraToUseForPointers = scene.activeCamera
  } else {
    vrCamera.position.copyFrom(scene.activeCamera!.position)
    scene.switchActiveCamera(vrCamera)
    scene.cameraToUseForPointers = scene.activeCamera
  }
}

export function isThirdPersonCamera() {
  return scene.activeCamera === arcCamera
}

export function setCameraPosition(position: BABYLON.Vector3) {
  if (scene.activeCamera === arcCamera) {
    arcCamera.target.copyFrom(position)
  } else {
    scene.activeCamera!.position.copyFrom(position)
  }
}

export function cameraPositionToRef(ref: BABYLON.Vector3) {
  if (scene.activeCamera === arcCamera) {
    ref.copyFrom(arcCamera.target)
  } else {
    ref.copyFrom(scene.activeCamera!.position)
  }
}

export function rayToGround(screenX: number, screenY: number) {
  const mouseVec = new BABYLON.Vector3(screenX, screenY, 0)
  return unprojectToPlane(mouseVec)
}

function unprojectToPlane(vec: BABYLON.Vector3) {
  const viewport = scene.activeCamera!.viewport.toGlobal(engine.getRenderWidth(), engine.getRenderHeight())

  let onPlane = BABYLON.Vector3.Unproject(
    vec,
    viewport.width,
    viewport.height,
    BABYLON.Matrix.Identity(),
    scene.activeCamera!.getViewMatrix(),
    scene.activeCamera!.getProjectionMatrix()
  )

  let dir = onPlane.subtract(scene.activeCamera!.position).normalize()
  let distance = -scene.activeCamera!.position.y / dir.y
  dir.scaleInPlace(distance)
  onPlane = scene.activeCamera!.position.add(dir)
  return onPlane
}

teleportObservable.add((position: { x: number; y: number }) => {
  return teleportTo(position.x, position.y)
})

export async function teleportTo(x: number, y: number) {
  if (
    !(
      parcelLimits.minLandCoordinateX <= x &&
      x <= parcelLimits.maxLandCoordinateX &&
      parcelLimits.minLandCoordinateY <= y &&
      y <= parcelLimits.maxLandCoordinateY
    )
  ) {
    return false
  }
  // Option A: Load scene description, list teleporting point(s), pick one at random if list, calculate world position and set camera
  // ... not implemented!
  // Option B: Set world position :D
  gridToWorld(x, y, vrCamera.position)
}
