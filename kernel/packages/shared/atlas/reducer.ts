import { AnyAction } from 'redux'
import { ILand } from 'shared/types'
import { REPORTED_SCENES_FOR_MINIMAP, FetchDataFromSceneJsonSuccess, QuerySceneData, FetchDataFromSceneJsonFailure, ReportedScenes, QUERY_DATA_FROM_SCENE_JSON, SUCCESS_DATA_FROM_SCENE_JSON, FAILURE_DATA_FROM_SCENE_JSON, MARKET_DATA, LAST_REPORTED_POSITION, DISTRICT_DATA, ReportLastPosition, INITIALIZE_POI_TILES, InitializePoiTiles } from './actions'
import { getSceneNameFromAtlasState, getSceneNameWithMarketAndAtlas, postProcessSceneName } from './selectors'
import {
  AtlasState,
  MapSceneData,
  MarketData
} from './types'

const ATLAS_INITIAL_STATE: AtlasState = {
  hasMarketData: false,
  hasDistrictData: false,
  hasPois: false,
  tileToScene: {}, // '0,0' -> sceneId. Useful for mapping tile market data to actual scenes.
  idToScene: {}, // sceneId -> MapScene
  lastReportPosition: undefined,
  pois: []
}

const MAP_SCENE_DATA_INITIAL_STATE: MapSceneData = {
  sceneId: '',
  name: '',
  type: 0,
  estateId: 0,
  sceneJsonData: undefined,
  alreadyReported: false,
  requestStatus: 'loading'
}

export function atlasReducer(state?: AtlasState, action?: AnyAction) {
  if (!state) {
    return ATLAS_INITIAL_STATE
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case QUERY_DATA_FROM_SCENE_JSON:
      return reduceFetchDataFromSceneJson(state, (action as QuerySceneData).payload)
    case SUCCESS_DATA_FROM_SCENE_JSON:
      return reduceSuccessDataFromSceneJson(state, (action as FetchDataFromSceneJsonSuccess).payload.data)
    case FAILURE_DATA_FROM_SCENE_JSON:
      return reduceFailureDataFromSceneJson(state, (action as FetchDataFromSceneJsonFailure).payload.sceneIds)
    case MARKET_DATA:
      return reduceMarketData(state, action.payload)
    case LAST_REPORTED_POSITION:
      return reduceReportedLastPositionForMinimap(state, (action as ReportLastPosition).payload)
    case REPORTED_SCENES_FOR_MINIMAP:
      return reduceReportedScenesForMinimap(state, (action as ReportedScenes).payload)
    case DISTRICT_DATA:
      return reduceDistrictData(state, action)
    case INITIALIZE_POI_TILES:
      return reduceInitializePoiTiles(state, action as InitializePoiTiles)
  }
  return state
}

function reduceInitializePoiTiles(state: AtlasState, action: InitializePoiTiles) {
  return { ...state, hasPois: true, pois: action.payload.tiles }
}

function reduceFetchDataFromSceneJson(state: AtlasState, sceneIds: string[]) {
  const newScenes = []
  for (const sceneId of sceneIds) {
    if (!state.idToScene[sceneId] || state.idToScene[sceneId].requestStatus !== 'ok') {
      newScenes.push(sceneId)
    }
  }

  if (newScenes.length === 0) {
    return state
  }

  const idToScene = { ...state.idToScene }

  for (const sceneId of newScenes) {
    idToScene[sceneId] = { ...MAP_SCENE_DATA_INITIAL_STATE, requestStatus: 'loading' }
  }

  return { ...state, idToScene }
}

function reduceFailureDataFromSceneJson(state: AtlasState, sceneIds: string[]) {
  const idToScene = { ...state.idToScene }

  for (const sceneId of sceneIds) {
    if (!idToScene[sceneId]) {
      idToScene[sceneId] = { ...MAP_SCENE_DATA_INITIAL_STATE }
    }

    idToScene[sceneId].requestStatus = 'fail'
  }

  return { ...state, idToScene }
}

function reduceSuccessDataFromSceneJson(state: AtlasState, landData: ILand[]) {
  const newData = landData.filter(land => state.idToScene[land.sceneId]?.requestStatus !== 'ok')

  if (newData.length === 0) {
    return state
  }

  const tileToScene = { ...state.tileToScene }
  const idToScene = { ...state.idToScene }

  for (const land of newData) {
    let mapScene: MapSceneData = { ...state.idToScene[land.sceneId] }

    land.sceneJsonData.scene.parcels.forEach(x => {
      const scene = tileToScene[x]
      if (scene) {
        mapScene = {
          ...mapScene,
          name: scene.name,
          type: scene.type,
          estateId: scene.estateId
        }
      }
    })

    mapScene.requestStatus = 'ok'
    mapScene.sceneJsonData = land.sceneJsonData

    const name = getSceneNameFromAtlasState(mapScene.sceneJsonData) ?? mapScene.name
    mapScene.name = postProcessSceneName(name)

    mapScene.sceneJsonData.scene.parcels.forEach(x => {
      tileToScene[x] = mapScene
    })

    idToScene[land.sceneId] = mapScene
  }

  return { ...state, tileToScene, idToScene }
}

function reduceDistrictData(state: AtlasState, action: AnyAction) {
  return { ...state, hasDistrictData: true }
}

function reduceReportedLastPositionForMinimap(state: AtlasState, payload: ReportLastPosition['payload']) {
  return {
    ...state,
    lastReportPosition: payload.position
  }
}

function reduceReportedScenesForMinimap(
  state: AtlasState,
  payload: ReportedScenes['payload']
) {
  const tileToScene = { ...state.tileToScene }
  const idToScene = { ...state.idToScene }

  payload.parcels.forEach(x => {
    const mapScene = tileToScene[x]
    if (mapScene) {
      mapScene.alreadyReported = true
      if (mapScene.sceneId) {
        idToScene[mapScene.sceneId].alreadyReported = true
      }
    }
  })

  return {
    ...state,
    tileToScene
  }
}

function reduceMarketData(state: AtlasState, marketData: MarketData) {
  const tileToScene = { ...state.tileToScene }

  Object.keys(marketData.data).forEach(key => {
    const existingScene = tileToScene[key]

    const value = marketData.data[key]

    const sceneName = postProcessSceneName(getSceneNameWithMarketAndAtlas(marketData, tileToScene, value.x, value.y))

    let newScene: MapSceneData

    if (existingScene) {
      newScene = {
        ...existingScene,
        name: sceneName,
        type: value.type,
        estateId: value.estate_id
      }
    } else {
      newScene = {
        sceneId: '',
        name: sceneName,
        type: value.type,
        estateId: value.estate_id,
        sceneJsonData: undefined,
        alreadyReported: false,
        requestStatus: undefined
      }
    }

    tileToScene[key] = newScene
  })

  return { ...state, tileToScene, hasMarketData: true }
}
