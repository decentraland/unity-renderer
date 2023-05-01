import { Scene } from '@dcl/schemas'
import { saveToPersistentStorage } from 'lib/browser/persistentStorage'
import { getTilesRectFromCenter } from 'lib/decentraland/parcels/getTilesRectFromCenter'
import { parcelSize } from 'lib/decentraland/parcels/limits'
import { parseParcelPosition } from 'lib/decentraland/parcels/parseParcelPosition'
import { worldToGrid } from 'lib/decentraland/parcels/worldToGrid'
import defaultLogger from 'lib/logger'
import { Vector2 } from 'lib/math/Vector2'
import { waitFor } from 'lib/redux'
import { call, put, select, takeEvery, takeLatest } from 'redux-saga/effects'
import { trackEvent } from 'shared/analytics/trackEvent'
import { getContentService, getPOIService } from 'shared/dao/selectors'
import { SCENE_LOAD } from 'shared/loading/actions'
import { waitForRealm } from 'shared/realm/waitForRealmAdapter'
import { waitForRendererInstance } from 'shared/renderer/sagas-helper'
import { PARCEL_LOADING_STARTED } from 'shared/renderer/types'
import { fetchActiveSceneInWorldContext, fetchScenesByLocation } from 'shared/scene-loader/sagas'
import { getThumbnailUrlFromJsonDataAndContent } from 'lib/decentraland/sceneJson/getThumbnailUrlFromJsonDataAndContent'
import { getOwnerNameFromJsonData } from 'lib/decentraland/sceneJson/getOwnerNameFromJsonData'
import { getSceneDescriptionFromJsonData } from 'lib/decentraland/sceneJson/getSceneDescriptionFromJsonData'
import { store } from 'shared/store/isolatedStore'
import { LoadableScene } from 'shared/types'
import { getUnityInstance, MinimapSceneInfo } from 'unity-interface/IUnityInterface'
import { META_CONFIGURATION_INITIALIZED } from '../meta/actions'
import { lastPlayerPosition } from '../world/positionThings'
import {
  initializePoiTiles,
  INITIALIZE_POI_TILES,
  reportedScenes,
  reportLastPosition,
  ReportScenesAroundParcel,
  reportScenesAroundParcel,
  ReportScenesFromTile,
  reportScenesFromTiles,
  REPORT_SCENES_AROUND_PARCEL,
  REPORT_SCENES_FROM_TILES,
  SendHomeScene,
  sendHomeScene,
  SEND_HOME_SCENE_TO_UNITY,
  SetHomeScene,
  SET_HOME_SCENE,
  REPORT_SCENES_WORLD_CONTEXT,
  ReportScenesWorldContext
} from './actions'
import { getPoiTiles, postProcessSceneName } from './selectors'
import { RootAtlasState } from './types'
import { homePointKey } from './utils'
import { Entity } from '@dcl/schemas'

export function* atlasSaga(): any {
  yield takeEvery(SCENE_LOAD, checkAndReportAround)

  yield takeLatest(META_CONFIGURATION_INITIALIZED, initializePois)
  yield takeLatest(PARCEL_LOADING_STARTED, reportPois)

  yield takeLatest(REPORT_SCENES_AROUND_PARCEL, reportScenesAroundParcelAction)
  yield takeLatest(REPORT_SCENES_WORLD_CONTEXT, reportScenesWorldContext)
  yield takeEvery(REPORT_SCENES_FROM_TILES, reportScenesFromTilesAction)
  yield takeEvery(SET_HOME_SCENE, setHomeSceneAction)
  yield takeEvery(SEND_HOME_SCENE_TO_UNITY, sendHomeSceneToUnityAction)
}

const TRIGGER_DISTANCE = 10 * parcelSize
const MAX_SCENES_AROUND = 15

function* checkAndReportAround() {
  const userPosition = lastPlayerPosition
  const lastReport: Vector2 | undefined = yield select((state) => state.atlas.lastReportPosition)

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

function atlasHasPois(state: RootAtlasState) {
  return !!state.atlas.hasPois
}
const waitForPoiTilesInitialization = waitFor(atlasHasPois, INITIALIZE_POI_TILES)

function* reportPois() {
  yield call(waitForPoiTilesInitialization)

  const pois: string[] = yield select(getPoiTiles)

  yield put(reportScenesFromTiles(pois))
}

function* reportScenesAroundParcelAction(action: ReportScenesAroundParcel) {
  const tilesAround = getTilesRectFromCenter(action.payload.parcelCoord, action.payload.scenesAround)
  yield put(reportScenesFromTiles(tilesAround))
}

function* reportScenesWorldContext(action: ReportScenesWorldContext) {
  yield call(waitForPoiTilesInitialization)
  const pois = yield select(getPoiTiles)

  const scenesResponse: Array<Entity> = yield call(
    fetchActiveSceneInWorldContext,
    getTilesRectFromCenter(action.payload.parcelCoord, action.payload.scenesAround)
  )
  const minimapSceneInfoResult: MinimapSceneInfo[] = []
  scenesResponse.forEach((scene) => {
    const parcels: Vector2[] = []
    let isPOI: boolean = false

    const metadata: Scene | undefined = scene.metadata

    if (metadata) {
      let sceneName = metadata.display?.title || ''

      metadata.scene.parcels.forEach((parcel) => {
        const xy: Vector2 = parseParcelPosition(parcel)

        if (pois.includes(parcel)) {
          isPOI = true
          sceneName = sceneName || metadata.scene.base
        }

        parcels.push(xy)
      })

      minimapSceneInfoResult.push({
        name: postProcessSceneName(sceneName),
        owner: getOwnerNameFromJsonData(metadata),
        description: getSceneDescriptionFromJsonData(metadata),
        previewImageUrl: getThumbnailUrlFromJsonDataAndContent(
          metadata,
          scene.content,
          getContentService(store.getState()) + '/contents'
        ),
        // type is not used by renderer
        type: undefined as any,
        parcels,
        isPOI
      })
    }
  })
  getUnityInstance().UpdateMinimapSceneInformationFromAWorld(minimapSceneInfoResult)
}

function* initializePois() {
  yield call(waitForRealm)

  const daoPOIs: string[] | undefined = yield call(fetchPOIsFromDAO)

  if (daoPOIs) {
    yield put(initializePoiTiles(daoPOIs))
  } else {
    yield put(initializePoiTiles([]))
  }
}

function* reportScenesFromTilesAction(action: ReportScenesFromTile) {
  const tiles = action.payload.tiles
  try {
    const result: Array<LoadableScene> = yield call(fetchScenesByLocation, tiles)

    yield call(reportScenes, result)
  } catch (err: any) {
    trackEvent('error', {
      context: 'reportScenesFromTilesAction',
      message: err.message,
      stack: err.stack
    })
  }
  yield put(reportedScenes(tiles))
}

function* setHomeSceneAction(action: SetHomeScene) {
  yield call(saveToPersistentStorage, homePointKey, action.payload.position)
  yield put(sendHomeScene(action.payload.position))
}

function* sendHomeSceneToUnityAction(action: SendHomeScene) {
  yield call(waitForRendererInstance)
  getUnityInstance().UpdateHomeScene(action.payload.position)
}

function* reportScenes(scenes: LoadableScene[]): any {
  yield call(waitForPoiTilesInitialization)
  const pois = yield select(getPoiTiles)

  const minimapSceneInfoResult: MinimapSceneInfo[] = []

  scenes.forEach((scene) => {
    const parcels: Vector2[] = []
    let isPOI: boolean = false
    const metadata: Scene | undefined = scene.entity.metadata

    if (metadata) {
      let sceneName = metadata.display?.title || ''

      metadata.scene.parcels.forEach((parcel) => {
        const xy: Vector2 = parseParcelPosition(parcel)

        if (pois.includes(parcel)) {
          isPOI = true
          sceneName = sceneName || metadata.scene.base
        }

        parcels.push(xy)
      })

      minimapSceneInfoResult.push({
        name: postProcessSceneName(sceneName),
        owner: getOwnerNameFromJsonData(metadata),
        description: getSceneDescriptionFromJsonData(metadata),
        previewImageUrl: getThumbnailUrlFromJsonDataAndContent(metadata, scene.entity.content, scene.baseUrl),
        // type is not used by renderer
        type: undefined as any,
        parcels,
        isPOI
      })
    }
  })

  yield call(waitForRendererInstance)
  getUnityInstance().UpdateMinimapSceneInformation(minimapSceneInfoResult)
}

async function fetchPOIsFromDAO(): Promise<string[] | undefined> {
  const url = getPOIService(store.getState())
  try {
    const response = await fetch(url)
    if (response.ok) {
      const result = await response.json()
      return result
    }
  } catch (error) {
    defaultLogger.warn(`Error while fetching POIs from DAO ${error}`)
  }
}
