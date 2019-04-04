declare var global: any
declare var window: any

global['isEditor'] = window['isEditor'] = true

import 'engine'

import { initLocalPlayer, domReadyFuture, onWindowResize } from '../engine/renderer'

import { initBabylonClient } from '../engine/dcl'
import * as _envHelper from '../engine/renderer/envHelper'
import { canvas, scene } from '../engine/renderer/init'
import { loadedParcelSceneWorkers, enablePositionReporting } from '../shared/world/parcelSceneManager'
import {
  LoadableParcelScene,
  ILandToLoadableParcelScene,
  ILand,
  IScene,
  EnvironmentData,
  ContentMapping,
  normalizeContentMappings
} from '../shared/types'
import { SceneWorker } from '../shared/world/SceneWorker'
import { WebGLParcelScene } from '../engine/dcl/WebGLParcelScene'
import { EventEmitter } from 'events'
import { SharedSceneContext } from '../engine/entities/SharedSceneContext'
import {
  vrCamera,
  arcCamera,
  DEFAULT_CAMERA_ZOOM,
  setCameraPosition as _setCameraPosition,
  cameraPositionToRef,
  rayToGround
} from '../engine/renderer/camera'
import { setEditorEnvironment } from '../engine/renderer/ambientLights'
import { sleep } from '../atomicHelpers/sleep'
import * as Gizmos from '../engine/components/ephemeralComponents/Gizmos'
import { Gizmo } from '../decentraland-ecs/src/decentraland/Gizmos'
import { Vector3 } from 'babylonjs'
import future, { IFuture } from 'fp-future'
import { BaseEntity } from '../engine/entities/BaseEntity'

let didStartPosition = false

const evtEmitter = new EventEmitter()

let webGlParcelScene: WebGLParcelScene | null = null
let parcelsX = 1
let parcelsY = 1

async function loadScene(scene: IScene & { baseUrl: string }) {
  if (!scene) return

  let id = '0x0'
  if (scene && scene.scene && scene.scene.base) {
    const [x, y] = scene.scene.base.split(',').map($ => parseInt($, 10))
    id = `${x},${y}`
  }

  const publisher = '0x0'

  const contents = normalizeContentMappings(scene._mappings || [])

  if (!scene.baseUrl) throw new Error('baseUrl missing in scene')

  let defaultScene: ILand = {
    baseUrl: scene.baseUrl,
    scene,
    mappingsResponse: {
      contents,
      parcel_id: id,
      publisher,
      root_cid: 'Qmtest'
    }
  }

  await initializePreview(ILandToLoadableParcelScene(defaultScene), scene.scene.parcels.length)
}

async function initializePreview(userScene: EnvironmentData<LoadableParcelScene>, parcelCount: number) {
  loadedParcelSceneWorkers.forEach($ => {
    $.dispose()
    loadedParcelSceneWorkers.delete($)
  })
  webGlParcelScene = new WebGLParcelScene(userScene)
  let parcelScene = new SceneWorker(webGlParcelScene)
  const context = webGlParcelScene.context as SharedSceneContext

  context.on('uuidEvent' as any, event => {
    const { type } = event.payload

    if (type === 'gizmoSelected') {
      evtEmitter.emit('gizmoSelected', {
        gizmoType: event.payload.gizmoType,
        entityId: event.payload.entityId
      })
    } else if (type === 'gizmoDragEnded') {
      evtEmitter.emit('transform', {
        entityId: event.payload.entityId,
        transform: event.payload.transform
      })
    }
  })

  context.on('metricsUpdate', e => {
    evtEmitter.emit('metrics', {
      metrics: e.given,
      limits: e.limit
    })
  })

  context.on('entitiesOutOfBoundaries', e => {
    evtEmitter.emit('entitiesOutOfBoundaries', e)
  })

  context.on('entityOutOfScene', e => {
    evtEmitter.emit('entityOutOfScene', e)
  })

  context.on('entityBackInScene', e => {
    evtEmitter.emit('entityBackInScene', e)
  })

  // we need closeParcelScenes to enable interactions in preview mode
  loadedParcelSceneWorkers.add(parcelScene)

  enablePositionReporting()

  if (!didStartPosition) {
    // TODO (eordano): Find a fancier way to do this
    // As the "+5,+5" is a hack to make the scene appear in front of the user
    initLocalPlayer({
      x: parcelScene.position.x + 5,
      y: 0,
      z: parcelScene.position.z - 5
    })
    didStartPosition = true
  }

  const system = await parcelScene.system

  const engineAPI = parcelScene.engineAPI!

  while (!system.isEnabled || !engineAPI.didStart) {
    await sleep(10)
  }

  evtEmitter.emit('ready', {})
}

export namespace editor {
  export const babylon = BABYLON

  export async function handleMessage(message: any) {
    if (message.type === 'update') {
      await loadScene(message.payload.scene)
    }
  }

  export function setGridResolution(position: number, scale: number, radians: number) {
    Gizmos.gizmoManager.gizmos.positionGizmo!.snapDistance = position
    Gizmos.gizmoManager.gizmos.scaleGizmo!.snapDistance = scale
    Gizmos.gizmoManager.gizmos.rotationGizmo!.snapDistance = radians
  }

  export function selectEntity(entityId: string) {
    if (webGlParcelScene) {
      const context = webGlParcelScene.context as SharedSceneContext
      const entity = context.entities.get(entityId)
      if (!entity) {
        throw new Error(`Entity(${entityId}) not found`)
      }

      const gizmos = entity.getBehaviorByName('gizmos') as Gizmos.Gizmos

      if (!entity) {
        throw new Error(`Entity(${entityId}) has no gizmo component`)
      }

      gizmos.activate()
    }
  }

  export function getDCLCanvas() {
    return domReadyFuture.isPending ? domReadyFuture : Promise.resolve(canvas)
  }

  export function getScenes() {
    return loadedParcelSceneWorkers
  }

  function configureEditorEnvironment(enabled: boolean) {
    const target = new BABYLON.Vector3((parcelsX * 10) / 2, 0, (parcelsY * 10) / 2)

    setEditorEnvironment(enabled)

    if (enabled) {
      if (document.pointerLockElement) {
        document.exitPointerLock()
      }
      arcCamera.target.copyFrom(target)
      arcCamera.alpha = -Math.PI / 4
      arcCamera.beta = Math.PI / 3
      arcCamera.radius = DEFAULT_CAMERA_ZOOM
    } else {
      canvas.focus()
      canvas.requestPointerLock()
      if (webGlParcelScene) {
        vrCamera.position.x = webGlParcelScene.worker!.position.x + 5
        vrCamera.position.y = 1.6
        vrCamera.position.z = webGlParcelScene.worker!.position.z + 5
      }
    }
  }

  /**
   * Call this function when the content mappings has changed
   */
  function setMappings(mappings: Record<string, string> | Array<ContentMapping>) {
    const context = webGlParcelScene!.context as SharedSceneContext
    const seenMappings = new Set()

    const sanitizedMappings = normalizeContentMappings(mappings)

    for (let { file, hash } of sanitizedMappings) {
      seenMappings.add(file)
      context.registerMappings([{ file, hash: hash }])
    }

    context.registeredMappings.forEach((_, file) => {
      // TODO: check no textures or models or sounds are using the mappings we are removing
      if (!seenMappings.has(file)) {
        context.registeredMappings.delete(file)
      }
    })
  }

  /**
   * Call this function when wanting to send an action to the worker
   */
  export function sendExternalAction(action: { type: string; payload: { [key: string]: any } }) {
    if (webGlParcelScene && webGlParcelScene.worker) {
      const worker = webGlParcelScene.worker as SceneWorker

      if (action.payload.mappings) {
        setMappings(action.payload.mappings)
      }

      worker.engineAPI!.sendSubscriptionEvent('externalAction', action)
    }
  }

  export async function initEngine(px: number = 1, py: number = 1) {
    parcelsX = px
    parcelsY = py

    await initBabylonClient()
    configureEditorEnvironment(true)
  }

  export function selectGizmo(type: Gizmo) {
    Gizmos.selectGizmo(type)
  }

  export async function setPlayMode(on: boolean) {
    configureEditorEnvironment(!on)
  }

  export async function resize() {
    onWindowResize()
  }

  export function on(evt: string, listener: (...args: any[]) => void) {
    evtEmitter.addListener(evt, listener)
  }

  export function off(evt: string, listener: (...args: any[]) => void) {
    evtEmitter.removeListener(evt, listener)
  }

  export function setCameraZoomDelta(delta: number) {
    arcCamera.radius += delta

    if (arcCamera.radius > arcCamera.upperRadiusLimit!) {
      arcCamera.radius = arcCamera.upperRadiusLimit!
    }

    if (arcCamera.radius < arcCamera.lowerRadiusLimit!) {
      arcCamera.radius = arcCamera.lowerRadiusLimit!
    }
  }

  export function getCameraTarget() {
    return arcCamera.target.clone()
  }

  export function resetCameraZoom() {
    arcCamera.radius = DEFAULT_CAMERA_ZOOM
  }

  export function getMouseWorldPosition(localX: number, localY: number) {
    return rayToGround(localX, localY)
  }

  export function loadImage(url: string, image: HTMLImageElement) {
    scene.database.loadImageFromDB(url, image)
  }

  export function preloadFile(url: string, useArrayBuffer = true): Promise<string | ArrayBuffer> {
    const defer = future()

    BABYLON.Tools.LoadFile(
      url,
      data => {
        defer.resolve(data)
      },
      void 0,
      scene.database,
      useArrayBuffer,
      (_: any, err: any) => {
        defer.reject(err)
      }
    )

    return defer
  }

  export function setCameraRotation(alpha: number, beta?: number) {
    arcCamera.alpha = alpha
    if (beta !== undefined) {
      arcCamera.beta = beta
    }
  }

  export function getLoadingEntity(): BaseEntity | null {
    if (webGlParcelScene) {
      const context = webGlParcelScene.context as SharedSceneContext
      return context.rootEntity.getLoadingEntity()
    }

    return null
  }

  export function takeScreenshot(): IFuture<string> {
    const ret = future()

    scene.onAfterRenderObservable.addOnce(() => {
      ret.resolve(canvas.toDataURL())
    })

    return ret
  }

  export const envHelper = _envHelper

  export const setCameraPosition = _setCameraPosition

  export function getCameraPosition() {
    const ret = new Vector3()
    cameraPositionToRef(ret)
    return ret
  }
}

global['editor'] = editor
