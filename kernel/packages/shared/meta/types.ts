import { Vector2Component } from 'atomicHelpers/landHelpers'

export type MetaConfiguration = {
  explorer: {
    minBuildNumber: number
  }
  servers: {
    added: string[]
    denied: string[]
    contentWhitelist: string[]
  }
  world: {
    pois: Vector2Component[]
  }
}

export type MetaState = {
  initialized: boolean
  config: Partial<MetaConfiguration>
}

export type RootMetaState = {
  meta: MetaState
}
