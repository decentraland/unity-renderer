import * as BABYLON from 'babylonjs'

import { playerConfigurations, DEBUG } from 'config'
import { scene, engine } from './init'
import Joystick from './controls/joystick'
import PlaneCanvasControl from './controls/planeCanvasControl'
import { isThirdPersonCamera, vrCamera, arcCamera } from './camera'
import { loadedSceneWorkers } from 'shared/world/parcelSceneManager'
import { WebGLScene } from '../dcl/WebGLScene'
import { hud } from '../dcl/widgets/ui'

/**
 * This is a map of keys (see enum Keys): boolean
 */
const keyState: {
  [keyCode: number]: boolean
  [keyName: string]: boolean
} = {}

enum Keys {
  KEY_W = 87,
  KEY_A = 65,
  KEY_F = 70,
  KEY_S = 83,
  KEY_D = 68,

  KEY_LEFT = 37,
  KEY_UP = 38,
  KEY_RIGHT = 39,
  KEY_DOWN = 40,

  KEY_SHIFT = -1,
  KEY_CTRL = -2,
  KEY_SPACE = 32,

  KEY_E = 69,
  KEY_Q = 81
}

/// --- EXPORTS ---

export { keyState, Keys }

let didInit = false

export function initKeyboard() {
  if (didInit) return
  didInit = true
  vrCamera.keysUp = [Keys.KEY_W as number] // W
  vrCamera.keysDown = [Keys.KEY_S as number] // S
  vrCamera.keysLeft = [Keys.KEY_A as number] // A
  vrCamera.keysRight = [Keys.KEY_D as number] // D

  document.body.addEventListener('keydown', e => {
    if (e.code === 'Enter' && hud) {
      hud.context.entities.forEach((e: any) => e.dispatchUUIDEvent('onEnter', {}))
    }

    keyState[Keys.KEY_SHIFT] = e.shiftKey

    if (e.shiftKey && vrCamera.applyGravity) {
      vrCamera.speed = playerConfigurations.runningSpeed
    }

    if (e.key === 'f') {
      vrCamera.applyGravity = !vrCamera.applyGravity
    }

    keyState[Keys.KEY_CTRL] = e.ctrlKey
    keyState[e.keyCode] = true
  })

  document.body.addEventListener('keyup', e => {
    if (!e.shiftKey) {
      vrCamera.speed = playerConfigurations.speed
    }

    keyState[Keys.KEY_SHIFT] = e.shiftKey
    keyState[Keys.KEY_CTRL] = e.ctrlKey
    keyState[e.keyCode] = false
  })

  const CAMERA_SPEED = 0.02
  const CAMERA_LEFT = BABYLON.Quaternion.RotationYawPitchRoll(Math.PI / 2, 0, 0)
  const CAMERA_RIGHT = BABYLON.Quaternion.RotationYawPitchRoll(-Math.PI / 2, 0, 0)
  const CAMERA_FORWARD = BABYLON.Quaternion.RotationYawPitchRoll(Math.PI, 0, 0)
  const CAMERA_BACKWARD = BABYLON.Quaternion.RotationYawPitchRoll(0, 0, 0)

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
}

function moveCamera(camera: BABYLON.ArcRotateCamera, directionRotation: BABYLON.Quaternion, speed: number) {
  const direction = camera.position.subtract(camera.target)
  direction.y = 0
  direction.normalize()

  applyQuaternion(direction, directionRotation)
  return direction.scaleInPlace(speed)
}

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

export function enableVirtualJoystick(sceneCanvas: HTMLCanvasElement) {
  // Change camera inputs to one rotation only
  vrCamera.inputs.remove(vrCamera.inputs.attached.deviceOrientation)
  vrCamera.inputs.remove(vrCamera.inputs.attached.mouse)
  vrCamera.inputs.remove(vrCamera.inputs.attached.keyboard)

  sceneCanvas.setAttribute('touch-action', 'none')

  enableMovementJoystick()
  enableRotationCanvas(sceneCanvas)
}

function enableMovementJoystick() {
  const joystickSize = 150
  const joystickSensibility = 0.025

  const container = document.createElement('div')
  container.setAttribute('id', 'joystick-container')
  container.style.cssText = 'padding-left:25px;padding-bottom:50px;position:fixed;bottom:0;left:0;z-index:1200;'

  const joystick = new Joystick({ debug: DEBUG, joystickSize })
  const canvas = joystick.getCanvas()

  joystick.onMove(e => {
    const cameraTransform = BABYLON.Matrix.RotationYawPitchRoll(vrCamera.rotation.y, vrCamera.rotation.x, 0)

    const deltaTransform = BABYLON.Vector3.TransformCoordinates(
      new BABYLON.Vector3((e.deltaX * joystickSensibility) / 10, 0, ((e.deltaY * joystickSensibility) / 10) * -1),
      cameraTransform
    )
    vrCamera.cameraDirection = vrCamera.cameraDirection.add(deltaTransform)
  })

  container.appendChild(canvas)
  document.body.appendChild(container)
  joystick.setup()
}

function enableRotationCanvas(sceneCanvas: HTMLCanvasElement) {
  sceneCanvas.style.cssText += ';touch-action:none'

  const rotationSensibility = 0.00025

  const container = document.createElement('div')
  container.setAttribute('id', 'rotation-control-container')
  container.style.cssText = `position:fixed;top:0;left:0;z-index:1100`

  const planeCanvas = new PlaneCanvasControl()
  const canvas = planeCanvas.getCanvas()

  // Rotate on plane canvas move
  planeCanvas.onMove(e => {
    const deltaTransform = new BABYLON.Vector3(e.deltaY * rotationSensibility, e.deltaX * rotationSensibility, 0)
    vrCamera.cameraRotation = vrCamera.cameraRotation.addVector3(deltaTransform)
  })

  // Enable interactions
  planeCanvas.onTouch(e => {
    e.preventDefault()
    sceneCanvas.focus()

    const { x, y } = e.center
    console['log'](`clicking ${x}, ${y}`)
    interactWithScene('pointerDown', x, y, 1)
  })

  container.appendChild(canvas)
  document.body.appendChild(container)
  planeCanvas.setup()

  resizeRotationCanvas(sceneCanvas)
}

function findParentEntity<T extends BABYLON.Node & { isDCLEntity?: boolean }>(
  node: T
): {
  handleClick(pointerEvent: 'pointerUp' | 'pointerDown', pointerId: number, pickingResult: BABYLON.PickingInfo): void
} | null {
  // Find the next entity parent to dispatch the event
  let parent: BABYLON.Node | null = node.parent

  while (parent && !('isDCLEntity' in parent)) {
    parent = parent.parent

    // If the element has no parent, stop execution
    if (!parent) return null
  }

  return (parent as any) || null
}

export function interactWithScene(pointerEvent: 'pointerUp' | 'pointerDown', x: number, y: number, pointerId: number) {
  const pickingResult = scene.pick(x, y, void 0, false)

  const mesh = pickingResult!.pickedMesh

  const entity = mesh && findParentEntity(mesh)

  if (entity) {
    entity.handleClick(pointerEvent, pointerId, pickingResult!)
  } else {
    for (let [, scene] of loadedSceneWorkers) {
      if (scene.parcelScene instanceof WebGLScene) {
        scene.parcelScene.context.sendPointerEvent(pointerEvent, pointerId, null as any, pickingResult!)
      }
    }
  }
}

export function resizeRotationCanvas(sceneCanvas: HTMLCanvasElement) {
  const canvas = document.getElementById('rotation-control-canvas') as HTMLCanvasElement
  if (canvas) {
    canvas.width = sceneCanvas.width
    canvas.height = sceneCanvas.height
  }
}

export function enableMouseLock(canvas: HTMLCanvasElement) {
  const hasPointerLock = () =>
    document.pointerLockElement === canvas ||
    document['mozPointerLockElement'] === canvas ||
    document['webkitPointerLockElement'] === canvas

  if (!canvas.requestPointerLock) {
    canvas.requestPointerLock = canvas.requestPointerLock || canvas['mozRequestPointerLock']
  }

  scene.onPointerObservable.add(e => {
    if (e.type === BABYLON.PointerEventTypes.POINTERDOWN) {
      const evt = e.event as PointerEvent

      if (hud) {
        hud.context.entities.forEach((e: any) => e.dispatchUUIDEvent('onPointerLock', {}))
      }

      if (isThirdPersonCamera()) {
        canvas.focus()
        interactWithScene('pointerDown', evt.offsetX, evt.offsetY, evt.pointerId)
      } else {
        if (hasPointerLock()) {
          canvas.focus()
          interactWithScene('pointerDown', canvas.width / 2, canvas.height / 2, evt.pointerId)
        } else {
          canvas.requestPointerLock()
          canvas.focus()
        }
      }
    } else if (e.type === BABYLON.PointerEventTypes.POINTERUP) {
      const evt = e.event as PointerEvent

      if (isThirdPersonCamera()) {
        interactWithScene('pointerUp', evt.offsetX, evt.offsetY, evt.pointerId)
      } else if (hasPointerLock()) {
        interactWithScene('pointerUp', canvas.width / 2, canvas.height / 2, evt.pointerId)
      }
    }
  })
}
