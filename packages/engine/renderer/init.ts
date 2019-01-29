import * as Babylon from 'babylonjs'

import { initMonkeyLoader } from './monkeyLoader'

import 'babylonjs-gui'
import 'babylonjs-materials'
import 'babylonjs-loaders'
import 'babylonjs-procedural-textures'

import { future } from 'fp-future'
import {
  isRunningTest,
  playerConfigurations,
  interactionLimits,
  isStandaloneHeadset,
  DEBUG,
  PREVIEW,
  EDITOR
} from 'config'

export const canvas = document.createElement('canvas')
canvas.setAttribute('id', 'main-canvas')
canvas.style.cssText = 'position:relative;z-index:1000;'
canvas.oncontextmenu = function(e) {
  e.preventDefault()
}

export const engine = new Babylon.Engine(canvas, !isRunningTest, {
  audioEngine: true,
  autoEnableWebVR: true,
  deterministicLockstep: true,
  lockstepMaxSteps: 4,
  alpha: false,
  antialias: !isRunningTest
})

/**
 * This is the main scene of the engine.
 */
export const scene = new Babylon.Scene(engine)
export const vrHelper = scene.createDefaultVRExperience({
  defaultHeight: playerConfigurations.height,
  rayLength: interactionLimits.clickDistance,
  createFallbackVRDeviceOrientationFreeCamera: false,
  createDeviceOrientationCamera: true
})

/**
 * This Future<HTMLCanvasElement> instance is resolved when the
 * engine is ready to render and the canvas is set in the screen
 */
export const domReadyFuture = future<HTMLCanvasElement>()
export const bodyReadyFuture = future<HTMLBodyElement>()

export const audioEngine = BABYLON.Engine.audioEngine

export const effectLayers: BABYLON.EffectLayer[] = []

/// --- SIDE EFFECTS ---

{
  if (document.body) {
    bodyReadyFuture.resolve(document.body as HTMLBodyElement)
  } else {
    document.addEventListener('DOMContentLoaded', () => {
      if (document.body) {
        bodyReadyFuture.resolve(document.body as HTMLBodyElement)
      } else {
        bodyReadyFuture.reject(new Error('no document.body was found'))
      }
    })
  }

  scene.clearColor = BABYLON.Color3.Black().toColor4(1)
  scene.collisionsEnabled = true

  scene.autoClear = false // Color buffer
  scene.autoClearDepthAndStencil = false // Depth and stencil
  scene.setRenderingAutoClearDepthStencil(0, false)
  scene.setRenderingAutoClearDepthStencil(1, true, true, false)

  canvas.style.width = '100%'
  canvas.style.height = '100%'

  scene.gravity = new BABYLON.Vector3(0, playerConfigurations.gravity, 0)
  scene.enablePhysics(scene.gravity, new BABYLON.OimoJSPlugin(2))
  scene.audioEnabled = true
  scene.headphone = true

  vrHelper.enableInteractions()
  vrHelper.enableTeleportation({
    floorMeshName: 'ground'
  })

  const gazeMaterial = vrHelper.gazeTrackerMesh.material

  vrHelper.gazeTrackerMesh = BABYLON.MeshBuilder.CreateSphere('gaze', { segments: 10, diameter: 0.007 }, scene)
  vrHelper.gazeTrackerMesh.material = gazeMaterial

  vrHelper.displayGaze = true
  vrHelper.displayLaserPointer = true

  vrHelper.meshSelectionPredicate = function(mesh) {
    return true
  }

  vrHelper.raySelectionPredicate = function(mesh) {
    return true
  }

  scene.actionManager = new BABYLON.ActionManager(scene)

  engine.enableOfflineSupport = (!DEBUG && !PREVIEW) || EDITOR
  engine.disableManifestCheck = true

  if (!isStandaloneHeadset || isRunningTest) {
    scene.onReadyObservable.add(() => {
      const gl = new BABYLON.GlowLayer('glow', scene)
      scene.addEffectLayer(gl)

      scene.removeEffectLayer = function(layer) {
        if (layer === gl) return
        scene.constructor.prototype.removeEffectLayer.apply(this, arguments)
      } as any

      scene.addEffectLayer = function(layer) {
        if (layer === gl) return
        scene.constructor.prototype.addEffectLayer.apply(this, arguments)
      } as any
    })
  }

  initMonkeyLoader()
}
