import { DEBUG, ENGINE_DEBUG_PANEL, SCENE_DEBUG_PANEL } from '../../config'
import { initShared } from '../../shared'
import { lastPlayerPosition, teleportObservable } from '../../shared/world/positionThings'
import { enablePositionReporting, loadedParcelSceneWorkers } from '../../shared/world/parcelSceneManager'
import EngineInterface from '../EngineInterface'
import { UnityGame } from '../types'
import initializeDecentralandUI from '../initializeDecentralandUI'
import browserInterface from '../browserInterface'
import { IScene, MappingsResponse, ILand, ILandToLoadableParcelScene, LoadableParcelScene } from '../../shared/types'
import { SceneWorker } from '../../shared/world/SceneWorker'
import { chatObservable } from '../../shared/comms/chat'
import { getUnityClass } from '../unityParcelScene'

declare var window: Window & {
  unityInterface?: any
  messages?: (e: any) => any
}

export async function initializePreview(gameInstance: UnityGame) {
  const unityInterface = new EngineInterface(gameInstance)
  window.unityInterface = unityInterface

  const net = await initShared()
  unityInterface.SetPosition(lastPlayerPosition.x, lastPlayerPosition.y, lastPlayerPosition.z)

  teleportObservable.add((position: { x: number; y: number }) => {
    unityInterface.SetPosition(position.x, 0, position.y)
  })

  if (DEBUG) {
    unityInterface.SetDebug()
  }

  if (ENGINE_DEBUG_PANEL) {
    unityInterface.SetEngineDebugPanel()
  }

  if (SCENE_DEBUG_PANEL) {
    unityInterface.SetSceneDebugPanel()
  }

  await initializeDecentralandUI(unityInterface)

  enablePositionReporting()
  await loadPreviewScene(unityInterface)

  teleportObservable.add((position: { x: number; y: number }) => {
    unityInterface.SetPosition(position.x, 0, position.y)
  })

  return {
    net,
    loadPreviewScene: () => loadPreviewScene(unityInterface),
    onMessage(type: string, message: any) {
      if (type in browserInterface) {
        // tslint:disable-next-line:semicolon
        ;(browserInterface as any)[type](message)
      } else {
        // tslint:disable-next-line:no-console
        console.log('MessageFromEngine', type, message)
      }
    }
  }
}

async function loadPreviewScene(unityInterface: EngineInterface) {
  const result = await fetch('/scene.json?nocache=' + Math.random())

  loadedParcelSceneWorkers.forEach(worker => {
    if (!worker.persistent) {
      worker.dispose()
      loadedParcelSceneWorkers.delete(worker)
    }
  })

  if (result.ok) {
    // we load the scene to get the metadata
    // about rhe bounds and position of the scene
    // TODO(fmiras): Validate scene according to https://github.com/decentraland/proposals/blob/master/dsp/0020.mediawiki
    const scene = (await result.json()) as IScene
    const mappingsFetch = await fetch('/mappings')
    const mappingsResponse = (await mappingsFetch.json()) as MappingsResponse

    let defaultScene: ILand = {
      baseUrl: location.toString().replace(/\?[^\n]+/g, ''),
      scene,
      mappingsResponse: mappingsResponse
    }

    // tslint:disable-next-line: no-console
    console.log('Starting Preview...')
    const Class = getUnityClass(unityInterface)
    const parcelScene = new Class(ILandToLoadableParcelScene(defaultScene))
    const parcelSceneWorker = new SceneWorker(parcelScene)

    loadedParcelSceneWorkers.add(parcelSceneWorker)

    const target: LoadableParcelScene = { ...ILandToLoadableParcelScene(defaultScene).data }
    delete target.land

    unityInterface.LoadParcelScenes([target])
  } else {
    throw new Error('Could not load scene.json')
  }
}

window['messages'] = (e: any) => chatObservable.notifyObservers(e)
