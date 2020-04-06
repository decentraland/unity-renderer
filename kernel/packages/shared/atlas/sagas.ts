import { call, fork, put, select, take, takeEvery, takeLatest } from 'redux-saga/effects'
import { getServer, LifecycleManager } from '../../decentraland-loader/lifecycle/manager'
import { SceneStart, SCENE_START, SCENE_LOAD } from '../loading/actions'
import defaultLogger from '../logger'
import { RENDERER_INITIALIZED } from '../renderer/types'
import { lastPlayerPosition } from '../world/positionThings'
import {
  districtData,
  fetchNameFromSceneJson,
  fetchNameFromSceneJsonFailure,
  fetchNameFromSceneJsonSuccess,
  FetchNameFromSceneJsonSuccess,
  marketData,
  QuerySceneName,
  reportedScenes,
  ReportScenesAroundParcel
} from './actions'
import { getNameFromAtlasState, getTypeFromAtlasState, shouldLoadSceneJsonName } from './selectors'
import {
  AtlasState,
  FETCH_NAME_FROM_SCENE_JSON,
  MarketEntry,
  SUCCESS_NAME_FROM_SCENE_JSON,
  MARKET_DATA,
  REPORT_SCENES_AROUND_PARCEL
} from './types'
import { parcelLimits } from '../../config'
import { Vector2Component } from 'atomicHelpers/landHelpers'
import { CAMPAIGN_PARCEL_SEQUENCE } from 'shared/world/TeleportController'
import { MinimapSceneInfo } from 'decentraland-ecs/src/decentraland/Types'

declare const window: {
  unityInterface: {
    UpdateMinimapSceneInformation: (data: MinimapSceneInfo[]) => void
  }
}

export function* atlasSaga(): any {
  yield fork(fetchDistricts)
  yield fork(fetchTiles)

  yield takeEvery(SCENE_START, querySceneName)
  yield takeEvery(SCENE_LOAD, checkAndReportAround)
  yield takeEvery(FETCH_NAME_FROM_SCENE_JSON, fetchName)

  yield takeLatest(RENDERER_INITIALIZED, reportScenesAround)
  yield takeLatest(SUCCESS_NAME_FROM_SCENE_JSON, reportOne)

  yield takeLatest(REPORT_SCENES_AROUND_PARCEL, reportScenesAroundParcel)
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

function* querySceneName(action: QuerySceneName) {
  if (yield select(shouldLoadSceneJsonName, action.payload) !== undefined) {
    yield put(fetchNameFromSceneJson(action.payload))
  }
}

function* fetchName(action: SceneStart) {
  try {
    const { name, parcels } = yield call(() => getNameFromSceneJson(action.payload))
    yield put(fetchNameFromSceneJsonSuccess(action.payload, name, parcels))
  } catch (e) {
    yield put(fetchNameFromSceneJsonFailure(action.payload, e))
  }
}

async function getNameFromSceneJson(sceneId: string) {
  const server: LifecycleManager = getServer()

  const land = (await server.getParcelData(sceneId)) as any
  return { name: land.scene.display.title, parcels: land.scene.scene.parcels }
}

function* reportOne(action: FetchNameFromSceneJsonSuccess) {
  const atlasState = (yield select(state => state.atlas)) as AtlasState
  const parcels = action.payload.parcels
  const [firstX, firstY] = parcels[0].split(',').map(_ => parseInt(_, 10))
  const name = getNameFromAtlasState(atlasState, firstX, firstY)
  const type = getTypeFromAtlasState(atlasState, firstX, firstY)
  yield put(reportedScenes(parcels))

  let isPOI: boolean = false

  let parcelsAsV2: { x: number; y: number }[] = parcels.map(p => {
    const [x, y] = p.split(',').map(_ => parseInt(_, 10))
    return { x, y }
  })

  // TODO(Brian): we will refactor this in the info plumbing PR
  parcelsAsV2.forEach(p => {
    CAMPAIGN_PARCEL_SEQUENCE.forEach(p2 => {
      if (p.x === p2.x && p.y === p2.y) isPOI = true
    })
  })

  window.unityInterface.UpdateMinimapSceneInformation([
    {
      name,
      type,
      parcels: parcelsAsV2,
      isPOI: isPOI
    }
  ])
}

export function* checkAndReportAround() {
  const userPosition = lastPlayerPosition
  let lastReport = yield select(state => state.atlas.lastReportPosition)
  const TRIGGER_DISTANCE = 10 * parcelLimits.parcelSize
  if (
    Math.abs(userPosition.x - lastReport.x) > TRIGGER_DISTANCE ||
    Math.abs(userPosition.z - lastReport.y) > TRIGGER_DISTANCE
  ) {
    yield call(reportScenesAround)
  }
}

export function* reportScenesAround() {
  let atlasState = (yield select(state => state.atlas)) as AtlasState
  while (!atlasState.marketName['0,0']) {
    yield take(MARKET_DATA)
    atlasState = yield select(state => state.atlas)
  }

  const userPosition = lastPlayerPosition
  const MAX_SCENES_AROUND = 15
  const userX = userPosition.x / parcelLimits.parcelSize
  const userY = userPosition.z / parcelLimits.parcelSize
  const targets = getScenesAround({ x: userX, y: userY }, MAX_SCENES_AROUND, atlasState)

  yield put(reportedScenes(Object.keys(targets), { x: userPosition.x, y: userPosition.z }))
  yield call(reportScenes, atlasState, targets)
}

export function* reportScenesAroundParcel(action: ReportScenesAroundParcel) {
  let atlasState = (yield select(state => state.atlas)) as AtlasState
  while (!atlasState.marketName['0,0']) {
    yield take(MARKET_DATA)
    atlasState = yield select(state => state.atlas)
  }

  const targets = getScenesAround(action.payload.parcelCoord, action.payload.scenesAround, atlasState)
  yield call(reportScenes, atlasState, targets)
}

function getScenesAround(parcelCoords: Vector2Component, maxScenesAround: number, atlasState: AtlasState) {
  const data = atlasState.marketName
  const targets: Record<string, MarketEntry> = {}

  Object.keys(data).forEach(index => {
    const parcel = data[index]
    if (atlasState.alreadyReported[`${parcel.x},${parcel.y}`]) {
      return
    }
    if (Math.abs(parcel.x - parcelCoords.x) > maxScenesAround) {
      return
    }
    if (Math.abs(parcel.y - parcelCoords.y) > maxScenesAround) {
      return
    }
    targets[index] = parcel
  })
  return targets
}

export function* reportScenes(marketplaceInfo?: AtlasState, selection?: Record<string, MarketEntry>): any {
  const atlasState = marketplaceInfo ? marketplaceInfo : ((yield select(state => state.atlas)) as AtlasState)
  const data = selection ? selection : atlasState.marketName

  const keyToParcels: Record<string, { x: number; y: number }[]> = {}
  const keyToTypeAndName: Record<string, { type: number; name: string }> = {}
  const keyToPOI: Record<string, boolean> = {}

  const typeAndNameKeys: string[] = []

  Object.keys(data).forEach(index => {
    const parcel = data[index]
    const name = getNameFromAtlasState(atlasState, parcel.x, parcel.y)
    const type = getTypeFromAtlasState(atlasState, parcel.x, parcel.y)
    const key = `${type}_${name}`
    if (!keyToParcels[key]) {
      keyToParcels[key] = []
      typeAndNameKeys.push(key)
      keyToTypeAndName[key] = { type, name }
    }
    keyToParcels[key].push({ x: parcel.x, y: parcel.y })

    // TODO(Brian): we will refactor this in the info plumbing PR
    if (keyToPOI[key] === false) {
      CAMPAIGN_PARCEL_SEQUENCE.forEach(element => {
        if (element.x === parcel.x && element.y === parcel.y) {
          keyToPOI[key] = true
        }
      })
    }
  })

  window.unityInterface.UpdateMinimapSceneInformation(
    typeAndNameKeys.map(
      key =>
        ({
          name: keyToTypeAndName[key].name,
          type: keyToTypeAndName[key].type,
          parcels: keyToParcels[key],
          isPOI: keyToPOI[key]
        } as MinimapSceneInfo)
    )
  )
}
