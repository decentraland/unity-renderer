import { PlayerQuestDetails } from 'dcl-quests-client'

export type RootQuestsState = {
  quests: QuestsState
}

export type QuestsState = {
  initialized: boolean
  previousQuests?: PlayerQuestDetails[]
  quests: PlayerQuestDetails[]
}
