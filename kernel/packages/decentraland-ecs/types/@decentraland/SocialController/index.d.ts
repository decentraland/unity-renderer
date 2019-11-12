declare module '@decentraland/SocialController' {
  /**
   * Adds user to list of muted users (stored in localStorage)
   * @param user
   */
  export function mute(user: string): void

  /**
   * Removes user from list of muted users (stored in localStorage)
   * @param user
   */
  export function unmute(user: string): void

  /**
   * Adds user to list of blocked users (stored in localStorage)
   * @param user
   */
  export function block(user: string): void

  /**
   * Removes user from list of blocked users (stored in localStorage)
   * @param user
   */
  export function unblock(user: string): void

  /**
   * Gets a list of blocked users
   */
  export function getBlockedUsers(): Promise<string[]>

  /**
   * Gets a list of muted users
   */
  export function getMutedUsers(): Promise<string[]>

  /**
   * Subscribes to events dispatched by the EngineAPI
   * Use it to listen to events from the scene (like `click`)
   * @param event
   */
  export function subscribe(event: string): Promise<void>

  /**
   * Removes a subscription to an event
   * @param event
   */
  export function unsubscribe(event: string): Promise<void>

  /** Event handler for subscription events */
  export function onSubscribedEvent(fn: any): void
}
