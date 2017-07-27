import { registerAPI, exposeMethod, APIOptions } from 'decentraland-rpc/lib/host'

import { ExposableAPI } from './ExposableAPI'
import { EntityAction } from '../types'
import { IEventNames, IEvents } from 'shared/events'
import { ParcelSceneAPI } from 'shared/world/SceneWorker'

export interface IEngineAPI {
  /**
   * Subscribes to events dispatched by the EngineAPI
   * Use it to listen to events from the scene (like `click`)
   * @param event
   */
  subscribe(event: string): Promise<void>

  /**
   * Removes a subscription to an event
   * @param event
   */
  unsubscribe(event: string): Promise<void>

  /**
   * Send batch
   * @param batch
   */
  sendBatch(actions: EntityAction[]): Promise<void>

  /** Event handler for subscription events */
  onSubscribedEvent(fn: any): void
}

@registerAPI('EngineAPI')
export class EngineAPI extends ExposableAPI implements IEngineAPI {
  parcelSceneAPI!: ParcelSceneAPI

  // this dictionary contains the list of subscriptions.
  // the boolean value indicates if the client is actively
  // listenting to that event
  subscribedEvents: { [event: string]: boolean } = {}

  constructor(options: APIOptions) {
    super(options)
  }

  @exposeMethod
  async subscribe(event: IEventNames) {
    if (typeof (event as any) === 'string') {
      if (!(event in this.subscribedEvents)) {
        this.parcelSceneAPI.on(event, (data: any) => {
          if (this.subscribedEvents[event]) {
            this.sendSubscriptionEvent(event, data)
          }
        })
      }
      this.subscribedEvents[event] = true
    }
  }

  @exposeMethod
  async unsubscribe(event: string) {
    if (typeof (event as any) === 'string') {
      this.subscribedEvents[event] = false
    }
  }

  @exposeMethod
  async sendBatch(actions: EntityAction[]): Promise<void> {
    this.parcelSceneAPI.sendBatch(actions)
  }

  // TODO: add getAttributes so we can load scenes

  sendSubscriptionEvent<K extends IEventNames>(event: K, data: IEvents[K]) {
    if (this.subscribedEvents[event]) {
      this.options.notify('SubscribedEvent', {
        event,
        data
      })
    }
  }

  /**
   * Releases the resources of the rendered scene
   */
  apiWillUnmount() {
    // stub
  }

  onSubscribedEvent(fn: any): void {
    // stub, we implement this function here to fulfill the interface of EntityController
  }
}
