import { TeleportController } from 'shared/world/TeleportController'
import { DEBUG, EDITOR, ENGINE_DEBUG_PANEL, NO_ASSET_BUNDLES, SCENE_DEBUG_PANEL, SHOW_FPS_COUNTER } from 'config'
import { aborted } from 'shared/loading/ReportFatalError'
import { loadingScenes, setLoadingScreen, teleportTriggered } from 'shared/loading/types'
import { defaultLogger } from 'shared/logger'
import { ILand, LoadableParcelScene, MappingsResponse, SceneJsonData } from 'shared/types'
import {
  enableParcelSceneLoading,
  getParcelSceneID,
  loadParcelScene,
  stopParcelSceneWorker
} from 'shared/world/parcelSceneManager'
import { teleportObservable } from 'shared/world/positionThings'
import { SceneWorker } from 'shared/world/SceneWorker'
import { hudWorkerUrl } from 'shared/world/SceneSystemWorker'
import { renderStateObservable } from 'shared/world/worldState'
import { StoreContainer } from 'shared/store/rootTypes'
import { ILandToLoadableParcelScene, ILandToLoadableParcelSceneUpdate } from 'shared/selectors'
import { UnityParcelScene } from './UnityParcelScene'

import { loginCompleted } from 'shared/ethereum/provider'
import { UnityInterface, unityInterface } from './UnityInterface'
import { BrowserInterface, browserInterface } from './BrowserInterface'
import { UnityScene } from './UnityScene'
import { ensureUiApis } from 'shared/world/uiSceneInitializer'
import Html from '../shared/Html'
import { WebSocketTransport } from 'decentraland-rpc'
import { kernelConfigForRenderer } from './kernelConfigForRenderer'
import type { ScriptingTransport } from 'decentraland-rpc/lib/common/json-rpc/types'

declare const globalThis: UnityInterfaceContainer &
  BrowserInterfaceContainer &
  StoreContainer & { analytics: any; delighted: any }

export type BrowserInterfaceContainer = {
  browserInterface: BrowserInterface
}

export type UnityInterfaceContainer = {
  unityInterface: UnityInterface
}

globalThis.browserInterface = browserInterface
globalThis.unityInterface = unityInterface

type GameInstance = {
  SendMessage(object: string, method: string, ...args: (number | string)[]): void
}

const rendererVersion = require('decentraland-renderer')
window['console'].log('Renderer version: ' + rendererVersion)

export let gameInstance!: GameInstance
export let isTheFirstLoading = true

export function setLoadingScreenVisible(shouldShow: boolean) {
  globalThis.globalStore.dispatch(setLoadingScreen(shouldShow))
  Html.setLoadingScreen(shouldShow)

  if (!shouldShow && !EDITOR) {
    isTheFirstLoading = false
    TeleportController.stopTeleportAnimation()
  }
}

////////////////////////////////////////////////////////////////////////////////

function debuggingDecorator(_gameInstance: GameInstance) {
  const debug = false
  const decorator = {
    // @ts-ignore
    SendMessage: (...args) => {
      defaultLogger.info('gameInstance', ...args)
      // @ts-ignore
      _gameInstance.SendMessage(...args)
    }
  }
  return debug ? decorator : _gameInstance
}

/**
 *
 * Common initialization logic for the unity engine
 *
 * @param _gameInstance Unity game instance
 */
export async function initializeEngine(_gameInstance: GameInstance) {
  gameInstance = debuggingDecorator(_gameInstance)

  unityInterface.Init(_gameInstance)

  unityInterface.DeactivateRendering()

  unityInterface.SetKernelConfiguration(kernelConfigForRenderer())

  if (DEBUG) {
    unityInterface.SetDebug()
  }

  if (SCENE_DEBUG_PANEL) {
    unityInterface.SetSceneDebugPanel()
  }

  if (NO_ASSET_BUNDLES) {
    unityInterface.SetDisableAssetBundles()
  }

  if (SHOW_FPS_COUNTER) {
    unityInterface.ShowFPSPanel()
  }

  if (ENGINE_DEBUG_PANEL) {
    unityInterface.SetEngineDebugPanel()
  }

  if (!EDITOR) {
    await startGlobalScene(unityInterface)
  }

  return {
    unityInterface,
    onMessage(type: string, message: any) {
      if (type in browserInterface) {
        // tslint:disable-next-line:semicolon
        ;(browserInterface as any)[type](message)
      } else {
        defaultLogger.info(`Unknown message (did you forget to add ${type} to unity-interface/dcl.ts?)`, message)
      }
    }
  }
}

export async function startGlobalScene(unityInterface: UnityInterface) {
  const sceneId = 'dcl-ui-scene'

  const scene = new UnityScene({
    sceneId,
    name: 'ui',
    baseUrl: location.origin,
    main: hudWorkerUrl,
    useFPSThrottling: false,
    data: {},
    mappings: []
  })

  const worker = loadParcelScene(scene, undefined, true)

  await ensureUiApis(worker)

  unityInterface.CreateUIScene({ id: getParcelSceneID(scene), baseUrl: scene.data.baseUrl })
}

export async function startUnitySceneWorkers() {
  globalThis.globalStore.dispatch(loadingScenes())

  await enableParcelSceneLoading({
    parcelSceneClass: UnityParcelScene,
    preloadScene: async (_land) => {
      // TODO:
      // 1) implement preload call
      // 2) await for preload message or timeout
      // 3) return
    },
    onLoadParcelScenes: (lands) => {
      unityInterface.LoadParcelScenes(
        lands.map(($) => {
          const x = Object.assign({}, ILandToLoadableParcelScene($).data)
          delete x.land
          return x
        })
      )
    },
    onUnloadParcelScenes: (lands) => {
      lands.forEach(($) => {
        unityInterface.UnloadScene($.sceneId)
      })
    },
    onPositionSettled: (spawnPoint) => {
      if (!aborted) {
        unityInterface.Teleport(spawnPoint)
        unityInterface.ActivateRendering()
      }
    },
    onPositionUnsettled: () => {
      unityInterface.DeactivateRendering()
    }
  })
}

// Builder functions
let currentLoadedScene: SceneWorker | null

export async function loadPreviewScene(ws?: string) {
  const result = await fetch('/scene.json?nocache=' + Math.random())

  let lastId: string | null = null

  if (currentLoadedScene) {
    lastId = currentLoadedScene.getSceneId()
    stopParcelSceneWorker(currentLoadedScene)
  }

  if (result.ok) {
    // we load the scene to get the metadata
    // about rhe bounds and position of the scene
    // TODO(fmiras): Validate scene according to https://github.com/decentraland/proposals/blob/master/dsp/0020.mediawiki
    const scene = (await result.json()) as SceneJsonData
    const mappingsFetch = await fetch('/mappings')
    const mappingsResponse = (await mappingsFetch.json()) as MappingsResponse

    let defaultScene: ILand = {
      sceneId: 'previewScene',
      baseUrl: location.toString().replace(/\?[^\n]+/g, ''),
      baseUrlBundles: '',
      sceneJsonData: scene,
      mappingsResponse: mappingsResponse
    }

    const parcelScene = new UnityParcelScene(ILandToLoadableParcelScene(defaultScene))

    let transport: undefined | ScriptingTransport = undefined

    if (ws) {
      transport = WebSocketTransport(new WebSocket(ws, ['dcl-scene']))
    }

    currentLoadedScene = loadParcelScene(parcelScene, transport)

    const target: LoadableParcelScene = { ...ILandToLoadableParcelScene(defaultScene).data }
    delete target.land

    defaultLogger.info('Reloading scene...')

    if (lastId) {
      unityInterface.UnloadScene(lastId)
    }

    unityInterface.LoadParcelScenes([target])

    defaultLogger.info('finish...')

    return defaultScene
  } else {
    throw new Error('Could not load scene.json')
  }
}

export function loadBuilderScene(sceneData: ILand) {
  unloadCurrentBuilderScene()

  const parcelScene = new UnityParcelScene(ILandToLoadableParcelScene(sceneData))
  currentLoadedScene = loadParcelScene(parcelScene)

  const target: LoadableParcelScene = { ...ILandToLoadableParcelScene(sceneData).data }
  delete target.land

  unityInterface.LoadParcelScenes([target])
  return parcelScene
}

export function unloadCurrentBuilderScene() {
  if (currentLoadedScene) {
    unityInterface.DeactivateRendering()
    currentLoadedScene.emit('builderSceneUnloaded', {})

    stopParcelSceneWorker(currentLoadedScene)
    unityInterface.SendBuilderMessage('UnloadBuilderScene', currentLoadedScene.getSceneId())
    currentLoadedScene = null
  }
}

export function updateBuilderScene(sceneData: ILand) {
  if (currentLoadedScene) {
    const target: LoadableParcelScene = { ...ILandToLoadableParcelSceneUpdate(sceneData).data }
    delete target.land
    unityInterface.UpdateParcelScenes([target])
  }
}

teleportObservable.add((position: { x: number; y: number; text?: string }) => {
  // before setting the new position, show loading screen to avoid showing an empty world
  setLoadingScreenVisible(true)
  globalThis.globalStore.dispatch(teleportTriggered(position.text || `Teleporting to ${position.x}, ${position.y}`))
})

renderStateObservable.add(async (isRunning) => {
  if (isRunning) {
    await loginCompleted
    setLoadingScreenVisible(false)
  }
})

document.addEventListener('pointerlockchange', pointerLockChange, false)

let isPointerLocked: boolean = false

function pointerLockChange() {
  const doc: any = document
  const isLocked = (doc.pointerLockElement || doc.mozPointerLockElement || doc.webkitPointerLockElement) != null
  if (isPointerLocked !== isLocked) {
    unityInterface.SetCursorState(isLocked)
  }
  isPointerLocked = isLocked
}
