import * as Babylon from 'babylonjs'

import 'babylonjs-gui'
import 'babylonjs-materials'
import 'babylonjs-loaders'
import 'babylonjs-procedural-textures'

import { initMonkeyLoader } from './monkeyLoader'
import { Database } from '../database'

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
  antialias: !isRunningTest,
  stencil: true
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

let highlightLayer: BABYLON.HighlightLayer

const gl = new BABYLON.GlowLayer('glow', scene)

export const effectLayers: BABYLON.EffectLayer[] = [gl]

const database: BABYLON.Database = new Database() as any

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

  scene.clearColor = BABYLON.Color3.FromInts(31, 29, 35).toColor4(1)
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

  if (!isRunningTest && !PREVIEW) {
    database.openAsync(
      () => {
        Object.defineProperty(scene, 'database', {
          get: () => database,
          set: () => void 0
        })

        Object.defineProperty(database, 'mustUpdateRessources', {
          get: () => false,
          set: () => void 0
        })

        const initialGetFile = database.loadFileFromDB

        database.loadFileFromDB = function(...args: any[]) {
          const url = args[0]

          if (url.startsWith('data:') || url.startsWith('blob:')) {
            const onNoIDB = args[3]
            if (onNoIDB) {
              onNoIDB()
              return
            }
          }

          return initialGetFile.apply(database, args)
        }

        const initialGetImage = database.loadImageFromDB

        database.loadImageFromDB = function(...args: any[]) {
          return initialGetImage.apply(database, args)
        }
      },
      () => {
        // tslint:disable-next-line:no-console
        console.error('Error opening IDB')
      }
    )
  }

  engine.disableManifestCheck = true

  scene.getBoundingBoxRenderer().showBackLines = false

  if (!isStandaloneHeadset || isRunningTest) {
    scene.onReadyObservable.add(() => {
      effectLayers.forEach($ => scene.effectLayers.includes($) || scene.addEffectLayer($))

      scene.removeEffectLayer = function(layer) {
        if (effectLayers.includes(layer)) return
        scene.constructor.prototype.removeEffectLayer.apply(this, arguments)
      } as any

      scene.addEffectLayer = function(layer) {
        if (effectLayers.includes(layer)) return
        scene.constructor.prototype.addEffectLayer.apply(this, arguments)
      } as any
    })
  }

  initMonkeyLoader()
}

export function getHighlightLayer() {
  if (highlightLayer) {
    return highlightLayer
  }

  highlightLayer = new BABYLON.HighlightLayer('highlight', scene)

  scene.effectLayers.includes(highlightLayer) || scene.addEffectLayer(highlightLayer)

  highlightLayer.innerGlow = false
  highlightLayer.outerGlow = true

  effectLayers.push(highlightLayer)

  return highlightLayer
}
