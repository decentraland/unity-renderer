global['preview'] = window['preview'] = true

import 'engine'

import { initLocalPlayer, domReadyFuture, onWindowResize } from '../engine/renderer'

import { initBabylonClient } from '../dcl'
import { log } from '../engine/logger'
import * as _envHelper from '../engine/renderer/envHelper'
import { canvas } from '../engine/renderer/init'
import { initShared } from '../shared'
import { loadedParcelSceneWorkers } from '../shared/world/parcelSceneManager'
import { LoadableParcelScene, ILandToLoadableParcelScene, ILand, IScene, EnvironmentData } from '../shared/types'
import { SceneWorker } from '../shared/world/SceneWorker'
import { WebGLParcelScene } from '../dcl/WebGLParcelScene'

let didStartPosition = false

async function loadScene(scene: IScene) {
  if (!scene) return

  let id = '0x0'
  if (scene && scene.scene && scene.scene.base) {
    const [x, y] = scene.scene.base.split(',').map($ => parseInt($, 10))
    id = `${x},${y}`
  }

  const publisher = '0x0'

  const mappings = scene._mappings || {}

  let defaultScene: ILand = {
    baseUrl: location.toString().replace(/\?[^\n]+/g, ''),
    scene,
    mappingsResponse: {
      contents: mappings,
      parcel_id: id,
      publisher,
      root_cid: 'Qmtest'
    }
  }

  await initializePreview(ILandToLoadableParcelScene(defaultScene))
}

async function initializePreview(userScene: EnvironmentData<LoadableParcelScene>) {
  log('Starting Preview...')

  loadedParcelSceneWorkers.forEach($ => {
    $.dispose()
    loadedParcelSceneWorkers.delete($)
  })

  let parcelScene = new SceneWorker(new WebGLParcelScene(userScene))

  // we need closeParcelScenes to enable interactions in preview mode
  loadedParcelSceneWorkers.add(parcelScene)

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
}

export namespace editor {
  export const babylon = BABYLON

  export function handleServerMessage(message) {
    if (message.type === 'update') {
      loadScene(message.payload.scene)
    }
  }

  export function getDCLCanvas() {
    return domReadyFuture.isPending ? domReadyFuture : Promise.resolve(canvas)
  }

  export async function initEngine(scene) {
    await initShared()
    await initBabylonClient()
    await loadScene(scene)
  }

  export async function resize() {
    onWindowResize()
  }

  export const envHelper = _envHelper
}

global['editor'] = editor
