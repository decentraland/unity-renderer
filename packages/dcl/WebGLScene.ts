import { EntityAction, EnvironmentData } from 'shared/types'
import { SceneWorker, ParcelSceneAPI } from 'shared/world/SceneWorker'
import { SharedSceneContext } from 'engine/entities/SharedSceneContext'
import { defaultLogger, ILogger } from 'shared/logger'
import { DevTools } from 'shared/apis/DevTools'
import { IEvents, IEventNames } from 'decentraland-ecs/src/decentraland/Types'

/**
 * The WebGLScene has the responsibility of communicating the SceneWorker with the SharedSceneContext
 * That involves:
 * - Sending messages to the worker (i.e. events)
 * - Receive messages messages from the worker (i.e. create an entity)
 * The WebGLScene must be compliant with the ParcelSceneAPI
 */
export class WebGLScene<T> implements ParcelSceneAPI {
  public worker: SceneWorker | null = null

  public logger: ILogger = defaultLogger
  public disposed: boolean = false

  constructor(public data: EnvironmentData<T>, public context: SharedSceneContext) {
    this.logger = this.context.logger
  }

  // TODO: ECS, figure out how to send metrics and who should handle it
  sendBatch(actions: EntityAction[]): void {
    if (!this.disposed) {
      for (let i = 0; i < actions.length; i++) {
        const action = actions[i]
        const fn = this.context[action.type] as ((x: any) => void)
        fn.call(this.context, JSON.parse(action.payload))
      }
    }
  }

  registerWorker(worker: SceneWorker): void {
    this.worker = worker

    worker.system
      .then(system => {
        system.getAPIInstance(DevTools).logger = this.logger
      })
      .catch($ => this.logger.error('registerWorker', $))
  }

  dispose(): void {
    this.disposed = true

    if (!this.context) {
      return this.logger.error('WebGLScene is already disposed')
    }

    this.context.dispose()
    delete this.context
  }

  on<T extends IEventNames>(eventName: T, fn: (event: IEvents[T]) => void): void {
    // TODO: agus revisa esto
    if (!this.disposed) {
      this.context.on(eventName, fn)
    }
  }
}
