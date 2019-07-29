// tslint:disable:no-console
declare var global: any
declare var window: any

global['preview'] = window['preview'] = true
global['enableWeb3'] = window['enableWeb3']

import { initializeUnity } from '../unity-interface/initializer'
import { loadPreviewScene } from '../unity-interface/dcl'

// Remove the 'dcl-loading' class, used until JS loads.
document.body.classList.remove('dcl-loading')

const container = document.getElementById('gameContainer')

if (!container) throw new Error('cannot find element #gameContainer')

function startPreviewWatcher() {
  // this is set to avoid double loading scenes due queued messages
  let currentlyLoadingScene: Promise<any> | null = loadPreviewScene()

  global['handleServerMessage'] = function(message: any) {
    if (message.type === 'update') {
      // if a scene is currently loading we do not trigger another load
      if (currentlyLoadingScene) return

      currentlyLoadingScene = loadPreviewScene()

      currentlyLoadingScene
        .then(() => {
          currentlyLoadingScene = null
        })
        .catch(err => {
          currentlyLoadingScene = null
          console.error('Error loading scene')
          console.error(err)
        })
    }
  }
}

initializeUnity(container)
  .then(ret => {
    startPreviewWatcher()
  })
  .catch(err => {
    console['error']('Error loading Unity')
    console['error'](err)
  })
