import { error } from './helpers'

declare var Promise: any

/** @public */
export type TaskResult<T> = Promise<T> & {
  isComplete: boolean
  didFail?: boolean
  error?: Error
  result?: T
}

const _defer = Promise.resolve().then.bind(Promise.resolve())

/**
 * Executes an asynchronous task
 * @param task - the task to execute
 * @public
 */
export function executeTask<T>(task: () => Promise<T>): TaskResult<T> {
  const result: TaskResult<T> = _defer(task)

  result.isComplete = false

  result
    .then($ => {
      result.isComplete = true
      result.result = $
      result.didFail = false
    })
    .catch($ => {
      result.isComplete = true
      result.error = $
      result.didFail = true
      error('executeTask: FAILED ' + $.toString(), $)
    })

  return result
}
