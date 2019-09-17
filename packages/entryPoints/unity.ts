import { initializeUnity } from '../unity-interface/initializer'
import { startUnityParcelLoading } from '../unity-interface/dcl'
import { worldToGrid } from '../atomicHelpers/parcelScenePositions'
import { lastPlayerPosition, teleportObservable } from '../shared/world/positionThings'
import defaultLogger from '../shared/logger'

const container = document.getElementById('gameContainer')

if (!container) throw new Error('cannot find element #gameContainer')

initializeUnity(container)
  .then(async _ => {
    await startUnityParcelLoading()

    _.instancedJS
      .then($ => teleportObservable.notifyObservers(worldToGrid(lastPlayerPosition)))
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
