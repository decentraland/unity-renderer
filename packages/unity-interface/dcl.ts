type GameInstance = { SendMessage(object: string, method: string, ...args: (number | string)[]) }

import { initShared } from '../shared'
import { DevTools } from '../shared/apis/DevTools'
import { ILogger, createLogger } from '../shared/logger'
import { positionObservable, lastPlayerPosition } from '../shared/world/positionThings'
import { enableParcelSceneLoading, getParcelById } from '../shared/world/parcelSceneManager'
import { IEventNames, IEvents } from '../decentraland-ecs/src/decentraland/Types'
import { LoadableParcelScene, EntityAction, EnvironmentData, ILandToLoadableParcelScene } from '../shared/types'
import { SceneWorker, ParcelSceneAPI } from '../shared/world/SceneWorker'

import { EventDispatcher } from 'decentraland-rpc/lib/common/core/EventDispatcher'
import { ParcelIdentity } from '../shared/apis/ParcelIdentity'
import { Vector3, Quaternion, ReadOnlyVector3, ReadOnlyQuaternion } from '../decentraland-ecs/src/decentraland/math'
import { DEBUG } from '../config'

let gameInstance: GameInstance = null
const preloadedScenes = new Set<string>()

const positionEvent = {
  position: Vector3.Zero(),
  quaternion: Quaternion.Identity,
  rotation: Vector3.Zero()
}

/////////////////////////////////// HANDLERS ///////////////////////////////////

const browserInterface = {
  /** Triggered when the camera moves */
  ReportPosition(data: { position: ReadOnlyVector3; rotation: ReadOnlyQuaternion }) {
    positionEvent.position.set(data.position.x, data.position.y, data.position.z)
    positionEvent.quaternion.set(data.rotation.x, data.rotation.y, data.rotation.z, data.rotation.w)
    positionEvent.rotation.copyFrom(positionEvent.quaternion.eulerAngles)
    positionObservable.notifyObservers(positionEvent)
  },

  SceneEvent(data: { sceneId: string; eventType: string; payload: any }) {
    const scene = getParcelById(data.sceneId)
    if (scene) {
      const parcelScene = scene.parcelScene as UnityParcelScene
      parcelScene.emit(data.eventType as IEventNames, data.payload)
    }
  },

  PreloadFinished(data: { sceneId: string }) {
    preloadedScenes.add(data.sceneId)
  }
}

let lastParcelScenesSent = ''

const unityInterface = {
  debug: false,
  SetDebug() {
    gameInstance.SendMessage('SceneController', 'SetDebug')
  },
  /** Sends the camera position to the engine */
  SetPosition(x: number, y: number, z: number) {
    let theY = y <= 0 ? 2 : y

    gameInstance.SendMessage('CharacterController', 'SetPosition', JSON.stringify({ x, y: theY, z }))
  },
  /** Tells the engine which scenes to load */
  LoadParcelScenes(parcelsToLoad: LoadableParcelScene[]) {
    const parcelScenes = JSON.stringify({ parcelsToLoad })
    if (parcelScenes !== lastParcelScenesSent) {
      lastParcelScenesSent = parcelScenes
      gameInstance.SendMessage('SceneController', 'LoadParcelScenes', parcelScenes)
    }
  },
  sendSceneMessage(parcelSceneId: string, method: string, payload: string) {
    if (unityInterface.debug) {
      // tslint:disable-next-line:no-console
      console.log(parcelSceneId, method, payload)
    }
    gameInstance.SendMessage(`SceneController`, `SendSceneMessage`, `${parcelSceneId}\t${method}\t${payload}`)
  }
}

export function finishScenePreload(id: string): void {
  preloadedScenes.add(id)
}

window['unityInterface'] = unityInterface

////////////////////////////////////////////////////////////////////////////////

class UnityParcelScene implements ParcelSceneAPI {
  eventDispatcher = new EventDispatcher()
  worker: SceneWorker
  unitySceneId: string
  logger: ILogger

  constructor(public data: EnvironmentData<LoadableParcelScene>) {
    this.unitySceneId = data.data.id
    this.logger = createLogger(this.unitySceneId + ': ')
  }

  sendBatch(actions: EntityAction[]): void {
    for (let i = 0; i < actions.length; i++) {
      const action = actions[i]
      unityInterface.sendSceneMessage(this.unitySceneId, action.type, action.payload)
    }
  }

  registerWorker(worker: SceneWorker): void {
    this.worker = worker

    this.worker.system
      .then(system => {
        system.getAPIInstance(DevTools).logger = this.logger

        const parcelIdentity = system.getAPIInstance(ParcelIdentity)
        parcelIdentity.land = this.data.data.land
      })
      .catch(e => this.logger.error('Error initializing system', e))
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

////////////////////////////////////////////////////////////////////////////////

export async function initializeEngine(_gameInstance: GameInstance) {
  gameInstance = _gameInstance
  const { net } = await initShared()

  unityInterface.SetPosition(lastPlayerPosition.x, lastPlayerPosition.y, lastPlayerPosition.z)

  if (DEBUG) {
    unityInterface.SetDebug()
  }

  await enableParcelSceneLoading(net, {
    parcelSceneClass: UnityParcelScene,
    shouldLoadParcelScene: land => {
      return true
      // TODO integrate with unity the preloading feature
      // tslint:disable-next-line: no-commented-out-code
      // return preloadedScenes.has(land.scene.scene.base)
    },
    onLoadParcelScenes: scenes => {
      unityInterface.LoadParcelScenes(
        scenes.map($ => {
          const x = Object.assign({}, ILandToLoadableParcelScene($).data)
          delete x.land
          return x
        })
      )
    }
  })

  return {
    onMessage(type: string, message: any) {
      if (type in browserInterface) {
        browserInterface[type](message)
      } else {
        // tslint:disable-next-line:no-console
        console.log('MessageFromEngine', type, message)
      }
    }
  }
}
