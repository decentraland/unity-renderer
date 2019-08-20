import { initializeUnity } from '../unity-interface/initializer'
import { startUnityParcelLoading } from '../unity-interface/dcl'
import { lastPlayerPosition, teleportObservable } from '../shared/world/positionThings'
import defaultLogger from '../shared/logger'
import { worldToGrid } from '../atomicHelpers/parcelScenePositions'

const container = document.getElementById('gameContainer')

if (!container) throw new Error('cannot find element #gameContainer')

initializeUnity(container)
  .then(async _ => {
    await startUnityParcelLoading()

    _.instancedJS
      .then($ => {
        const gridPosition = { x: 0, y: 0 }
        worldToGrid(lastPlayerPosition, gridPosition)
        teleportObservable.notifyObservers(gridPosition)
      })
      .catch(defaultLogger.error)

    document.body.classList.remove('dcl-loading')
  })
  .catch(err => {
    if (err.message.includes('Authentication error')) {
      window.location.reload()
    }

    console['error']('Error loading Unity')
    console['error'](err)

    container.innerText = err.toString()

    document.body.classList.remove('dcl-loading')
  })
