declare var window: any

type GameInstance = {
  SendMessage(object: string, method: string, ...args: (number | string)[]): void
}

import { EventDispatcher } from 'decentraland-rpc/lib/common/core/EventDispatcher'

import {
  LoadableParcelScene,
  EntityAction,
  EnvironmentData,
  ILandToLoadableParcelScene,
  IScene,
  MappingsResponse,
  ILand
} from '../shared/types'
import { DevTools } from '../shared/apis/DevTools'
import { gridToWorld } from '../atomicHelpers/parcelScenePositions'
import { ILogger, createLogger, defaultLogger } from '../shared/logger'
import {
  positionObservable,
  lastPlayerPosition,
  getWorldSpawnpoint,
  teleportObservable
} from '../shared/world/positionThings'
import {
  enableParcelSceneLoading,
  getSceneWorkerBySceneID,
  getParcelSceneID,
  stopParcelSceneWorker,
  loadParcelScene
} from '../shared/world/parcelSceneManager'
import { SceneWorker, ParcelSceneAPI, hudWorkerUrl } from '../shared/world/SceneWorker'
import { ensureUiApis } from '../shared/world/uiSceneInitializer'
import { ParcelIdentity } from '../shared/apis/ParcelIdentity'
import { IEventNames, IEvents } from '../decentraland-ecs/src/decentraland/Types'
import { Vector3, Quaternion, ReadOnlyVector3, ReadOnlyQuaternion } from '../decentraland-ecs/src/decentraland/math'
import { DEBUG, ENGINE_DEBUG_PANEL, SCENE_DEBUG_PANEL, parcelLimits, playerConfigurations } from '../config'
import { chatObservable } from '../shared/comms/chat'
import { queueTrackingEvent } from '../shared/analytics'

let gameInstance!: GameInstance

const positionEvent = {
  position: Vector3.Zero(),
  quaternion: Quaternion.Identity,
  rotation: Vector3.Zero(),
  playerHeight: playerConfigurations.height
}

/////////////////////////////////// HANDLERS ///////////////////////////////////

const browserInterface = {
  /** Triggered when the camera moves */
  ReportPosition(data: { position: ReadOnlyVector3; rotation: ReadOnlyQuaternion; playerHeight?: number }) {
    positionEvent.position.set(data.position.x, data.position.y, data.position.z)
    positionEvent.quaternion.set(data.rotation.x, data.rotation.y, data.rotation.z, data.rotation.w)
    positionEvent.rotation.copyFrom(positionEvent.quaternion.eulerAngles)
    positionEvent.playerHeight = data.playerHeight || playerConfigurations.height
    positionObservable.notifyObservers(positionEvent)
  },

  SceneEvent(data: { sceneId: string; eventType: string; payload: any }) {
    const scene = getSceneWorkerBySceneID(data.sceneId)

    if (scene) {
      const parcelScene = scene.parcelScene as UnityParcelScene
      parcelScene.emit(data.eventType as IEventNames, data.payload)
    } else {
      defaultLogger.error(`SceneEvent: Scene ${data.sceneId} not found`, data)
    }
  },

  PreloadFinished(data: { sceneId: string }) {
    // stub. there is no code about this in unity side yet
  }
}

const unityInterface = {
  debug: false,
  SetDebug() {
    gameInstance.SendMessage('SceneController', 'SetDebug')
  },
  CreateUIScene(data: { id: string; baseUrl: string }) {
    /**
     * UI Scenes are scenes that does not check any limit or boundary. The
     * position is fixed at 0,0 and they are universe-wide. An example of this
     * kind of scenes is the Avatar scene. All the avatars are just GLTFs in
     * a scene.
     */
    gameInstance.SendMessage('SceneController', 'CreateUIScene', JSON.stringify(data))
  },
  /** Sends the camera position to the engine */
  SetPosition(x: number, y: number, z: number) {
    let theY = y <= 0 ? 2 : y

    gameInstance.SendMessage('CharacterController', 'SetPosition', JSON.stringify({ x, y: theY, z }))
  },
  /** Tells the engine which scenes to load */
  LoadParcelScenes(parcelsToLoad: LoadableParcelScene[]) {
    if (parcelsToLoad.length > 1) {
      throw new Error('Only one scene at a time!')
    }
    gameInstance.SendMessage('SceneController', 'LoadParcelScenes', JSON.stringify(parcelsToLoad[0]))
  },
  UnloadScene(sceneId: string) {
    gameInstance.SendMessage('SceneController', 'UnloadScene', sceneId)
  },
  SendSceneMessage(parcelSceneId: string, method: string, payload: string, tag: string = '') {
    if (unityInterface.debug) {
      defaultLogger.info(parcelSceneId, method, payload, tag)
    }
    gameInstance.SendMessage(`SceneController`, `SendSceneMessage`, `${parcelSceneId}\t${method}\t${payload}\t${tag}`)
  },

  SetSceneDebugPanel() {
    gameInstance.SendMessage('SceneController', 'SetSceneDebugPanel')
  },

  SetEngineDebugPanel() {
    gameInstance.SendMessage('SceneController', 'SetEngineDebugPanel')
  },

  UnlockCursor() {
    gameInstance.SendMessage('MouseCatcher', 'UnlockCursor')
  }
}

window['unityInterface'] = unityInterface

////////////////////////////////////////////////////////////////////////////////

class UnityScene<T> implements ParcelSceneAPI {
  eventDispatcher = new EventDispatcher()
  worker!: SceneWorker
  logger: ILogger

  constructor(public data: EnvironmentData<T>) {
    this.logger = createLogger(getParcelSceneID(this) + ': ')
  }

  sendBatch(actions: EntityAction[]): void {
    const sceneId = getParcelSceneID(this)
    for (let i = 0; i < actions.length; i++) {
      const action = actions[i]
      unityInterface.SendSceneMessage(sceneId, action.type, action.payload, action.tag)
    }
  }

  registerWorker(worker: SceneWorker): void {
    this.worker = worker
  }

  dispose(): void {
    // TODO: do we need to release some resource after releasing a scene worker?
  }

  on<T extends IEventNames>(event: T, cb: (event: IEvents[T]) => void): void {
    this.eventDispatcher.on(event, cb)
  }

  emit<T extends IEventNames>(event: T, data: IEvents[T]): void {
    this.eventDispatcher.emit(event, data)
  }
}

class UnityParcelScene extends UnityScene<LoadableParcelScene> {
  constructor(public data: EnvironmentData<LoadableParcelScene>) {
    super(data)
    this.logger = createLogger(data.data.basePosition.x + ',' + data.data.basePosition.y + ': ')
  }

  registerWorker(worker: SceneWorker): void {
    super.registerWorker(worker)

    gridToWorld(this.data.data.basePosition.x, this.data.data.basePosition.y, worker.position)

    this.worker.system
      .then(system => {
        system.getAPIInstance(DevTools).logger = this.logger

        const parcelIdentity = system.getAPIInstance(ParcelIdentity)
        parcelIdentity.land = this.data.data.land
        parcelIdentity.cid = getParcelSceneID(worker.parcelScene)
      })
      .catch(e => this.logger.error('Error initializing system', e))
  }
}

////////////////////////////////////////////////////////////////////////////////

export async function initializeEngine(_gameInstance: GameInstance) {
  gameInstance = _gameInstance

  unityInterface.SetPosition(lastPlayerPosition.x, lastPlayerPosition.y, lastPlayerPosition.z)

  if (DEBUG) {
    unityInterface.SetDebug()
  }

  if (SCENE_DEBUG_PANEL) {
    unityInterface.SetSceneDebugPanel()
  }

  if (ENGINE_DEBUG_PANEL) {
    unityInterface.SetEngineDebugPanel()
  }

  await initializeDecentralandUI()

  return {
    unityInterface,
    onMessage(type: string, message: any) {
      if (type in browserInterface) {
        // tslint:disable-next-line:semicolon
        ;(browserInterface as any)[type](message)
      } else {
        defaultLogger.info('MessageFromEngine', type, message)
      }
    }
  }
}

export async function startUnityParcelLoading() {
  await enableParcelSceneLoading({
    parcelSceneClass: UnityParcelScene,
    preloadScene: async _land => {
      // TODO:
      // 1) implement preload call
      // 2) await for preload message or timeout
      // 3) return
    },
    onSpawnpoint: initialLand => {
      const newPosition = getWorldSpawnpoint(initialLand)
      unityInterface.SetPosition(newPosition.x, newPosition.y, newPosition.z)
      queueTrackingEvent('Scene Spawn', { parcel: initialLand.scene.scene.base, spawnpoint: newPosition })
    },
    onLoadParcelScenes: lands => {
      unityInterface.LoadParcelScenes(
        lands.map($ => {
          const x = Object.assign({}, ILandToLoadableParcelScene($).data)
          delete x.land
          return x
        })
      )
    },
    onUnloadParcelScenes: lands => {
      lands.forEach($ => {
        unityInterface.UnloadScene($.sceneId)
      })
    }
  })
}

async function initializeDecentralandUI() {
  const sceneId = 'dcl-ui-scene'

  const scene = new UnityScene({
    sceneId,
    baseUrl: location.origin,
    main: hudWorkerUrl,
    data: {},
    mappings: []
  })

  const worker = loadParcelScene(scene)
  worker.persistent = true

  await ensureUiApis(worker)

  unityInterface.CreateUIScene({ id: getParcelSceneID(scene), baseUrl: scene.data.baseUrl })
}

let currentLoadedScene: SceneWorker

export async function loadPreviewScene() {
  const result = await fetch('/scene.json?nocache=' + Math.random())

  if (currentLoadedScene) {
    stopParcelSceneWorker(currentLoadedScene)
  }

  if (result.ok) {
    // we load the scene to get the metadata
    // about rhe bounds and position of the scene
    // TODO(fmiras): Validate scene according to https://github.com/decentraland/proposals/blob/master/dsp/0020.mediawiki
    const scene = (await result.json()) as IScene
    const mappingsFetch = await fetch('/mappings')
    const mappingsResponse = (await mappingsFetch.json()) as MappingsResponse

    let defaultScene: ILand = {
      sceneId: 'previewScene',
      baseUrl: location.toString().replace(/\?[^\n]+/g, ''),
      scene,
      mappingsResponse: mappingsResponse
    }

    defaultLogger.info('Starting Preview...')
    const parcelScene = new UnityParcelScene(ILandToLoadableParcelScene(defaultScene))
    currentLoadedScene = loadParcelScene(parcelScene)

    const target: LoadableParcelScene = { ...ILandToLoadableParcelScene(defaultScene).data }
    delete target.land

    unityInterface.LoadParcelScenes([target])
  } else {
    throw new Error('Could not load scene.json')
  }
}

teleportObservable.add((position: { x: number; y: number }) => {
  unityInterface.SetPosition(position.x * parcelLimits.parcelSize, 0, position.y * parcelLimits.parcelSize)
})

window['messages'] = (e: any) => chatObservable.notifyObservers(e)

document.addEventListener('pointerlockchange', e => {
  if (!document.pointerLockElement) {
    unityInterface.UnlockCursor()
  }
})
