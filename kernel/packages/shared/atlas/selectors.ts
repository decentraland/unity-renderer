import { PlazaNames, RootAtlasState, AtlasState, MapSceneData, MarketData } from './types'
import { getSceneNameFromJsonData } from 'shared/selectors'
import { SceneJsonData } from 'shared/types'

export const EMPTY_PARCEL_NAME = 'Empty parcel'

export function shouldLoadSceneJsonData(state: RootAtlasState, sceneId: string) {
  return !state.atlas.idToScene.hasOwnProperty(sceneId) || state.atlas.idToScene[sceneId].requestStatus !== 'ok'
}

export function isMarketDataInitialized(state: RootAtlasState) {
  return state.atlas.hasMarketData
}

export function getPoiTiles(state: RootAtlasState) {
  return state.atlas.pois
}

export function getType(state: RootAtlasState, x: number, y: number): number {
  const key = `${x},${y}`
  if (!state.atlas.tileToScene[key] || !state.atlas.tileToScene[key].type) {
    return 9
  }
  return state.atlas.tileToScene[key] && state.atlas.tileToScene[key].type
}

export function getMapScene(tileToScene: AtlasState['tileToScene'], x: number, y: number): MapSceneData | undefined {
  return tileToScene[`${x},${y}`]
}

export function getSceneNameFromAtlasState(sceneJsonData?: SceneJsonData): string | undefined {
  if (!sceneJsonData) {
    return undefined
  }

  const sceneJsonName = getSceneNameFromJsonData(sceneJsonData)

  if (sceneJsonName !== 'Unnamed') {
    return sceneJsonName
  }

  return undefined
}

export function getSceneNameFromMarketData(marketData: MarketData, x: number, y: number): string | undefined {
  const key = `${x},${y}`
  const marketEntry = marketData.data[key]

  if (marketEntry) {
    if (marketEntry.name) {
      return marketEntry.name
    }

    if (marketEntry.estate_id !== undefined && PlazaNames[marketEntry.estate_id]) {
      return PlazaNames[marketEntry.estate_id]
    }
  }

  return undefined
}

export function getSceneNameWithMarketAndAtlas(
  marketData: MarketData,
  tileToScene: AtlasState['tileToScene'],
  x: number,
  y: number
): string | undefined {
  let tentativeName: string | undefined

  const mapScene = getMapScene(tileToScene, x, y)

  if (mapScene) {
    tentativeName = getSceneNameFromAtlasState(mapScene.sceneJsonData)
  }

  if (tentativeName === undefined) {
    tentativeName = getSceneNameFromMarketData(marketData, x, y)
  }

  return tentativeName
}

export function postProcessSceneName(name: string | undefined): string {
  if (name === undefined || name === 'interactive-text') {
    return EMPTY_PARCEL_NAME
  }

  if (name.startsWith('Road at')) {
    return 'Road'
  }

  return name
}

export function getSceneTypeFromAtlasState(state: AtlasState, x: number, y: number): number {
  const key = `${x},${y}`
  return state.tileToScene[key] ? state.tileToScene[key].type : 0
}
