import { IEventNames, IEvents } from '../decentraland-ecs/src/decentraland/Types'
import { EventDispatcher } from 'decentraland-rpc/lib/common/core/EventDispatcher'
import { gridToWorld } from '../atomicHelpers/parcelScenePositions'
import { EnvironmentData, EntityAction, LoadableParcelScene } from '../shared/types'
import { SceneWorker, ParcelSceneAPI } from '../shared/world/SceneWorker'
import { ILogger, createLogger } from '../shared/logger'

import EngineInterface from './EngineInterface'
import { DevTools } from '../shared/apis/DevTools'
import { ParcelIdentity } from '../shared/apis/ParcelIdentity'
import { getParcelSceneCID } from '../shared/world/parcelSceneManager'

export class UnityScene<T> implements ParcelSceneAPI {
  eventDispatcher = new EventDispatcher()
  worker!: SceneWorker
  unitySceneId: string
  logger: ILogger

  constructor(public id: string, public data: EnvironmentData<T>, private unityInterface: EngineInterface) {
    this.unitySceneId = id
    this.logger = createLogger(this.unitySceneId + ': ')
  }

  sendBatch(actions: EntityAction[]): void {
    for (let i = 0; i < actions.length; i++) {
      const action = actions[i]
      this.unityInterface.sendSceneMessage(this.unitySceneId, action.type, action.payload)
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

export function getUnityClass(unityInterface: EngineInterface) {
  return class UnityParcelScene extends UnityScene<LoadableParcelScene> {
    constructor(public data: EnvironmentData<LoadableParcelScene>) {
      super(data.data.id, data, unityInterface)
    }

    registerWorker(worker: SceneWorker): void {
      super.registerWorker(worker)

      gridToWorld(this.data.data.basePosition.x, this.data.data.basePosition.y, worker.position)

      this.worker.system
        .then(system => {
          system.getAPIInstance(DevTools).logger = this.logger

          const parcelIdentity = system.getAPIInstance(ParcelIdentity)
          parcelIdentity.land = this.data.data.land
          parcelIdentity.cid = getParcelSceneCID(worker)
        })
        .catch(e => this.logger.error('Error initializing system', e))
    }
  }
}
