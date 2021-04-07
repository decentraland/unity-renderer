import { EntityAction } from "shared/types"

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

  /**
   * Start signal, sent after everything was initialized
   */
  startSignal(): Promise<void>

  /** Event handler for subscription events */
  onSubscribedEvent(fn: any): void
}
