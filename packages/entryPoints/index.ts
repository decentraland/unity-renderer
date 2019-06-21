import 'engine'

import { ETHEREUM_NETWORK } from '../config'
import { initBabylonClient } from '../engine/dcl'
import { domReadyFuture, bodyReadyFuture, scene } from '../engine/renderer/init'
import { initShared } from '../shared'
import { enableParcelSceneLoading } from '../shared/world/parcelSceneManager'
import { WebGLParcelScene } from '../engine/dcl/WebGLParcelScene'
import { enableMiniMap } from '../engine/dcl/widgets/minimap'
import { getWorldSpawnpoint } from '../shared/world/positionThings'

document.body.classList.remove('dcl-loading')

const container = document.body

async function loadClient(net: ETHEREUM_NETWORK) {
  await initBabylonClient()

  container.appendChild(enableMiniMap())

  await enableParcelSceneLoading(net, {
    parcelSceneClass: WebGLParcelScene,
    onSpawnpoint: initialLand => {
      const newPosition = getWorldSpawnpoint(initialLand)
      const result = new BABYLON.Vector3(newPosition.x, newPosition.y, newPosition.z)
      if (scene.activeCamera) {
        scene.activeCamera.position.copyFrom(result)
      }
    },
    shouldLoadParcelScene: () => true
  })
}

bodyReadyFuture
  .then(async body => {
    const net = await initShared(container)

    await loadClient(net)

    domReadyFuture
      .then(canvas => {
        body.appendChild(canvas)
      })
      .catch(handleError)
  })
  .catch(handleError)

function handleError(e: Error) {
  container.classList.remove('dcl-loading')
  container.innerHTML = `<h3>${e.message}</h3>`
}
