import { action } from 'typesafe-actions'
import {
  DistrictData,
  DISTRICT_DATA,
  FAILURE_NAME_FROM_SCENE_JSON,
  FETCH_NAME_FROM_SCENE_JSON,
  MarketData,
  MARKET_DATA,
  QUERY_NAME_FROM_SCENE_JSON,
  SUCCESS_NAME_FROM_SCENE_JSON
} from './types'
import { Vector2Component } from '../../atomicHelpers/landHelpers'

export const querySceneName = (scene: string) => action(QUERY_NAME_FROM_SCENE_JSON, scene)
export type QuerySceneName = ReturnType<typeof querySceneName>

export const fetchNameFromSceneJson = (scene: string) => action(FETCH_NAME_FROM_SCENE_JSON, scene)
export const fetchNameFromSceneJsonSuccess = (scene: string, name: string, parcels: string[]) =>
  action(SUCCESS_NAME_FROM_SCENE_JSON, { sceneId: scene, name, parcels })
export const fetchNameFromSceneJsonFailure = (scene: string, e: any) =>
  action(FAILURE_NAME_FROM_SCENE_JSON, { sceneId: scene, error: e })
export type FetchNameFromSceneJsonSuccess = ReturnType<typeof fetchNameFromSceneJsonSuccess>

export const districtData = (districts: DistrictData) => action(DISTRICT_DATA, districts)
export const marketData = (data: MarketData) => action(MARKET_DATA, data)
export type MarketDataAction = ReturnType<typeof marketData>

export const REPORTED_SCENES_FOR_MINIMAP = 'Reporting scenes for minimap'
export const reportedScenes = (parcels: string[], reportPosition?: Vector2Component) =>
  action(REPORTED_SCENES_FOR_MINIMAP, { parcels, reportPosition })
