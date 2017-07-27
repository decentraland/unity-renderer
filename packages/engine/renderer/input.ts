import * as BABYLON from 'babylonjs'

import { camera } from '.'
import { playerConfigurations, DEBUG } from 'config'
import { vrHelper, scene } from './init'
import Joystick from './controls/joystick'
import PlaneCanvasControl from './controls/planeCanvasControl'
import { isMobile } from 'shared/comms/mobile'

/**
 * This is a map of keys (see enum Keys): boolean
 */
const keyState: {
  [keyCode: number]: boolean
  [keyName: string]: boolean
} = {}

const PointerLock = {
  isLocked: false,
  preventLocking: false
}

enum Keys {
  KEY_W = 87,
  KEY_A = 65,
  KEY_F = 70,
  KEY_S = 83,
  KEY_D = 68,
  KEY_SHIFT = -1,
  KEY_CTRL = -2,
  KEY_SPACE = 32,
  KEY_E = 69,
  KEY_Q = 81
}

/// --- EXPORTS ---

export { keyState, PointerLock, Keys }

export function blockPointerLock() {
  PointerLock.isLocked = false
  PointerLock.preventLocking = true
}

export function initKeyboard() {
  camera.keysUp = [Keys.KEY_W as number] // Z
  camera.keysDown = [Keys.KEY_S as number] // S
  camera.keysLeft = [Keys.KEY_A as number] // Q
  camera.keysRight = [Keys.KEY_D as number] // D

  if (vrHelper.currentVRCamera) {
    const camera = vrHelper.currentVRCamera as BABYLON.DeviceOrientationCamera
    camera.keysUp = [Keys.KEY_W as number] // Z
    camera.keysDown = [Keys.KEY_S as number] // S
    camera.keysLeft = [Keys.KEY_A as number] // Q
    camera.keysRight = [Keys.KEY_D as number] // D
  }

  document.body.addEventListener('keydown', e => {
    if (document.activeElement && document.activeElement.nodeName === 'INPUT') {
      // CHAT
      return
    }

    keyState[Keys.KEY_SHIFT] = e.shiftKey

    if (e.shiftKey && camera.applyGravity) {
      camera.speed = playerConfigurations.runningSpeed
    }

    if (e.key === 'f') {
      camera.applyGravity = !camera.applyGravity
    }

    keyState[Keys.KEY_CTRL] = e.ctrlKey
    keyState[e.keyCode] = true
  })

  document.body.addEventListener('keyup', e => {
    if (!e.shiftKey) {
      camera.speed = playerConfigurations.speed
    }

    keyState[Keys.KEY_SHIFT] = e.shiftKey
    keyState[Keys.KEY_CTRL] = e.ctrlKey
    keyState[e.keyCode] = false
  })
}

export function enableVirtualJoystick(sceneCanvas: HTMLCanvasElement) {
  // Change camera inputs to one rotation only
  camera.inputs.remove(camera.inputs.attached.deviceOrientation)
  camera.inputs.remove(camera.inputs.attached.mouse)
  camera.inputs.remove(camera.inputs.attached.keyboard)

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
    const cameraTransform = BABYLON.Matrix.RotationYawPitchRoll(camera.rotation.y, camera.rotation.x, 0)
    const deltaTransform = BABYLON.Vector3.TransformCoordinates(
      new BABYLON.Vector3((e.deltaX * joystickSensibility) / 10, 0, ((e.deltaY * joystickSensibility) / 10) * -1),
      cameraTransform
    )
    camera.cameraDirection = camera.cameraDirection.add(deltaTransform)
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
    camera.cameraRotation = camera.cameraRotation.addVector3(deltaTransform)
  })

  // Enable interactions
  planeCanvas.onTouch(e => {
    e.preventDefault()
    sceneCanvas.focus()

    const { x, y } = e.center
    console['log'](`clicking ${x}, ${y}`)
    interactWithScene(e, x, y)
  })

  container.appendChild(canvas)
  document.body.appendChild(container)
  planeCanvas.setup()

  resizeRotationCanvas(sceneCanvas)
}

function interactWithScene(e, x, y) {
  const pickingResult = scene.pick(x, y)
  if (!pickingResult || !pickingResult.pickedMesh) {
    return
  }

  if (isMobile()) {
    const actionManager = pickingResult.pickedMesh.actionManager
    if (!actionManager) {
      return
    }

    const action = new BABYLON.ActionEvent(pickingResult.pickedMesh, x, y, pickingResult.pickedMesh, e)
    actionManager.processTrigger(BABYLON.ActionManager.OnPickDownTrigger, action)
  } else {
    const eventData: PointerEventInit = {
      clientX: x,
      clientY: y,
      screenX: x,
      screenY: y,
      detail: e.detail,
      button: e.button,
      buttons: e.buttons,
      pointerId: e.pointerId,
      pointerType: e.pointerType
    }

    scene.cameraToUseForPointers = scene.activeCamera
    scene.simulatePointerDown(pickingResult, eventData)
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

  canvas.requestPointerLock = canvas.requestPointerLock || canvas['mozRequestPointerLock']

  window.addEventListener('pointerdown', evt => {
    if (hasPointerLock() && !vrHelper.isInVRMode) {
      evt.preventDefault()
      evt.stopPropagation()
      canvas.focus()
      interactWithScene(evt, canvas.width / 2, canvas.height / 2)
    }
  })

  window.addEventListener('pointerup', evt => {
    if (hasPointerLock() && !vrHelper.isInVRMode) {
      evt.preventDefault()
      evt.stopPropagation()
      canvas.focus()

      const pickingResult = scene.pick(canvas.width / 2, canvas.height / 2)

      if (pickingResult && pickingResult.pickedMesh) {
        const eventData: PointerEventInit = {
          clientX: canvas.width / 2,
          clientY: canvas.height / 2,
          screenX: canvas.width / 2,
          screenY: canvas.height / 2,
          detail: evt.detail,
          button: evt.button,
          buttons: evt.buttons,
          pointerId: (evt as any).pointerId,
          pointerType: (evt as any).pointerType
        }
        scene.cameraToUseForPointers = scene.activeCamera
        scene.simulatePointerUp(pickingResult, eventData)
      }
    }
  })

  canvas.addEventListener('click', () => {
    const preventPointerLock = PointerLock.preventLocking

    if (!preventPointerLock && !hasPointerLock()) {
      canvas.requestPointerLock()
      canvas.focus()
    }

    // Disable locking prevention again (as it was set initially)
    PointerLock.preventLocking = false
  })

  scene.onPrePointerObservable.add(evt => {
    if (evt.type === BABYLON.PointerEventTypes.POINTERDOWN || evt.type === BABYLON.PointerEventTypes.POINTERUP) {
      // regular mouse events doesn't have the `ray` property
      evt.skipOnPointerObservable = !evt.ray
    }
  })

  document.addEventListener('pointerlockchange', function() {
    const isPointerLocked = hasPointerLock()
    PointerLock.isLocked = isPointerLocked

    if (!isPointerLocked) {
      camera.detachControl(canvas)
    } else {
      camera.attachControl(canvas)
    }
  })
}
