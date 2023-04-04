import type { Vector2 } from 'lib/math/Vector2'

export type AtlasState = {
  hasPois: boolean
  pois: string[] // tiles of POIs of the form `x,y`
  lastReportPosition?: Vector2
}

export type RootAtlasState = {
  atlas: AtlasState
}
