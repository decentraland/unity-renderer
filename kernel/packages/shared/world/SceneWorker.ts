import { future } from 'fp-future'
import { APIOptions, ScriptingHost } from 'decentraland-rpc/lib/host'
import { ScriptingTransport } from 'decentraland-rpc/lib/common/json-rpc/types'
import { defaultLogger } from 'shared/logger'
import { EnvironmentAPI } from 'shared/apis/EnvironmentAPI'
import { EngineAPI } from 'shared/apis/EngineAPI'
import { Vector3 } from 'decentraland-ecs/src/decentraland/math'
import { IEventNames, IEvents } from 'decentraland-ecs/src'
import { PREVIEW } from 'config'
import { ParcelSceneAPI } from './ParcelSceneAPI'

export abstract class SceneWorker {
  protected engineAPI: EngineAPI | null = null
  private readonly system = future<ScriptingHost>()
  private enabled = true

  constructor(private readonly parcelScene: ParcelSceneAPI, transport: ScriptingTransport) {
    parcelScene.registerWorker(this)

    this.startSystem(transport)
      .then($ => this.system.resolve($))
      .catch($ => this.system.reject($))
  }

  abstract setPosition(position: Vector3): void
  abstract isPersistent(): boolean
  abstract hasSceneStarted(): boolean

  getSceneId(): string {
    return this.parcelScene.data.sceneId
  }

  getParcelScene(): ParcelSceneAPI {
    return this.parcelScene
  }

  emit<T extends IEventNames>(event: T, data: IEvents[T]): void {
    this.parcelScene.emit(event, data)
  }

  getAPIInstance<X>(api: { new(options: APIOptions): X }): Promise<X> {
    return this.system.then(system => system.getAPIInstance(api))
  }

  sendSubscriptionEvent<K extends IEventNames>(event: K, data: IEvents[K]) {
    this.engineAPI?.sendSubscriptionEvent(event, data)
  }

  dispose() {
    if (this.enabled) {
      this.childDispose()
      this.enabled = false

      // Unmount the system
      this.system
        .then(system => {
          try {
            system.unmount()
          } catch (e) {
            defaultLogger.error('Error unmounting system', e)
          }
        })
        .catch(e => defaultLogger.error('Unable to unmount system', e))

      this.parcelScene.dispose()
    }
  }

  protected abstract childDispose(): void

  private async startSystem(transport: ScriptingTransport) {
    const system = await ScriptingHost.fromTransport(transport)

    this.engineAPI = system.getAPIInstance('EngineAPI') as EngineAPI
    this.engineAPI.parcelSceneAPI = this.parcelScene

    system.getAPIInstance(EnvironmentAPI).data = this.parcelScene.data

    // TODO: track this errors using rollbar because this kind of event are usually triggered due to setInterval() or unreliable code in scenes, that is not sandboxed
    system.on('error', (e) => {
      // @ts-ignore
      console['log']('Unloading scene because of unhandled exception in the scene worker: ')

      // @ts-ignore
      console['error'](e)

      // These errors should be handled in development time
      if (PREVIEW) {
        debugger
      }

      transport.close()
    })

    system.enable()

    return system
  }
}
