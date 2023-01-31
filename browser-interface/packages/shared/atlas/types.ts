import { Vector2Component } from 'atomicHelpers/landHelpers'

export type AtlasState = {
  hasPois: boolean
  pois: string[] // tiles of POIs of the form `x,y`
  lastReportPosition?: Vector2Component
}

export type RootAtlasState = {
  atlas: AtlasState
}
