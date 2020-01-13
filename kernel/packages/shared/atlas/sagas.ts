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
  reportedScenes
} from './actions'
import { getNameFromAtlasState, getTypeFromAtlasState, shouldLoadSceneJsonName } from './selectors'
import { AtlasState, FETCH_NAME_FROM_SCENE_JSON, MarketEntry, SUCCESS_NAME_FROM_SCENE_JSON, MARKET_DATA } from './types'
import { parcelLimits } from '../../config'

declare const window: {
  unityInterface: {
    UpdateMinimapSceneInformation: (data: { name: string; type: number; parcels: { x: number; y: number }[] }[]) => void
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
  window.unityInterface.UpdateMinimapSceneInformation([
    {
      name,
      type,
      parcels: parcels.map(p => {
        const [x, y] = p.split(',').map(_ => parseInt(_, 10))
        return { x, y }
      })
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
  const userPosition = lastPlayerPosition
  let atlasState = (yield select(state => state.atlas)) as AtlasState
  while (!atlasState.marketName['0,0']) {
    yield take(MARKET_DATA)
    atlasState = yield select(state => state.atlas)
  }
  const data = atlasState.marketName
  const targets: Record<string, MarketEntry> = {}
  const MAX_SCENES_AROUND = 15
  const userX = userPosition.x / parcelLimits.parcelSize
  const userY = userPosition.z / parcelLimits.parcelSize
  Object.keys(data).forEach(index => {
    const parcel = data[index]
    if (atlasState.alreadyReported[`${parcel.x},${parcel.y}`]) {
      return
    }
    if (Math.abs(parcel.x - userX) > MAX_SCENES_AROUND) {
      return
    }
    if (Math.abs(parcel.y - userY) > MAX_SCENES_AROUND) {
      return
    }
    targets[index] = parcel
  })
  yield put(reportedScenes(Object.keys(targets), { x: userPosition.x, y: userPosition.z }))
  yield call(reportScenes, atlasState, targets)
}

export function* reportScenes(marketplaceInfo?: AtlasState, selection?: Record<string, MarketEntry>): any {
  const atlasState = marketplaceInfo ? marketplaceInfo : ((yield select(state => state.atlas)) as AtlasState)
  const data = selection ? selection : atlasState.marketName
  const mapByTypeAndName: Record<string, { x: number; y: number }[]> = {}
  const typeAndNameKeys: string[] = []
  const keyToTypeAndName: Record<string, { type: number; name: string }> = {}
  Object.keys(data).forEach(index => {
    const parcel = data[index]
    const name = getNameFromAtlasState(atlasState, parcel.x, parcel.y)
    const type = getTypeFromAtlasState(atlasState, parcel.x, parcel.y)
    const key = `${type}_${name}`
    if (!mapByTypeAndName[key]) {
      mapByTypeAndName[key] = []
      typeAndNameKeys.push(key)
      keyToTypeAndName[key] = { type, name }
    }
    mapByTypeAndName[key].push({ x: parcel.x, y: parcel.y })
  })
  window.unityInterface.UpdateMinimapSceneInformation(
    typeAndNameKeys.map(key => ({
      name: keyToTypeAndName[key].name,
      type: keyToTypeAndName[key].type,
      parcels: mapByTypeAndName[key]
    }))
  )
}
