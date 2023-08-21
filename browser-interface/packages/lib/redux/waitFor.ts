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

export function* waitForSelector(selector: (state: any) => any) {
  if (yield select(selector)) return; // (1)

  while (true) {
    yield take('*'); // (1a)
    if (yield select(selector)) return; // (1b)
  }
}