export type EnvironmentRealm = {
  domain: string
  /** @deprecated use room instead */
  layer: string
  room: string
  serverName: string
  displayName: string
  protocol: string
}

export type ExplorerConfiguration = {
  clientUri: string
  configurations: Record<string, string | number | boolean>
}

export const enum Platform {
  DESKTOP = 'desktop',
  BROWSER = 'browser'
}
