import { select, take } from 'redux-saga/effects'

export function waitFor(selector: (state: any) => any, actionType?: string | string[]) {
  return function* waitingFor() {
    let result = yield select(selector)
    while (!result) {
      yield take(actionType || '*')
      result = yield select(selector)
    }
    return result
  }
}
