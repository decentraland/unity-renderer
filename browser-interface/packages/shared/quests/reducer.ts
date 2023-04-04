import { AnyAction } from 'redux'
import { QuestsInitialized, QuestsUpdated, QUESTS_INITIALIZED, QUESTS_UPDATED } from './actions'
import { QuestsState } from './types'

const INITIAL_STATE: QuestsState = {
  initialized: false,
  quests: []
}

export function questsReducer(state?: QuestsState, action?: AnyAction) {
  if (!state) {
    return INITIAL_STATE
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case QUESTS_INITIALIZED: {
      return { ...state, initialized: true, quests: (action as QuestsInitialized).payload.quests }
    }
    case QUESTS_UPDATED: {
      return {
        ...state,
        initialized: true,
        previousQuests: state.quests,
        quests: (action as QuestsUpdated).payload.quests
      }
    }
  }

  return state
}
