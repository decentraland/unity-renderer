import { RootQuestsState } from './types'

export const getQuests = (state: RootQuestsState) => state.quests.quests
export const getPreviousQuests = (state: RootQuestsState) => state.quests.previousQuests
