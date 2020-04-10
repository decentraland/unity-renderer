import { ILand } from 'shared/types'
import { action } from 'typesafe-actions'
import { Vector2Component } from '../../atomicHelpers/landHelpers'
import { DistrictData, MarketData } from './types'

export const QUERY_DATA_FROM_SCENE_JSON = '[Query] Fetch data from scene.json'
export const querySceneData = (sceneIds: string[]) => action(QUERY_DATA_FROM_SCENE_JSON, sceneIds)
export type QuerySceneData = ReturnType<typeof querySceneData>

export const SUCCESS_DATA_FROM_SCENE_JSON = '[Success] Fetch data from scene.json'
export const fetchDataFromSceneJsonSuccess = (sceneIds: string[], data: ILand[]) =>
  action(SUCCESS_DATA_FROM_SCENE_JSON, { sceneIds, data })
export type FetchDataFromSceneJsonSuccess = ReturnType<typeof fetchDataFromSceneJsonSuccess>

export const FAILURE_DATA_FROM_SCENE_JSON = '[Failure] Fetch data from scene.json'
export const fetchDataFromSceneJsonFailure = (sceneIds: string[], error: any) =>
  action(FAILURE_DATA_FROM_SCENE_JSON, { sceneIds, error })
export type FetchDataFromSceneJsonFailure = ReturnType<typeof fetchDataFromSceneJsonFailure>

export const DISTRICT_DATA = '[Info] District data downloaded'
export const districtData = (districts: DistrictData) => action(DISTRICT_DATA, districts)

export const MARKET_DATA = '[Info] Market data downloaded'
export const marketData = (data: MarketData) => action(MARKET_DATA, data)
export type MarketDataAction = ReturnType<typeof marketData>

export const REPORT_SCENES_AROUND_PARCEL = 'Report scenes around parcel'
export const reportScenesAroundParcel = (parcelCoord: { x: number; y: number }, rectSizeAround: number) =>
  action(REPORT_SCENES_AROUND_PARCEL, { parcelCoord, scenesAround: rectSizeAround })
export type ReportScenesAroundParcel = ReturnType<typeof reportScenesAroundParcel>

export const REPORTED_SCENES_FOR_MINIMAP = 'Reporting scenes for minimap'
export const reportedScenes = (parcels: string[]) => action(REPORTED_SCENES_FOR_MINIMAP, { parcels })
export type ReportedScenes = ReturnType<typeof reportedScenes>

export const LAST_REPORTED_POSITION = 'Last reported position'
export const reportLastPosition = (position: Vector2Component) => action(LAST_REPORTED_POSITION, { position })
export type ReportLastPosition = ReturnType<typeof reportLastPosition>

export const INITIALIZE_POI_TILES = 'Initialize POI tiles'
export const initializePoiTiles = (tiles: string[]) => action(INITIALIZE_POI_TILES, { tiles })
export type InitializePoiTiles = ReturnType<typeof initializePoiTiles>
