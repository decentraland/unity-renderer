import * as BABYLON from 'babylonjs'
import * as TWEEN from '@tweenjs/tween.js'

// register the interfaces that will be used by the entity system
import 'shared/apis'

// Our imports
import { DEBUG, PREVIEW, NETWORK_HZ, EDITOR, playerConfigurations } from 'config'

import { positionObservable, lastPlayerPosition, PositionReport } from 'shared/world/positionThings'

import { createStats } from './widgets/stats'
import { Metrics, drawMetrics } from './widgets/metrics'

import { scene, engine, vrHelper, initLocalPlayer, initDCL } from 'engine/renderer'
import { enableVirtualJoystick, initKeyboard, enableMouseLock } from 'engine/renderer/input'
import { reposition } from 'engine/renderer/ambientLights'

import { quaternionToRotationBABYLON } from 'atomicHelpers/math'
import { vrCamera } from 'engine/renderer/camera'

import { isMobile } from 'shared/comms/mobile'

import { toggleColliderHighlight, toggleBoundingBoxes } from 'engine/entities/utils/colliders'
import { initHudSystem } from './widgets/ui'

import { loadedParcelSceneWorkers } from 'shared/world/parcelSceneManager'
import { WebGLParcelScene } from './WebGLParcelScene'
import { IParcelSceneLimits } from 'atomicHelpers/landHelpers'

let isEngineRunning = false

// Draw Parcel metrics (triangles, entities, objects)
const parcelMetrics = drawMetrics(getMetrics())
// Draws FPS / ms
const stats = createStats()

let lastMetrics = 0

function _render() {
  TWEEN.update()

  if (vrCamera.position.y < -64) {
    vrCamera.position.y = 10
    return
  }

  reposition()

  scene.render()

  if (lastMetrics++ > 10) {
    lastMetrics = 0
    updateMetrics()
  }

  stats.update()
}

const notifyPositionObservers = (() => {
  const position: BABYLON.Vector3 = BABYLON.Vector3.Zero()
  const rotation = BABYLON.Vector3.Zero()
  const quaternion = BABYLON.Quaternion.Identity()

  const objectToSend: PositionReport = {
    position,
    rotation,
    quaternion,
    playerHeight: playerConfigurations.height
  }

  return () => {
    if (isEngineRunning) {
      const rotationCamera: BABYLON.TargetCamera = (vrHelper.isInVRMode
        ? vrHelper.currentVRCamera!.leftCamera || vrHelper.currentVRCamera
        : scene.activeCamera) as BABYLON.TargetCamera

      position.copyFrom(scene.activeCamera!.position)

      if (rotationCamera.rotationQuaternion) {
        quaternion.copyFrom(rotationCamera.rotationQuaternion)
        quaternionToRotationBABYLON(quaternion, rotation)
      }

      positionObservable.notifyObservers(objectToSend)
    }
  }
})()

/// --- SIDE EFFECTS ---

{
  stats.showPanel(1)
  stats.begin()

  stats.dom.style.visibility = 'hidden'

  if (DEBUG || PREVIEW) {
    stats.dom.style.visibility = 'visible'
  }

  const networkTime = 1000 / NETWORK_HZ
  let now = performance.now()
  let nextNetworkTime = now + networkTime

  scene.onAfterRenderObservable.add(function() {
    now = performance.now()

    if (nextNetworkTime < now) {
      nextNetworkTime = now + networkTime
      notifyPositionObservers()
    }
  })
}

function getMetrics(): Metrics {
  return [...loadedParcelSceneWorkers]
    .filter(parcelScene => (parcelScene.parcelScene as WebGLParcelScene).context)
    .map(parcelScene => {
      const context = (parcelScene.parcelScene as WebGLParcelScene).context
      context.updateMetrics()
      return context.metrics
    })
    .filter(metrics => !!metrics)
    .reduce<IParcelSceneLimits>(
      (metrics, m) => {
        Object.keys(metrics).map(key => {
          // tslint:disable-next-line:semicolon
          ;(metrics as any)[key] += (m as any)[key]
        })
        return metrics
      },
      { triangles: 0, bodies: 0, entities: 0, materials: 0, textures: 0, geometries: 0 }
    )
}

function updateMetrics(): void {
  const metrics = getMetrics()
  if (!EDITOR && parcelMetrics) {
    parcelMetrics.update(metrics)
  }
}

/// --- EXPORTS ---

export function start() {
  if (isEngineRunning) return
  isEngineRunning = true
  engine.runRenderLoop(_render)
}

export function stop() {
  isEngineRunning = false
  engine.stopRenderLoop(_render)
}

export function addStats() {
  if (!EDITOR) {
    document.body.appendChild(stats.dom)
  }
}

export async function initBabylonClient() {
  initDCL()

  initLocalPlayer(lastPlayerPosition)

  if (isMobile()) {
    enableVirtualJoystick(engine.getRenderingCanvas()!)
  } else if (!EDITOR) {
    await initHudSystem()
  }
  enableMouseLock(engine.getRenderingCanvas()!)

  initKeyboard()
  initDebugCommands()

  addStats()
  start()
}

function initDebugCommands() {
  if (DEBUG || PREVIEW || EDITOR || document.location.hostname === 'localhost') {
    {
      // toggle colliders

      let colliderVisible = false
      let bboxVisible = false

      scene.actionManager.registerAction(
        new BABYLON.ExecuteCodeAction(BABYLON.ActionManager.OnKeyDownTrigger, evt => {
          if (evt.sourceEvent.key === 'c') {
            colliderVisible = !colliderVisible
            loadedParcelSceneWorkers.forEach($ =>
              toggleColliderHighlight(colliderVisible, ($.parcelScene as WebGLParcelScene).context.rootEntity)
            )
          } else if (evt.sourceEvent.key === 'b') {
            bboxVisible = !bboxVisible
            loadedParcelSceneWorkers.forEach($ =>
              toggleBoundingBoxes(bboxVisible, ($.parcelScene as WebGLParcelScene).context.rootEntity)
            )
          }
        })
      )
    }
  }
}
