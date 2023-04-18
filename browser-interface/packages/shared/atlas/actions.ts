import { Vector2 } from 'lib/math/Vector2'
import { action } from 'typesafe-actions'

export const REPORT_SCENES_WORLD_CONTEXT = 'Report scenes in world context'
export const reportScenesWorldContext = (parcelCoord: { x: number; y: number }, rectSizeAround: number) =>
  action(REPORT_SCENES_WORLD_CONTEXT, { parcelCoord, scenesAround: rectSizeAround })
export type ReportScenesWorldContext = ReturnType<typeof reportScenesWorldContext>

export const REPORT_SCENES_AROUND_PARCEL = 'Report scenes around parcel'
export const reportScenesAroundParcel = (parcelCoord: { x: number; y: number }, rectSizeAround: number) =>
  action(REPORT_SCENES_AROUND_PARCEL, { parcelCoord, scenesAround: rectSizeAround })
export type ReportScenesAroundParcel = ReturnType<typeof reportScenesAroundParcel>

export const REPORT_SCENES_FROM_TILES = 'Report scenes from tile'
export const reportScenesFromTiles = (tiles: string[]) => action(REPORT_SCENES_FROM_TILES, { tiles })
export type ReportScenesFromTile = ReturnType<typeof reportScenesFromTiles>

export const REPORTED_SCENES_FOR_MINIMAP = 'Reporting scenes for minimap'
export const reportedScenes = (parcels: string[]) => action(REPORTED_SCENES_FOR_MINIMAP, { parcels })
export type ReportedScenes = ReturnType<typeof reportedScenes>

export const LAST_REPORTED_POSITION = 'Last reported position'
export const reportLastPosition = (position: Vector2) => action(LAST_REPORTED_POSITION, { position })
export type ReportLastPosition = ReturnType<typeof reportLastPosition>

export const INITIALIZE_POI_TILES = 'Initialize POI tiles'
export const initializePoiTiles = (tiles: string[]) => action(INITIALIZE_POI_TILES, { tiles })
export type InitializePoiTiles = ReturnType<typeof initializePoiTiles>

export const SET_HOME_SCENE = 'Set home scene'
export const setHomeScene = (position: string) => action(SET_HOME_SCENE, { position })
export type SetHomeScene = ReturnType<typeof setHomeScene>

export const SEND_HOME_SCENE_TO_UNITY = 'Send home scene to unity'
export const sendHomeScene = (position: string) => action(SEND_HOME_SCENE_TO_UNITY, { position })
export type SendHomeScene = ReturnType<typeof sendHomeScene>
