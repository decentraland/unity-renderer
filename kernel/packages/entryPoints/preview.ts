declare const global: any & StoreContainer
declare const window: any
;(window as any).reactVersion = false

// IMPORTANT! This should be execd before loading 'config' module to ensure that init values are successfully loaded
global.preview = window.preview = true
global.enableWeb3 = window.enableWeb3

import { initializeUnity } from 'unity-interface/initializer'
import { loadPreviewScene } from 'unity-interface/dcl'
import { DEBUG_WS_MESSAGES, FORCE_RENDERING_STYLE } from 'config'
import defaultLogger from 'shared/logger'
import { ILand, HUDElementID } from 'shared/types'
import { pickWorldSpawnpoint } from 'shared/world/positionThings'
import { signalRendererInitialized } from 'shared/renderer/actions'
import { StoreContainer } from 'shared/store/rootTypes'
import { future, IFuture } from 'fp-future'
import { sceneLifeCycleObservable } from 'decentraland-loader/lifecycle/controllers/scene'

// Remove the 'dcl-loading' class, used until JS loads.
document.body.classList.remove('dcl-loading')

const container = document.getElementById('gameContainer')

if (!container) throw new Error('cannot find element #gameContainer')

const defaultScene: IFuture<ILand> = future()

let wsScene: string | undefined = undefined

if (location.search.indexOf('WS_SCENE') !== -1) {
  wsScene = `${location.protocol === 'https:' ? 'wss' : 'ws'}://${document.location.host}/?scene`
}

function startPreviewWatcher() {
  // this is set to avoid double loading scenes due queued messages
  let isSceneLoading: boolean = true

  const loadScene = () => {
    isSceneLoading = true
    loadPreviewScene(wsScene)
      .then((scene) => {
        isSceneLoading = false
        defaultScene.resolve(scene)
      })
      .catch((err) => {
        isSceneLoading = false
        defaultLogger.error('Error loading scene', err)
        defaultScene.reject(err)
      })
  }

  loadScene()

  global.handleServerMessage = function (message: any) {
    if (message.type === 'update') {
      if (DEBUG_WS_MESSAGES) {
        defaultLogger.info('Message received: ', message)
      }
      // if a scene is currently loading we do not trigger another load
      if (isSceneLoading) {
        if (DEBUG_WS_MESSAGES) {
          defaultLogger.trace('Ignoring message, scene still loading...')
        }
        return
      }

      loadScene()
    }
  }
}

function sceneRenderable() {
  const sceneRenderable = future<void>()

  const observer = sceneLifeCycleObservable.add(async (sceneStatus) => {
    if (sceneStatus.sceneId === (await defaultScene).sceneId) {
      sceneLifeCycleObservable.remove(observer)
      sceneRenderable.resolve()
    }
  })

  return sceneRenderable
}

initializeUnity(container)
  .then(async (ret) => {
    const i = (await ret.instancedJS).unityInterface
    i.ConfigureHUDElement(HUDElementID.MINIMAP, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.NOTIFICATION, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.SETTINGS_PANEL, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.AIRDROPPING, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.OPEN_EXTERNAL_URL_PROMPT, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.NFT_INFO_DIALOG, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.TELEPORT_DIALOG, { active: true, visible: false })

    global.globalStore.dispatch(signalRendererInitialized())

    if (FORCE_RENDERING_STYLE) {
      i.SetRenderProfile(FORCE_RENDERING_STYLE)
    }

    const renderable = sceneRenderable()

    startPreviewWatcher()

    await renderable

    i.Teleport(pickWorldSpawnpoint(await defaultScene))
    i.ActivateRendering()
  })
  .catch((err) => {
    defaultLogger.error('There was an error', err)
  })
