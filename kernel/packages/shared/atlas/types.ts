import { Vector2Component } from 'atomicHelpers/landHelpers'
import { SceneJsonData } from 'shared/types'

export type AtlasState = {
  hasMarketData: boolean
  hasDistrictData: boolean
  hasPois: boolean

  pois: string[] // tiles of POIs of the form `x,y`
  tileToScene: Record<string, MapSceneData> // '0,0' -> sceneId. Useful for mapping tile market data to actual scenes.
  idToScene: Record<string, MapSceneData> // sceneId -> MapScene
  lastReportPosition?: Vector2Component
}

export type MapSceneData = {
  sceneId: string
  name: string
  type: number
  estateId?: number
  sceneJsonData?: SceneJsonData
  alreadyReported: boolean
  requestStatus: undefined | 'loading' | 'ok' | 'fail'
}

export type RootAtlasState = {
  atlas: AtlasState
}

export type DistrictData = {
  ok: boolean
  data: District[]
}

export type District = {
  id: string
  name: string
}

export type MarketData = {
  ok: boolean
  data: Record<string, MarketEntry>
}

export type MarketEntry = {
  x: number
  y: number
  name: string
  estate_id?: number
  type: number
}

export const PlazaNames: { [key: number]: string } = {
  1134: 'Vegas Plaza',
  1092: 'Forest Plaza',
  1094: 'CyberPunk Plaza',
  1132: 'Soho Plaza',
  1096: 'Medieval Plaza',
  1130: 'Gamer Plaza',
  1127: 'SciFi Plaza',
  1112: 'Asian Plaza',
  1164: 'Genesis Plaza',
  1825: 'Roads',
  1813: 'Roads',
  1815: 'Roads',
  1827: 'Roads',
  1824: 'Roads',
  1820: 'Roads',
  1924: 'Roads',
  1925: 'Roads',
  1830: 'Roads',
  1831: 'Roads',
  1832: 'Roads'
}
