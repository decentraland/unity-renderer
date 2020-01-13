import { Vector2Component } from 'atomicHelpers/landHelpers'

export const UPDATE_MINIMAP_SCENE_NAME = 'Update tile name'
export const QUERY_NAME_FROM_SCENE_JSON = '[Query] Fetch name from scene.json'
export const FETCH_NAME_FROM_SCENE_JSON = '[Request] Fetch name from scene.json'
export const SUCCESS_NAME_FROM_SCENE_JSON = '[Success] Fetch name from scene.json'
export const FAILURE_NAME_FROM_SCENE_JSON = '[Failure] Fetch name from scene.json'
export const DISTRICT_DATA = '[Info] District data downloaded'
export const MARKET_DATA = '[Info] Market data downloaded'

export type AtlasState = {
  marketName: Record<string, MarketEntry>
  districtName: Record<string, string>
  sceneNames: Record<string, string>
  requestStatus: Record<string, undefined | 'loading' | 'ok' | 'fail'>
  alreadyReported: Record<string, boolean>
  lastReportPosition?: Vector2Component
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
  estate_id: number
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
