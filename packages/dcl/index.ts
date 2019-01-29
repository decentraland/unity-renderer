import * as BABYLON from 'babylonjs'
import * as TWEEN from '@tweenjs/tween.js'

// register the interfaces that will be used by the entity system
import './api'

// Our imports
import { error } from 'engine/logger'
import { DEBUG, PREVIEW, NETWORK_HZ, EDITOR } from 'config'

import { positionObserver, lastPlayerPosition } from 'shared/world/positionThings'

import { createStats } from './widgets/stats'
import { createCrossHair } from './widgets/fpsCrossHair'
import { Metrics, drawMetrics } from './widgets/metrics'

import { scene, engine, vrHelper, initLocalPlayer, initDCL } from 'engine/renderer'
import { enableVirtualJoystick, initKeyboard, enableMouseLock } from 'engine/renderer/input'
import { reposition } from 'engine/renderer/ambientLights'

import { DebugTelemetry, instrumentTelemetry } from 'atomicHelpers/DebugTelemetry'

import { quaternionToRotationBABYLON } from 'atomicHelpers/math'
import { camera } from 'engine/renderer/camera'

import { isMobile } from 'shared/comms/mobile'

import { toggleColliderHighlight, toggleBoundingBoxes } from 'engine/entities/utils/colliders'
import { initChatSystem, initHudSystem } from './widgets/ui'

import { loadedParcelSceneWorkers } from 'shared/world/parcelSceneManager'
import { WebGLParcelScene } from './WebGLParcelScene'

let isEngineRunning = false

// Draw Parcel metrics (triangles, entities, objects)
const parcelMetrics = drawMetrics(getMetrics())
// Draws FPS / ms
const stats = createStats()

const crossHair = !EDITOR ? createCrossHair(scene) : null

const _render = instrumentTelemetry('render', function() {
  try {
    TWEEN.update()

    if (camera.position.y < -64) {
      camera.position.y = 10
      return
    }

    reposition()

    let memory = performance['memory'] || {}

    DebugTelemetry.collect('renderStats', {
      /*  programs: ((renderer.info.programs as any) as Array<BABYLON.WebGLProgram>).length,
      geometries: renderer.info.memory.geometries,
      calls: renderer.info.render.calls,
      faces: renderer.info.render.faces,
      points: renderer.info.render.points,
      textures: renderer.info.memory.textures, */
      usedHeap: (memory.usedJSHeapSize || 0) / 1048576,
      maxHeap: (memory.jsHeapSizeLimit || 0) / 1048576
    })

    scene.render()
  } catch (e) {
    DebugTelemetry.collect('renderError', { value: 1 }, { message: e.message })
    error(e)
  } finally {
    updateMetrics()
    stats.update()
  }
})

const notifyPositionObservers = (() => {
  const position: BABYLON.Vector3 = BABYLON.Vector3.Zero()
  const rotation = BABYLON.Vector3.Zero()
  const quaternion = BABYLON.Quaternion.Identity()

  const objectToSend = { position, rotation, quaternion }

  return () => {
    if (isEngineRunning) {
      const rotationCamera: BABYLON.TargetCamera = (vrHelper.isInVRMode
        ? vrHelper.currentVRCamera.leftCamera || vrHelper.currentVRCamera
        : scene.activeCamera) as BABYLON.TargetCamera

      position.copyFrom(scene.activeCamera.position)

      if (rotationCamera.rotationQuaternion) {
        quaternion.copyFrom(rotationCamera.rotationQuaternion)
        quaternionToRotationBABYLON(quaternion, rotation)
      }

      positionObserver.notifyObservers(objectToSend)
    }
  }
})()

/// --- SIDE EFFECTS ---

{
  if (crossHair) {
    crossHair.parent = vrHelper.deviceOrientationCamera
  }
  stats.showPanel(1)
  stats.begin()

  stats.dom.style.visibility = 'hidden'

  if (DEBUG || PREVIEW) {
    stats.dom.style.visibility = 'visible'
  }

  if (DEBUG) {
    window['telemetry'] = DebugTelemetry
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
    .map(parcelScene => (parcelScene.parcelScene as WebGLParcelScene).context.metrics)
    .filter(metrics => !!metrics)
    .reduce(
      (metrics, m) => {
        Object.keys(metrics).map(key => {
          metrics[key] += m[key]
        })
        return metrics
      },
      { triangles: 0, bodies: 0, entities: 0, materials: 0, textures: 0 }
    )
}

function updateMetrics(): void {
  const metrics = getMetrics()
  if (!EDITOR) {
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
    enableVirtualJoystick(engine.getRenderingCanvas())
  } else if (!EDITOR) {
    await initChatSystem()
    await initHudSystem()
    initKeyboard()
    initDebugCommands()
    enableMouseLock(engine.getRenderingCanvas())
  }
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
