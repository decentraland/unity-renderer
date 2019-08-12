// tslint:disable:no-console
declare var global: any
declare var window: any

global['preview'] = window['preview'] = true
global['enableWeb3'] = window['enableWeb3']

import { initializeUnity } from '../unity-interface/initializer'
import { loadPreviewScene } from '../unity-interface/dcl'
import { DEBUG_WS_MESSAGES } from '../config'

// Remove the 'dcl-loading' class, used until JS loads.
document.body.classList.remove('dcl-loading')

const container = document.getElementById('gameContainer')

if (!container) throw new Error('cannot find element #gameContainer')

function startPreviewWatcher() {
  // this is set to avoid double loading scenes due queued messages
  let isSceneLoading: boolean = true

  const loadScene = () => {
    loadPreviewScene()
      .then(() => {
        isSceneLoading = false
      })
      .catch(err => {
        isSceneLoading = false
        console.error('Error loading scene')
        console.error(err)
      })
  }

  loadScene()

  global['handleServerMessage'] = function(message: any) {
    if (message.type === 'update') {
      if (DEBUG_WS_MESSAGES) {
        console.log('Message received: ', message)
      }
      // if a scene is currently loading we do not trigger another load
      if (isSceneLoading) {
        if (DEBUG_WS_MESSAGES) {
          console.log('Ignoring message, scene still loading...')
        }
        return
      }

      isSceneLoading = true
      loadScene()
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
