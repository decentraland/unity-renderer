export type EnvironmentRealm = {
  domain: string
  /** @deprecated use room instead */
  layer: string
  room: string
  serverName: string
  displayName: string
  protocol: string
}

export const enum Platform {
  DESKTOP = 'desktop',
  BROWSER = 'browser'
}
