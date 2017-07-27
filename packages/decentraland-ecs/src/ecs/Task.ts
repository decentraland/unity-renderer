import { log } from './Engine'

export type TaskResult<T> = Promise<T> & {
  isComplete: boolean
  didFail?: boolean
  error?: Error
  result?: T
}

/**
 * Executes an asynchronous task
 * @param task - the task to execute
 * @beta
 */
export function executeTask<T>(task: () => Promise<T>): TaskResult<T> {
  const result: TaskResult<T> = task() as any

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
      log('executeTask: FAILED', $.toString(), $)
    })

  return result
}
