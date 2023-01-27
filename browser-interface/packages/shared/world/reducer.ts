import { AnyAction } from 'redux'
import { WorldState } from './types'
import { SetCurrentScene, SET_CURRENT_SCENE } from './actions'

const INITIAL_STATE: WorldState = {
  currentScene: undefined
}

export function worldReducer(state?: WorldState, action?: AnyAction): WorldState {
  if (!state) {
    return INITIAL_STATE
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case SET_CURRENT_SCENE:
      return {
        ...state,
        currentScene: (action as SetCurrentScene).payload.currentScene
      }
    default:
      return state
  }
}
