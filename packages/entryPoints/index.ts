import 'engine'

import { initBabylonClient } from '../engine/dcl'
import { domReadyFuture, bodyReadyFuture, scene } from '../engine/renderer/init'
import { initShared } from '../shared'
import { enableParcelSceneLoading } from '../shared/world/parcelSceneManager'
import { WebGLParcelScene } from '../engine/dcl/WebGLParcelScene'
import { enableMiniMap } from '../engine/dcl/widgets/minimap'
import { getWorldSpawnpoint } from '../shared/world/positionThings'
import { worldRunningObservable } from '../shared/world/worldState'
import { ILand } from '../shared/types'

document.body.classList.remove('dcl-loading')

const container = document.body

async function loadClient() {
  await initBabylonClient()

  container.appendChild(enableMiniMap())

  await enableParcelSceneLoading({
    parcelSceneClass: WebGLParcelScene,
    onPositionSettled: initialLand => {
      initialLand && setInitialPosition(initialLand)
      worldRunningObservable.notifyObservers(true)
    },
    preloadScene: async () => true
  })
}

bodyReadyFuture
  .then(async body => {
    await initShared(container)

    await loadClient()

    domReadyFuture
      .then(canvas => {
        body.appendChild(canvas)
      })
      .catch(handleError)
  })
  .catch(handleError)

function setInitialPosition(initialLand: ILand) {
  const newPosition = getWorldSpawnpoint(initialLand).position
  const result = new BABYLON.Vector3(newPosition.x, newPosition.y, newPosition.z)
  if (scene.activeCamera) {
    scene.activeCamera.position.copyFrom(result)
  }
}

function handleError(e: Error) {
  container.classList.remove('dcl-loading')
  container.innerHTML = `<h3>${e.message}</h3>`
}
