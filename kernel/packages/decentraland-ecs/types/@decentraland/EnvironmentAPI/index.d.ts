declare module '@decentraland/EnvironmentAPI' {
  type Realm = {
    domain: string
    layer: string
    serverName: string
    displayName: string
  }
  /**
   * Returns the current connected realm
   */
  export function getCurrentRealm(): Promise<Realm | undefined>
}
