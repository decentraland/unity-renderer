import { ClientResponse, PlayerQuestDetails } from 'dcl-quests-client'
import { delay, put, select, takeEvery } from 'redux-saga/effects'
import { USER_AUTHENTIFIED } from 'shared/session/actions'
import { questsInitialized, questsUpdated, QUESTS_INITIALIZED, QUESTS_UPDATED } from './actions'
import { questsRequest } from './client'
import { unityInterface } from 'unity-interface/UnityInterface'
import { toRendererQuest } from 'dcl-ecs-quests/src/mappings'
import { rendererInitialized } from 'shared/renderer'
import { QUESTS_ENABLED } from 'config'
import { getPreviousQuests, getQuests } from './selectors'
import { deepEqual } from 'atomicHelpers/deepEqual'

const QUESTS_REFRESH_INTERVAL = 30000

export function* questsSaga(): any {
  if (QUESTS_ENABLED) {
    yield takeEvery(USER_AUTHENTIFIED, initializeQuests)
    yield takeEvery(QUESTS_INITIALIZED, initUpdateQuestsInterval)
  }
}

function* initUpdateQuestsInterval() {
  yield takeEvery(QUESTS_UPDATED, updateQuestsLogData)

  while (true) {
    yield delay(QUESTS_REFRESH_INTERVAL)
    yield updateQuests()
  }
}

function* initializeQuests(): any {
  const questsResponse: ClientResponse<PlayerQuestDetails[]> = yield questsRequest((c) => c.getQuests())
  if (questsResponse.ok) {
    yield rendererInitialized()
    initQuestsLogData(questsResponse.body)
    yield put(questsInitialized(questsResponse.body))
  } else {
    yield delay(QUESTS_REFRESH_INTERVAL)
    yield initializeQuests()
  }
}

function* updateQuests() {
  const questsResponse: ClientResponse<PlayerQuestDetails[]> = yield questsRequest((c) => c.getQuests())
  if (questsResponse.ok) {
    yield put(questsUpdated(questsResponse.body))
  }
}

function* updateQuestsLogData() {
  const quests: PlayerQuestDetails[] = yield select(getQuests)
  const previousQuests: PlayerQuestDetails[] | undefined = yield select(getPreviousQuests)

  function hasChanged(quest: PlayerQuestDetails) {
    const previousQuest = previousQuests?.find((it) => it.id === quest.id)
    return !previousQuest || !deepEqual(previousQuest, quest)
  }

  quests.forEach((it) => {
    if (hasChanged(it)) {
      unityInterface.UpdateQuestProgress(toRendererQuest(it))
    }
  })
}

function initQuestsLogData(quests: PlayerQuestDetails[]) {
  const rendererQuests = quests.map((it) => toRendererQuest(it))

  unityInterface.InitQuestsInfo(rendererQuests)
}
