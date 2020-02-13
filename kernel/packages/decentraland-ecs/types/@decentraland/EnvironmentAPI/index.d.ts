declare module '@decentraland/EnvironmentAPI' {
  export type Realm = {
    domain: string
    catalystName: string
    layer: string
  }
  /**
   * Returns the current connected realm
   */
  export function getCurrentRealm(): Promise<Realm | undefined>
}
