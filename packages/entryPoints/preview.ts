// tslint:disable:no-console
declare var global: any
declare var window: any

global['preview'] = window['preview'] = true
global['enableWeb3'] = window['enableWeb3']

import { initializeUnity } from '../unity-interface/initializer'
import { loadPreviewScene, setInitialPosition } from '../unity-interface/dcl'
import { DEBUG_WS_MESSAGES } from '../config'
import defaultLogger from '../shared/logger'
import future from 'fp-future'
import { worldRunningObservable } from '../shared/world/worldState'

// Remove the 'dcl-loading' class, used until JS loads.
document.body.classList.remove('dcl-loading')

const container = document.getElementById('gameContainer')

if (!container) throw new Error('cannot find element #gameContainer')

const defaultScene = future()

function startPreviewWatcher() {
  // this is set to avoid double loading scenes due queued messages
  let isSceneLoading: boolean = true

  const loadScene = () => {
    loadPreviewScene()
      .then(scene => {
        isSceneLoading = false
        defaultScene.resolve(scene)
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
  .then(async ret => {
    startPreviewWatcher()

    const scene = await defaultScene
    worldRunningObservable.add(running => running && setInitialPosition(scene), undefined, true, undefined, true)

    ret.instancedJS
      .then($ => {
        $.unityInterface.ActivateRendering()
      })
      .catch(defaultLogger.error)
  })
  .catch(err => {
    console['error']('Error loading Unity')
    console['error'](err)
  })
