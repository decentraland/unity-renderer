global['preview'] = window['preview'] = true

import 'engine'

import { initLocalPlayer, domReadyFuture } from '../engine/renderer'

import { initBabylonClient } from '../dcl'
import { log } from '../engine/logger'
import { bodyReadyFuture, engine } from '../engine/renderer/init'
import { initShared } from '../shared'
import { loadedParcelSceneWorkers, enablePositionReporting } from '../shared/world/parcelSceneManager'
import { ETHEREUM_NETWORK, DEBUG } from '../config'
import { ILandToLoadableParcelScene, ILand, IScene, MappingsResponse } from '../shared/types'
import { SceneWorker } from '../shared/world/SceneWorker'
import { WebGLParcelScene } from '../dcl/WebGLParcelScene'

let didStartPosition = false

async function loadScene() {
  const result = await fetch('/scene.json?nocache=' + Math.random())

  if (result.ok) {
    // we load the scene to get the metadata
    // about rhe bounds and position of the scene
    // TODO(fmiras): Validate scene according to https://github.com/decentraland/proposals/blob/master/dsp/0020.mediawiki
    const scene = (await result.json()) as IScene

    const mappingsFetch = await fetch('/mappings')
    const mappingsResponse = (await mappingsFetch.json()) as MappingsResponse

    let defaultScene: ILand = {
      baseUrl: location.toString().replace(/\?[^\n]+/g, ''),
      scene,
      mappingsResponse: mappingsResponse
    }

    await initializePreview(defaultScene)
  } else {
    throw new Error('Could not load scene.json')
  }
}

async function initializePreview(userScene: ILand) {
  log('Starting Preview...')

  loadedParcelSceneWorkers.forEach($ => {
    $.dispose()
    loadedParcelSceneWorkers.delete($)
  })

  let parcelSceneWorker = new SceneWorker(new WebGLParcelScene(ILandToLoadableParcelScene(userScene)))

  // we need closeParcelScenes to enable interactions in preview mode
  loadedParcelSceneWorkers.add(parcelSceneWorker)

  enablePositionReporting()

  if (!didStartPosition) {
    // The 0,-15 rotation is a hack to put a player into a corner of the scene and look at the center
    initLocalPlayer({ x: parcelSceneWorker.position.x, y: 0, z: parcelSceneWorker.position.z }, { x: 0, y: 50.54 })

    didStartPosition = true
  }
}

async function loadClient() {
  await initBabylonClient()
  await loadScene()

  document.body.classList.remove('dcl-loading')
}

{
  global['handleServerMessage'] = function(message) {
    if (message.type === 'update') {
      loadScene()
    }
  }

  bodyReadyFuture
    .then(async body => {
      const { net } = await initShared()

      await loadClient()

      // Warn in case wallet is set in mainnet
      if (net === ETHEREUM_NETWORK.MAINNET && DEBUG) {
        const style = document.createElement('style') as HTMLStyleElement
        style.appendChild(
          document.createTextNode(
            `body:before{content:'You are using Mainnet Ethereum Network, real transactions are going to be made.';background:#ff0044;color:#fff;text-align:center;text-transform:uppercase;height:24px;width:100%;position:fixed;padding-top:2px}#main-canvas{padding-top:24px};`
          )
        )
        document.head.appendChild(style)
      }

      domReadyFuture.then(canvas => {
        body.appendChild(canvas)
        engine.resize()
      })
    })
    .catch(error => {
      document.body.classList.remove('dcl-loading')
      document.body.innerHTML = `
        <div style='padding: 30px; color: red; font-size: 16pt'>
          <h1>Error loading scene</h1>
          <pre>
              <code>${error.message}</code>
          </pre>
        </div>
      `
    })
}
