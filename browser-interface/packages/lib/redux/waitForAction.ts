import type { AnyAction } from 'redux'
import { take } from 'redux-saga/effects'

export function waitForAction(actionFilter: (action: AnyAction) => any, actionType?: string | string[]) {
  return function* waitingForAction() {
    while (true) {
      const action = yield take(actionType || '*')
      if (actionFilter(action)) {
        return
      }
    }
  }
}
