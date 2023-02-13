import type { Action } from 'redux'
import type { ForkEffect } from 'redux-saga/effects'
import { cancel, fork, take } from 'redux-saga/effects'

export function takeLatestById<T extends Action>(
  patternOrChannel: any,
  keyFunction: (action: T) => string,
  saga: any,
  ...args: any
): ForkEffect<any> {
  return fork(function* () {
    const lastTasks = new Map<any, any>()
    while (true) {
      const action: any = yield take(patternOrChannel)
      const key = keyFunction(action)
      const task = lastTasks.get(key)
      if (task) {
        lastTasks.delete(key)
        yield cancel(task) // cancel is no-op if the task has already terminated
      }
      const forked = yield fork<any>(saga, ...args.concat(action))
      lastTasks.set(key, forked)
    }
  })
}
