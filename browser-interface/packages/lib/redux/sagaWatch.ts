import { createLogger } from 'lib/logger'
import type { SagaMonitor } from 'redux-saga'

const logger = createLogger('sagas')

export function createSagaWatcher(): SagaMonitor {
  return {
    rootSagaStarted: (effectId: number, _, args) =>
      logger.info(`started ${effectId}: ${_.name}(${JSON.stringify(args)})`),
    effectTriggered: (effectId: number, parent: number, label) =>
      logger.info(`triggered ${effectId} from ${parent} (${label})`),
    effectRejected: (effectId, error) => logger.info(`errored ${effectId} (${error})`),
    effectCancelled: (effectId) => logger.info(`cancelled: ${effectId}`),
    actionDispatched: (action) => logger.info(`action of type ${action.type}`)
  } as any
}
