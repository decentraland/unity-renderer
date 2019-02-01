import * as BABYLON from 'babylonjs'

import { vrHelper, scene, engine } from './init'
import { playerConfigurations, visualConfigurations } from 'config'
import { keyState, Keys } from './input'

export const DEFAULT_CAMERA_ZOOM = 20
const CAMERA_SPEED = 0.01
const CAMERA_LEFT = BABYLON.Quaternion.RotationYawPitchRoll(Math.PI / 2, 0, 0)
const CAMERA_RIGHT = BABYLON.Quaternion.RotationYawPitchRoll(-Math.PI / 2, 0, 0)
const CAMERA_FORWARD = BABYLON.Quaternion.RotationYawPitchRoll(Math.PI, 0, 0)
const CAMERA_BACKWARD = BABYLON.Quaternion.RotationYawPitchRoll(0, 0, 0)

const vrCamera = vrHelper.deviceOrientationCamera

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

  arcCamera.keysDown = []
  arcCamera.keysUp = []
  arcCamera.keysLeft = []
  arcCamera.keysRight = []

  arcCamera.onAfterCheckInputsObservable.add(() => {
    if (arcCamera === scene.activeCamera) {
      if (keyState[Keys.KEY_LEFT]) {
        arcCamera.target.addInPlace(moveCamera(arcCamera, CAMERA_LEFT, CAMERA_SPEED * engine.getDeltaTime()))
      }

      if (keyState[Keys.KEY_RIGHT]) {
        arcCamera.target.addInPlace(moveCamera(arcCamera, CAMERA_RIGHT, CAMERA_SPEED * engine.getDeltaTime()))
      }

      if (keyState[Keys.KEY_UP]) {
        arcCamera.target.addInPlace(moveCamera(arcCamera, CAMERA_FORWARD, CAMERA_SPEED * engine.getDeltaTime()))
      }

      if (keyState[Keys.KEY_DOWN]) {
        arcCamera.target.addInPlace(moveCamera(arcCamera, CAMERA_BACKWARD, CAMERA_SPEED * engine.getDeltaTime()))
      }
    }
  })

  arcCamera.upperBetaLimit = Math.PI / 2
  arcCamera.allowUpsideDown = false
  arcCamera.panningDistanceLimit = 20
  arcCamera.pinchPrecision = 150
  arcCamera.wheelPrecision = 150
  arcCamera.lowerRadiusLimit = 5
}

/// --- EXPORTS ---

function applyQuaternion(v: BABYLON.Vector3, q: BABYLON.Quaternion) {
  let x = v.x
  let y = v.y
  let z = v.z
  let qx = q.x
  let qy = q.y
  let qz = q.z
  let qw = q.w

  // calculate quat * vector

  let ix = qw * x + qy * z - qz * y
  let iy = qw * y + qz * x - qx * z
  let iz = qw * z + qx * y - qy * x
  let iw = -qx * x - qy * y - qz * z

  // calculate result * inverse quat

  v.x = ix * qw + iw * -qx + iy * -qz - iz * -qy
  v.y = iy * qw + iw * -qy + iz * -qx - ix * -qz
  v.z = iz * qw + iw * -qz + ix * -qy - iy * -qx

  return v
}

function moveCamera(camera: any, directionRotation: BABYLON.Quaternion, speed: number) {
  const direction = camera.position.subtract(camera.target).normalize()
  direction.y = 0
  applyQuaternion(direction, directionRotation)
  return direction.scaleInPlace(speed)
}

export { vrCamera, arcCamera }

export function setCamera(thirdPerson: boolean) {
  if (thirdPerson && scene.activeCamera === arcCamera) return
  if (!thirdPerson && scene.activeCamera === vrCamera) return

  const canvas = engine.getRenderingCanvas()

  if (thirdPerson) {
    vrCamera.detachControl(canvas)
    arcCamera.attachControl(canvas, true)

    arcCamera.target.copyFrom(scene.activeCamera.position)

    scene.activeCamera = arcCamera
    scene.cameraToUseForPointers = scene.activeCamera
  } else {
    vrCamera.attachControl(canvas)
    arcCamera.detachControl(canvas)

    vrCamera.position.copyFrom(scene.activeCamera.position)

    scene.activeCamera = vrCamera
    scene.cameraToUseForPointers = scene.activeCamera
  }
}

export function isThirdPersonCamera() {
  return scene.activeCamera === arcCamera
}
