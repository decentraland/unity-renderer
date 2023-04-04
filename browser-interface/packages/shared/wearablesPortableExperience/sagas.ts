import { EntityType, Scene } from '@dcl/schemas'
import { call, select, takeEvery, takeLatest } from '@redux-saga/core/effects'
import { jsonFetch } from 'lib/javascript/jsonFetch'
import { put } from 'redux-saga-test-plan/matchers'
import { getFetchContentUrlPrefixFromRealmAdapter } from 'shared/realm/selectors'
import type { IRealmAdapter } from 'shared/realm/types'
import { wearablesRequest, WearablesSuccess, WEARABLES_SUCCESS } from 'shared/catalogs/actions'
import defaultLogger from 'lib/logger'
import type { ProfileSuccessAction } from 'shared/profiles/actions'
import { PROFILE_SUCCESS } from 'shared/profiles/actions'
import { isCurrentUserId } from 'shared/session/selectors'
import type { LoadableScene, WearableV2 } from 'shared/types'
import { getDesiredWearablePortableExpriences } from 'shared/wearablesPortableExperience/selectors'
import {
  addDesiredPortableExperience,
  processWearables,
  ProcessWearablesAction,
  PROCESS_WEARABLES,
  removeDesiredPortableExperience
} from './actions'
import { waitForRealm } from 'shared/realm/waitForRealmAdapter'

export function* wearablesPortableExperienceSaga(): any {
  yield takeLatest(PROFILE_SUCCESS, handleSelfProfileSuccess)
  yield takeEvery(WEARABLES_SUCCESS, handleWearablesSuccess)
  yield takeEvery(PROCESS_WEARABLES, handleProcessWearables)
}

function* handleSelfProfileSuccess(action: ProfileSuccessAction): any {
  const isMyProfile: boolean = yield select(isCurrentUserId, action.payload.profile.userId)

  // cancel the saga if we receive a profile from a different user
  if (!isMyProfile) {
    return
  }

  const newProfileWearables = action.payload.profile.avatar?.wearables || []
  const currentDesiredPortableExperiences: Record<string, LoadableScene | null> = yield select(
    getDesiredWearablePortableExpriences
  )

  // if the PX is no-longer present in the new profile then remove it from the "desired" list
  for (const id of Object.keys(currentDesiredPortableExperiences)) {
    if (!newProfileWearables.includes(id)) {
      yield put(removeDesiredPortableExperience(id))
    }
  }

  // create a list of wearables to load
  const wearablesToAdd: string[] = []
  for (const id of newProfileWearables) {
    if (!(id in currentDesiredPortableExperiences)) {
      wearablesToAdd.push(id)
      yield put(addDesiredPortableExperience(id, null))
    }
  }

  // TODO: use the catalog for this. The information is already available somewhere
  // send the request of wearables to load
  if (wearablesToAdd.length) {
    yield put(wearablesRequest({ wearableIds: wearablesToAdd }))
  }
}

// update the data on the currentDesiredPortableExperiences to include fetched runtime information
function* handleProcessWearables(action: ProcessWearablesAction) {
  const { payload } = action
  const currentDesiredPortableExperiences: Record<string, LoadableScene | null> = yield select(
    getDesiredWearablePortableExpriences
  )

  if (payload.wearable.id in currentDesiredPortableExperiences) {
    yield put(addDesiredPortableExperience(payload.wearable.id, payload.wearable))
  }
}

// process all the received wearables and creates portable experiences definitions for them
function* handleWearablesSuccess(action: WearablesSuccess): any {
  const { wearables } = action.payload
  const wearablesToProcess = wearables.filter((w) =>
    w.data.representations.some((r) => r.contents.some((c) => c.key.endsWith('game.js')))
  )

  if (wearablesToProcess.length > 0) {
    const adapter: IRealmAdapter = yield call(waitForRealm)
    const defaultBaseUrl: string = getFetchContentUrlPrefixFromRealmAdapter(adapter)

    for (const wearable of wearablesToProcess) {
      try {
        const entity: LoadableScene = yield call(wearableToSceneEntity, wearable, defaultBaseUrl)
        yield put(processWearables(entity))
      } catch (e: any) {
        defaultLogger.log(e)
      }
    }
  }
}

export async function wearableToSceneEntity(wearable: WearableV2, defaultBaseUrl: string): Promise<LoadableScene> {
  const defaultSceneJson = {
    main: 'bin/game.js',
    scene: {
      parcels: ['0,0'],
      base: '0,0'
    },
    requiredPermissions: []
  }
  const baseUrl = wearable.baseUrl ?? defaultBaseUrl

  // Get the wearable content containing the game.js
  const wearableContent = wearable.data.representations.filter((representation) =>
    representation.contents.some((c) => c.key.endsWith('game.js'))
  )[0].contents
  const sceneJson = wearableContent.find(($) => $.key.endsWith('scene.json'))

  // In the deployment the content was replicated when the bodyShape selected was 'both'
  //  this add the prefix 'female/' or 'male/' if they have more than one representations.
  // So, the scene (for now) is the same for both. We crop this prefix and keep the scene tree folder

  const femaleCrop =
    wearableContent.filter(($) => $.key.substring(0, 7) === 'female/').length === wearableContent.length
  const maleCrop = wearableContent.filter(($) => $.key.substring(0, 5) === 'male/').length === wearableContent.length

  const getFile = (key: string): string => {
    if (femaleCrop) return key.substring(7)
    if (maleCrop) return key.substring(5)
    return key
  }

  const content = wearableContent.map(($) => ({ file: getFile($.key), hash: $.hash }))
  const metadata: Scene = sceneJson ? await jsonFetch(baseUrl + sceneJson.hash) : defaultSceneJson

  return {
    id: wearable.id,
    baseUrl,
    parentCid: 'avatar',
    entity: {
      content,
      metadata,
      pointers: [wearable.id],
      timestamp: 0,
      type: EntityType.SCENE,
      version: 'v3'
    }
  }
}
