import { QuestState } from 'dcl-quests-client'

export type RootQuestsState = {
  quests: QuestsState
}

export type QuestsState = {
  initialized: boolean
  previousQuests?: QuestState[]
  quests: QuestState[]
}
