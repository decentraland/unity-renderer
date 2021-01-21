import { Vector2Component } from 'atomicHelpers/landHelpers'
import { MinimapSceneInfo } from 'decentraland-ecs/src/decentraland/Types'
import { call, fork, put, select, take, takeEvery, race, takeLatest } from 'redux-saga/effects'
import { parcelLimits } from 'config'
import { fetchSceneJson } from '../../decentraland-loader/lifecycle/utils/fetchSceneJson'
import { fetchSceneIds } from '../../decentraland-loader/lifecycle/utils/fetchSceneIds'
import { getOwnerNameFromJsonData, getSceneDescriptionFromJsonData, getThumbnailUrlFromJsonDataAndContent } from 'shared/selectors'
import defaultLogger from '../logger'
import { lastPlayerPosition } from '../world/positionThings'
import {
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
  INITIALIZE_POI_TILES,
  ReportScenesFromTile,
  reportScenesFromTiles,
  REPORT_SCENES_FROM_TILES
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
import { retrieve, store } from 'shared/cache'
import { getUpdateProfileServer } from 'shared/dao/selectors'
import { Store } from 'redux'
import { RootDaoState } from 'shared/dao/types'

declare const window: {
  unityInterface: {
    UpdateMinimapSceneInformation: (data: MinimapSceneInfo[]) => void
  }
}

const tiles = {
  id: 'tiles',
  url: 'https://api.decentraland.org/v1/tiles',
  build: marketData
}

type MarketplaceConfig = typeof tiles

type CachedMarketplaceTiles = { version: string; data: string }

export function* atlasSaga(): any {
  yield fork(loadMarketplace, tiles)

  yield takeEvery(SCENE_LOAD, checkAndReportAround)

  yield takeLatest(META_CONFIGURATION_INITIALIZED, initializePois)
  yield takeLatest(PARCEL_LOADING_STARTED, reportPois)

  yield takeEvery(QUERY_DATA_FROM_SCENE_JSON, querySceneDataAction)
  yield takeLatest(REPORT_SCENES_AROUND_PARCEL, reportScenesAroundParcelAction)
  yield takeEvery(REPORT_SCENES_FROM_TILES, reportScenesFromTilesAction)
}

function* loadMarketplace(config: MarketplaceConfig) {
  try {
    const cachedKey = `market-${config.id}`

    const cached: CachedMarketplaceTiles | undefined = yield retrieve(cachedKey)

    let data
    if (cached) {
      const currentEtag = yield call(() => fetch(config.url, { method: 'HEAD' }).then((e) => e.headers.get('etag')))

      if (cached.version === currentEtag) {
        data = cached.data
      }
    }

    if (!data) {
      // no cached data or cached does not correspond
      const response: Response = yield call(() => fetch(config.url))
      const etag = response.headers.get('etag')

      data = yield call(() => response.json())

      if (etag) {
        // if we get an etag from the response => cache both etag & data
        yield store(cachedKey, { version: etag, data })
      }
    }

    yield put(config.build(data))
  } catch (e) {
    defaultLogger.error(e)
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

const TRIGGER_DISTANCE = 10 * parcelLimits.parcelSize
const MAX_SCENES_AROUND = 15

function* checkAndReportAround() {
  const userPosition = lastPlayerPosition
  const lastReport: Vector2Component | undefined = yield select((state) => state.atlas.lastReportPosition)

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

  yield put(reportScenesFromTiles(pois))
}

function* reportScenesAroundParcelAction(action: ReportScenesAroundParcel) {
  const tilesAround = getTilesRectFromCenter(action.payload.parcelCoord, action.payload.scenesAround)
  yield put(reportScenesFromTiles(tilesAround))
}

function* initializePois() {
  const pois: Vector2Component[] = yield select(getPois)
  const poiTiles = pois.map((position) => `${position.x},${position.y}`)
  yield put(initializePoiTiles(poiTiles))
}

type stringOrNull = string | null

function* reportScenesFromTilesAction(action: ReportScenesFromTile) {
  while (!(yield select(isMarketDataInitialized))) {
    yield take(MARKET_DATA)
  }

  const tiles = action.payload.tiles
  const result: stringOrNull[] = yield call(fetchSceneIds, tiles)

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
  const store: Store<RootDaoState> = (window as any)['globalStore']
  yield call(waitForPoiTilesInitialization)
  const pois = yield select(getPoiTiles)

  const atlas: AtlasState = yield select((state) => state.atlas)

  const scenes = sceneIds.map((sceneId) => atlas.idToScene[sceneId])

  const minimapSceneInfoResult: MinimapSceneInfo[] = []

  scenes
    .filter((scene) => !scene.alreadyReported)
    .forEach((scene) => {
      const parcels: Vector2Component[] = []
      let isPOI: boolean = false

      scene.sceneJsonData?.scene.parcels.forEach((parcel) => {
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
        previewImageUrl: getThumbnailUrlFromJsonDataAndContent(scene.sceneJsonData, scene.contents, getUpdateProfileServer(store.getState())),
        name: scene.name,
        type: scene.type,
        parcels,
        isPOI
      })
    })

  window.unityInterface.UpdateMinimapSceneInformation(minimapSceneInfoResult)
}
