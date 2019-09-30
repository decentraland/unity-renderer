// tslint:disable:no-console
declare var global: any
declare var window: any

global['preview'] = window['preview'] = true
global['enableWeb3'] = window['enableWeb3']

import { initializeUnity } from '../unity-interface/initializer'
import { loadPreviewScene, HUD } from '../unity-interface/dcl'
import { DEBUG_WS_MESSAGES } from '../config'
import defaultLogger from 'shared/logger'
import { future, IFuture } from 'fp-future'
import { ILand } from 'shared/types'
import { pickWorldSpawnpoint } from 'shared/world/positionThings'
import { sceneLifeCycleObservable } from 'decentraland-loader/lifecycle/controllers/scene'

// Remove the 'dcl-loading' class, used until JS loads.
document.body.classList.remove('dcl-loading')

const container = document.getElementById('gameContainer')

if (!container) throw new Error('cannot find element #gameContainer')

const defaultScene: IFuture<ILand> = future()

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

function sceneRenderable() {
  const sceneRenderable = future<void>()

  const timer = setTimeout(() => {
    if (sceneRenderable.isPending) {
      sceneRenderable.reject(new Error('scene never got ready'))
    }
  }, 30000)

  const observer = sceneLifeCycleObservable.add(async sceneStatus => {
    if (sceneStatus.sceneId === (await defaultScene).sceneId) {
      sceneLifeCycleObservable.remove(observer)
      clearTimeout(timer)
      sceneRenderable.resolve()
    }
  })

  return sceneRenderable
}

initializeUnity(container)
  .then(async ret => {
    HUD.Minimap.configure({ active: true })
    HUD.Notification.configure({ active: true })

    const renderable = sceneRenderable()

    startPreviewWatcher()

    await renderable

    ret.instancedJS
      .then(async ({ unityInterface }) => {
        unityInterface.Teleport(pickWorldSpawnpoint(await defaultScene))
        unityInterface.ActivateRendering()
      })
      .catch(defaultLogger.error)
  })
  .catch(err => {
    console['error']('Error loading Unity')
    console['error'](err)
  })
