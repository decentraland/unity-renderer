import { QuestState } from 'dcl-quests-client'
import { action } from 'typesafe-actions'

export const QUESTS_INITIALIZED = 'Quests Initialized'
export const questsInitialized = (quests: QuestState[]) => action(QUESTS_INITIALIZED, { quests })
export type QuestsInitialized = ReturnType<typeof questsInitialized>

export const QUESTS_UPDATED = 'Quests Updated'
export const questsUpdated = (quests: QuestState[]) => action(QUESTS_UPDATED, { quests })
export type QuestsUpdated = ReturnType<typeof questsUpdated>
