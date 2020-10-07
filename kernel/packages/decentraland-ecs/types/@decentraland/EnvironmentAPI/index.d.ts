declare module '@decentraland/EnvironmentAPI' {
  export type Realm = {
    domain: string
    layer: string
    serverName: string
    displayName: string
  }
  /**
   * Returns the current connected realm
   */
  export function getCurrentRealm(): Promise<Realm | undefined>

  /**
   * Returns whether the scene is running in preview mode or not
   */
  export function isPreviewMode(): Promise<boolean>
}
