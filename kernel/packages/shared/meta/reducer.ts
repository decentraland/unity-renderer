import { AnyAction } from 'redux'
import { META_CONFIGURATION_INITIALIZED, META_UPDATE_MESSAGE_OF_THE_DAY } from './actions'
import { MetaState, WorldConfig } from './types'

const initialState = {
  initialized: false,
  config: {}
}

export function metaReducer(state?: MetaState, action?: AnyAction): MetaState {
  if (!state) {
    return initialState
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case META_CONFIGURATION_INITIALIZED:
      return {
        ...state,
        initialized: true,
        config: action.payload
      }
    case META_UPDATE_MESSAGE_OF_THE_DAY:
      return {
        ...state,
        config: {
          ...state.config,
          world: {
            ...(state.config.world || {}),
            messageOfTheDay: action.payload,
            messageOfTheDayInit: true
          } as WorldConfig
        }
      }
    default:
      return state
  }
}
