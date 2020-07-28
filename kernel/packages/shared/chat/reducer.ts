import { AnyAction } from 'redux'
import { ChatState } from './types'

const CHAT_INITIAL_STATE: ChatState = {}

export function chatReducer(state?: ChatState, action?: AnyAction) {
  if (!state) {
    return CHAT_INITIAL_STATE
  }
  if (!action) {
    return state
  }
  switch (action.type) {
  }
  return state
}
