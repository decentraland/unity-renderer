import { Action } from 'redux'
import { ForkEffect, fork, take, cancel } from 'redux-saga/effects'

export function takeLatestById<T extends Action>(
  patternOrChannel: any,
  keyFunction: (action: T) => string,
  saga: any,
  ...args: any
): ForkEffect {
  return fork(function*() {
    let lastTasks = new Map<any, any>()
    while (true) {
      const action = yield take(patternOrChannel)
      const key = keyFunction(action)
      const task = lastTasks.get(key)
      if (task) {
        lastTasks.delete(key)
        yield cancel(task) // cancel is no-op if the task has already terminated
      }
      lastTasks.set(key, yield fork(saga, ...args.concat(action)))
    }
  })
}
