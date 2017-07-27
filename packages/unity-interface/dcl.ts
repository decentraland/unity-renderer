type GameInstance = { SendMessage(object: string, method: string, ...args: (number | string)[]) }

import { initShared } from '../shared'
import { positionObserver, lastPlayerPosition } from '../shared/world/positionThings'
import { Vector3, Quaternion } from 'babylonjs'
import { enableParcelSceneLoading, getParcelById } from '../shared/world/parcelSceneManager'
import {
  LoadableParcelScene,
  Vector3Component,
  QuaternionComponent,
  EntityAction,
  EnvironmentData,
  ILandToLoadableParcelScene
} from '../shared/types'
import { SceneWorker, ParcelSceneAPI } from '../shared/world/SceneWorker'
import { IEventNames, IEvents } from '../shared/events'

let gameInstance: GameInstance = null

const positionEvent = {
  position: Vector3.Zero(),
  quaternion: Quaternion.Identity(),
  rotation: Vector3.Zero()
}

/////////////////////////////////// HANDLERS ///////////////////////////////////

const browserInterface = {
  /** Triggered when the camera moves */
  ReportPosition(data: { position: Vector3Component; rotation: QuaternionComponent }) {
    positionEvent.position.set(data.position.x, data.position.y, data.position.z)
    positionEvent.quaternion.set(data.rotation.x, data.rotation.y, data.rotation.z, data.rotation.w)
    positionEvent.quaternion.toEulerAnglesToRef(positionEvent.rotation)

    positionObserver.notifyObservers(positionEvent)
  },

  SceneEvent(data: { sceneId: string; eventType: string; payload: any }) {
    const scene = getParcelById(data.sceneId)
    if (scene) {
      // send message to scene.parcelScene as UnityParcelScene
    }
  }
}

const unityInterface = {
  debug: false,
  /** Sends the camera position to the engine */
  SetPosition(x: number, y: number, z: number) {
    gameInstance.SendMessage('CharacterController', 'SetPosition', JSON.stringify({ x, y, z }))
  },
  /** Tells the engine which scenes to load */
  LoadParcelScenes(parcelsToLoad: LoadableParcelScene[]) {
    gameInstance.SendMessage('SceneController', 'LoadParcelScenes', JSON.stringify({ parcelsToLoad }))
  },
  sendSceneMessage(parcelSceneId: string, method: string, payload: string) {
    if (unityInterface.debug) {
      // tslint:disable-next-line:no-console
      console.log(parcelSceneId, method, payload)
    }
    gameInstance.SendMessage(`SceneController`, `SendSceneMessage`, `${parcelSceneId}\t${method}\t${payload}`)
  }
}

window['unityInterface'] = unityInterface

////////////////////////////////////////////////////////////////////////////////

class UnityParcelScene implements ParcelSceneAPI {
  constructor(public data: EnvironmentData<LoadableParcelScene>) {
    // stub
  }

  sendBatch(ctions: EntityAction[]): void {
    throw new Error('Method not implemented.')
  }

  registerWorker(event: SceneWorker): void {
    throw new Error('Method not implemented.')
  }

  dispose(): void {
    throw new Error('Method not implemented.')
  }

  on<T extends IEventNames>(event: T, cb: (event: IEvents[T]) => void): void {
    throw new Error('Method not implemented.')
  }
}

////////////////////////////////////////////////////////////////////////////////

export async function initializeEngine(_gameInstance: GameInstance) {
  gameInstance = _gameInstance
  const { net } = await initShared()

  unityInterface.SetPosition(lastPlayerPosition.x, lastPlayerPosition.y, lastPlayerPosition.z)

  await enableParcelSceneLoading(net, {
    parcelSceneClass: UnityParcelScene,
    onLoadParcelScenes: scenes => {
      unityInterface.LoadParcelScenes(scenes.map($ => ILandToLoadableParcelScene($).data))
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
