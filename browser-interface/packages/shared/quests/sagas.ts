import { ClientResponse, QuestState } from 'dcl-quests-client'
import { call, delay, put, select, takeEvery } from 'redux-saga/effects'
import { USER_AUTHENTICATED } from 'shared/session/actions'
import { questsInitialized, questsUpdated, QUESTS_INITIALIZED, QUESTS_UPDATED } from './actions'
import { questsRequest } from './client'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import { toRendererQuest } from '@dcl/ecs-quests/@dcl/mappings'
import { getPreviousQuests, getQuests } from './selectors'
import { deepEqual } from 'lib/javascript/deepEqual'
import { getFeatureFlagEnabled } from '../meta/selectors'
import { waitForRendererInstance } from 'shared/renderer/sagas-helper'
import { waitForMetaConfigurationInitialization } from 'shared/meta/sagas'

const QUESTS_REFRESH_INTERVAL = 30000

export function* questsSaga(): any {
  yield takeEvery(USER_AUTHENTICATED, initializeQuests)
  yield takeEvery(QUESTS_INITIALIZED, initUpdateQuestsInterval)
}

function* areQuestsEnabled() {
  yield call(waitForMetaConfigurationInitialization)
  const ret: boolean = yield select(getFeatureFlagEnabled, 'quests')
  return ret
}

function* initUpdateQuestsInterval() {
  yield takeEvery(QUESTS_UPDATED, updateQuestsLogData)
  const questEnabled: boolean = yield call(areQuestsEnabled)
  if (questEnabled) {
    while (true) {
      yield delay(QUESTS_REFRESH_INTERVAL)
      yield updateQuests()
    }
  }
}

function* initializeQuests(): any {
  if (yield call(areQuestsEnabled)) {
    const questsResponse: ClientResponse<QuestState[]> = yield questsRequest((c) => c.getQuests())
    if (questsResponse.ok) {
      yield call(waitForRendererInstance)
      initQuestsLogData(questsResponse.body)
      yield put(questsInitialized(questsResponse.body))
    } else {
      yield delay(QUESTS_REFRESH_INTERVAL)
      yield initializeQuests()
    }
  }
}

function* updateQuests() {
  const questsResponse: ClientResponse<QuestState[]> = yield questsRequest((c) => c.getQuests())
  if (questsResponse.ok) {
    yield put(questsUpdated(questsResponse.body))
  }
}

function* updateQuestsLogData() {
  const quests: QuestState[] = yield select(getQuests)
  const previousQuests: QuestState[] | undefined = yield select(getPreviousQuests)

  function hasChanged(quest: QuestState) {
    const previousQuest = previousQuests?.find((it) => it.id === quest.id)
    return !previousQuest || !deepEqual(previousQuest, quest)
  }

  yield call(waitForRendererInstance)

  quests.forEach((it) => {
    if (hasChanged(it)) {
      getUnityInstance().UpdateQuestProgress(toRendererQuest(it))
    }
  })
}

function initQuestsLogData(quests: QuestState[]) {
  const rendererQuests = quests.map((it) => toRendererQuest(it))

  getUnityInstance().InitQuestsInfo(rendererQuests)
}
