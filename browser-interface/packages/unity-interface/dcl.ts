import { DEBUG, ENGINE_DEBUG_PANEL, rootURLPreviewMode, SCENE_DEBUG_PANEL, SHOW_FPS_COUNTER } from 'config'
import './UnityInterface'
import {
  allScenesEvent,
  loadParcelSceneWorker,
  reloadScene,
  unloadParcelSceneById
} from 'shared/world/parcelSceneManager'
import { getUnityInstance } from './IUnityInterface'
import { clientDebug, ClientDebug } from './ClientDebug'
import { kernelConfigForRenderer } from './kernelConfigForRenderer'
import { store } from 'shared/store/isolatedStore'
import type { UnityGame } from 'unity-interface/loader'
import { traceDecoratorUnityGame } from './trace'
import defaultLogger from 'lib/logger'
import { ContentMapping, EntityType, Scene, sdk } from '@dcl/schemas'
import { ensureMetaConfigurationInitialized } from 'shared/meta'
import { reloadScenePortableExperience } from 'shared/portableExperiences/actions'
import { wearableToSceneEntity } from 'shared/wearablesPortableExperience/sagas'
import { fetchScenesByLocation } from 'shared/scene-loader/sagas'
import { sleep } from 'lib/javascript/sleep'
import { avatarSceneInitialized, signalRendererInitializedCorrectly } from 'shared/renderer/actions'
import { browserInterface } from './BrowserInterface'
import { LoadableScene } from 'shared/types'

// eslint-disable-next-line @typescript-eslint/no-var-requires
const hudWorkerRaw = require('../../static/systems/decentraland-ui.scene.js.txt')
const hudWorkerBLOB = new Blob([hudWorkerRaw])
export const hudWorkerUrl = URL.createObjectURL(hudWorkerBLOB)

declare const globalThis: { clientDebug: ClientDebug }

globalThis.clientDebug = clientDebug

////////////////////////////////////////////////////////////////////////////////

/**
 *
 * Common initialization logic for the unity engine
 *
 * @param _gameInstance Unity game instance
 */
export async function initializeEngine(_gameInstance: UnityGame): Promise<void> {
  const gameInstance = traceDecoratorUnityGame(_gameInstance)

  getUnityInstance().Init(gameInstance)

  await browserInterface.startedFuture

  getUnityInstance().ActivateRendering()

  queueMicrotask(() => {
    // send an "engineStarted" notification, use a queueMicrotask
    // to escape the current stack leveraging the JS event loop
    store.dispatch(signalRendererInitializedCorrectly())
  })

  await ensureMetaConfigurationInitialized()

  getUnityInstance().SetKernelConfiguration(kernelConfigForRenderer())

  if (DEBUG) {
    getUnityInstance().SetDebug()
  }

  if (SCENE_DEBUG_PANEL) {
    getUnityInstance().SetKernelConfiguration({ debugConfig: { sceneDebugPanelEnabled: true } })
    getUnityInstance().ShowFPSPanel()
  }

  if (SHOW_FPS_COUNTER) {
    getUnityInstance().ShowFPSPanel()
  }

  if (ENGINE_DEBUG_PANEL) {
    getUnityInstance().SetEngineDebugPanel()
  }

  await startGlobalScene('dcl-gs-avatars', 'Avatars', hudWorkerUrl)

  store.dispatch(avatarSceneInitialized())
}

type GlobalSceneOptions = {
  ecs7?: boolean
  content?: ContentMapping[]
  baseUrl?: string
}
async function startGlobalScene(cid: string, title: string, fileContentUrl: string, options: GlobalSceneOptions = {}) {
  const metadataScene: Scene = {
    display: {
      title: title
    },
    main: 'scene.js',
    scene: {
      base: '0,0',
      parcels: ['0,0']
    }
  }

  const baseUrl = options.baseUrl || location.origin
  const extraContent = options.content || []
  const metadata: LoadableScene['entity']['metadata'] = { ...metadataScene }

  if (!!options.ecs7) {
    metadata.ecs7 = true
  }

  return await loadParcelSceneWorker({
    id: cid,
    baseUrl,
    entity: {
      content: [...extraContent, { file: 'scene.js', hash: fileContentUrl }],
      pointers: [cid],
      timestamp: 0,
      type: EntityType.SCENE,
      metadata,
      version: 'v3'
    },
    isGlobalScene: true,
    // global scenes have no FPS limit
    useFPSThrottling: false
  })
}

export async function getPreviewSceneId(): Promise<{ sceneId: string | null; sceneBase: string }> {
  const result = await fetch(new URL('scene.json?nocache=' + Math.random(), rootURLPreviewMode()).toString())

  if (result.ok) {
    const scene = (await result.json()) as Scene

    const scenes = await fetchScenesByLocation([scene.scene.base])
    if (!scenes.length) throw new Error('cant find scene ' + scene.scene.base)
    return { sceneId: scenes[0].id, sceneBase: scene.scene.base }
  } else {
    throw new Error('Could not load scene.json')
  }
}

export async function loadPreviewScene(message: sdk.Messages) {
  async function oldReload() {
    const { sceneId, sceneBase } = await getPreviewSceneId()
    if (sceneId) {
      await reloadScene(sceneId)
    } else {
      defaultLogger.log(`Unable to load sceneId of ${sceneBase}`)
      debugger
    }
  }

  if (message.type === sdk.SCENE_UPDATE && sdk.SceneUpdate.validate(message)) {
    if (message.payload.sceneType === sdk.ProjectType.PORTABLE_EXPERIENCE) {
      try {
        const { sceneId } = message.payload
        const url = `${rootURLPreviewMode()}/preview-wearables/${sceneId}`
        const collection: { data: any[] } = await (await fetch(url)).json()

        if (!!collection.data.length) {
          const wearable = collection.data[0]

          const entity = await wearableToSceneEntity(wearable, wearable.baseUrl)

          store.dispatch(reloadScenePortableExperience(entity))
        }
      } catch (err) {
        defaultLogger.error(`Unable to loader the preview portable experience`, message, err)
      }
    } else {
      if (message.payload.sceneId) {
        await reloadScene(message.payload.sceneId)
      } else {
        await oldReload()
      }
    }
  } else if (message.type === 'update') {
    defaultLogger.log(`Please update your CLI version to 3.9.0 or more.`, { message })
    await oldReload()
  } else {
    defaultLogger.log(`Unable to process message in loadPreviewScene`, { message })
  }
}

export async function reloadPlaygroundScene() {
  const playgroundCode: string = (globalThis as any).PlaygroundCode
  const playgroundContentMapping: ContentMapping[] = (globalThis as any).PlaygroundContentMapping || []
  const playgroundBaseUrl: string = (globalThis as any).PlaygroundBaseUrl || location.origin

  if (!playgroundCode) {
    console.log('There is no playground code')
    return
  }

  const sceneId = 'dcl-sdk-playground'

  await unloadParcelSceneById(sceneId)
  await sleep(300)

  const hudWorkerBLOB = new Blob([playgroundCode])
  const hudWorkerUrl = URL.createObjectURL(hudWorkerBLOB)
  await startGlobalScene(sceneId, 'SDK Playground', hudWorkerUrl, {
    content: playgroundContentMapping,
    baseUrl: playgroundBaseUrl,
    ecs7: true
  })
}

{
  // TODO: move to unity-renderer
  let isPointerLocked: boolean = false

  function pointerLockChange() {
    const doc: any = document
    const isLocked = !!(doc.pointerLockElement || doc.mozPointerLockElement || doc.webkitPointerLockElement)
    if (isPointerLocked !== isLocked && getUnityInstance()) {
      getUnityInstance().SetCursorState(isLocked)
    }
    isPointerLocked = isLocked
    allScenesEvent({
      eventType: 'onPointerLock',
      payload: {
        locked: isPointerLocked
      }
    })
  }

  document.addEventListener('pointerlockchange', pointerLockChange, false)
}
