import { Vector2Component } from 'atomicHelpers/landHelpers'
import { MinimapSceneInfo } from 'decentraland-ecs/src/decentraland/Types'
import { call, fork, put, select, take, takeEvery, race, takeLatest } from 'redux-saga/effects'
import { parcelLimits } from '../../config'
import { getServer, LifecycleManager } from '../../decentraland-loader/lifecycle/manager'
import { getOwnerNameFromJsonData, getSceneDescriptionFromJsonData } from '../../shared/selectors'
import defaultLogger from '../logger'
import { lastPlayerPosition } from '../world/positionThings'
import {
  districtData,
  fetchDataFromSceneJsonFailure,
  fetchDataFromSceneJsonSuccess,
  marketData,
  querySceneData,
  QuerySceneData,
  ReportScenesAroundParcel,
  reportedScenes,
  QUERY_DATA_FROM_SCENE_JSON,
  REPORT_SCENES_AROUND_PARCEL,
  MARKET_DATA,
  SUCCESS_DATA_FROM_SCENE_JSON,
  FAILURE_DATA_FROM_SCENE_JSON,
  reportScenesAroundParcel,
  reportLastPosition,
  initializePoiTiles,
  INITIALIZE_POI_TILES
} from './actions'
import { shouldLoadSceneJsonData, isMarketDataInitialized, getPoiTiles } from './selectors'
import { AtlasState, RootAtlasState } from './types'
import { getTilesRectFromCenter } from '../getTilesRectFromCenter'
import { ILand } from 'shared/types'
import { SCENE_LOAD } from 'shared/loading/actions'
import { worldToGrid } from '../../atomicHelpers/parcelScenePositions'
import { PARCEL_LOADING_STARTED } from 'shared/renderer/types'
import { getPois } from '../meta/selectors'
import { META_CONFIGURATION_INITIALIZED } from '../meta/actions'

declare const window: {
  unityInterface: {
    UpdateMinimapSceneInformation: (data: MinimapSceneInfo[]) => void
  }
}

export function* atlasSaga(): any {
  yield fork(fetchDistricts)
  yield fork(fetchTiles)

  yield takeEvery(SCENE_LOAD, checkAndReportAround)

  yield takeLatest(META_CONFIGURATION_INITIALIZED, initializePois)
  yield takeLatest(PARCEL_LOADING_STARTED, reportPois)

  yield takeEvery(QUERY_DATA_FROM_SCENE_JSON, querySceneDataAction)
  yield takeLatest(REPORT_SCENES_AROUND_PARCEL, reportScenesAroundParcelAction)
}

function* fetchDistricts() {
  try {
    const districts = yield call(() => fetch('https://api.decentraland.org/v1/districts').then(e => e.json()))
    yield put(districtData(districts))
  } catch (e) {
    defaultLogger.log(e)
  }
}

function* fetchTiles() {
  try {
    const tiles = yield call(() => fetch('https://api.decentraland.org/v1/tiles').then(e => e.json()))
    yield put(marketData(tiles))
  } catch (e) {
    defaultLogger.log(e)
  }
}

function* querySceneDataAction(action: QuerySceneData) {
  const sceneIds = action.payload
  try {
    const lands: ILand[] = yield call(fetchSceneJson, sceneIds)
    yield put(fetchDataFromSceneJsonSuccess(sceneIds, lands))
  } catch (e) {
    yield put(fetchDataFromSceneJsonFailure(sceneIds, e))
  }
}

async function fetchSceneJson(sceneIds: string[]) {
  const server: LifecycleManager = getServer()
  const lands = await Promise.all(sceneIds.map(sceneId => server.getParcelData(sceneId)))
  return lands
}

async function fetchSceneIds(tiles: string[]) {
  const server: LifecycleManager = getServer()
  const promises = server.getSceneIds(tiles)
  return Promise.all(promises)
}

const TRIGGER_DISTANCE = 10 * parcelLimits.parcelSize
const MAX_SCENES_AROUND = 15

function* checkAndReportAround() {
  const userPosition = lastPlayerPosition
  const lastReport: Vector2Component | undefined = yield select(state => state.atlas.lastReportPosition)

  if (
    !lastReport ||
    Math.abs(userPosition.x - lastReport.x) > TRIGGER_DISTANCE ||
    Math.abs(userPosition.z - lastReport.y) > TRIGGER_DISTANCE
  ) {
    const gridPosition = worldToGrid(userPosition)

    yield put(reportScenesAroundParcel(gridPosition, MAX_SCENES_AROUND))
    yield put(reportLastPosition({ x: userPosition.x, y: userPosition.z }))
  }
}

function* waitForPoiTilesInitialization() {
  while (!(yield select((state: RootAtlasState) => state.atlas.hasPois))) {
    yield take(INITIALIZE_POI_TILES)
  }
}

function* reportPois() {
  yield call(waitForPoiTilesInitialization)

  const pois: string[] = yield select(getPoiTiles)

  yield call(reportScenesFromTiles, pois)
}

function* reportScenesAroundParcelAction(action: ReportScenesAroundParcel) {
  const tilesAround = getTilesRectFromCenter(action.payload.parcelCoord, MAX_SCENES_AROUND)
  yield call(reportScenesFromTiles, tilesAround)
}

function* initializePois() {
  const pois: Vector2Component[] = yield select(getPois)
  const poiTiles = pois.map(position => `${position.x},${position.y}`)
  yield put(initializePoiTiles(poiTiles))
}

function* reportScenesFromTiles(tiles: string[]) {
  while (!(yield select(isMarketDataInitialized))) {
    yield take(MARKET_DATA)
  }

  const result: (string | null)[] = yield call(fetchSceneIds, tiles)

  // filter non null & distinct
  const sceneIds = result.filter((e, i) => e !== null && result.indexOf(e) === i) as string[]

  yield put(querySceneData(sceneIds))

  for (const id of sceneIds) {
    const shouldFetch = yield select(shouldLoadSceneJsonData, id)
    if (shouldFetch) {
      yield race({
        success: take(SUCCESS_DATA_FROM_SCENE_JSON),
        failure: take(FAILURE_DATA_FROM_SCENE_JSON)
      })
    }
  }

  yield call(reportScenes, sceneIds)
  yield put(reportedScenes(tiles))
}

function* reportScenes(sceneIds: string[]): any {
  yield call(waitForPoiTilesInitialization)
  const pois = yield select(getPoiTiles)

  const atlas: AtlasState = yield select(state => state.atlas)

  const scenes = sceneIds.map(sceneId => atlas.idToScene[sceneId])

  const minimapSceneInfoResult: MinimapSceneInfo[] = []

  scenes
    .filter(scene => !scene.alreadyReported)
    .forEach(scene => {
      const parcels: Vector2Component[] = []
      let isPOI: boolean = false

      scene.sceneJsonData?.scene.parcels.forEach(parcel => {
        let xyStr = parcel.split(',')
        let xy: Vector2Component = { x: parseInt(xyStr[0], 10), y: parseInt(xyStr[1], 10) }

        if (pois.includes(parcel)) {
          isPOI = true
        }

        parcels.push(xy)
      })

      minimapSceneInfoResult.push({
        owner: getOwnerNameFromJsonData(scene.sceneJsonData),
        description: getSceneDescriptionFromJsonData(scene.sceneJsonData),
        previewImageUrl: scene.sceneJsonData?.display?.navmapThumbnail,
        name: scene.name,
        type: scene.type,
        parcels,
        isPOI
      })
    })

  window.unityInterface.UpdateMinimapSceneInformation(minimapSceneInfoResult)
}
