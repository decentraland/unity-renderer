import { put, takeEvery, select, call } from 'redux-saga/effects'

import { STATIC_WORLD } from 'config'

import { createLogger } from 'shared/logger'
import { establishingComms, NEW_LOGIN, COMMS_COULD_NOT_BE_ESTABLISHED, commsErrorRetrying } from 'shared/loading/types'
import { USER_AUTHENTIFIED } from 'shared/session/actions'
import { getCurrentIdentity } from 'shared/session/selectors'
import { setWorldContext } from 'shared/protocol/actions'
import { ReportFatalError } from 'shared/loading/ReportFatalError'
import { ensureRealmInitialized } from 'shared/dao/sagas'

import { connect, disconnect } from '.'
import { IdTakenError, ConnectionEstablishmentError } from './interface/types'

const logger = createLogger('comms: ')

export function* commsSaga() {
  yield takeEvery(USER_AUTHENTIFIED, establishCommunications)
}

function* establishCommunications() {
  if (STATIC_WORLD) {
    return
  }

  yield call(ensureRealmInitialized)

  const identity = yield select(getCurrentIdentity)

  console['group']('connect#comms')
  yield put(establishingComms())
  const maxAttemps = 5
  for (let i = 1; ; ++i) {
    try {
      logger.info(`Attempt number ${i}...`)
      const context = yield connect(identity.address)
      if (context !== undefined) {
        yield put(setWorldContext(context))
      }

      break
    } catch (e) {
      if (e instanceof IdTakenError) {
        disconnect()
        ReportFatalError(NEW_LOGIN)
        throw e
      } else if (e instanceof ConnectionEstablishmentError) {
        if (i >= maxAttemps) {
          // max number of attemps reached => rethrow error
          logger.info(`Max number of attemps reached (${maxAttemps}), unsuccessful connection`)
          disconnect()
          ReportFatalError(COMMS_COULD_NOT_BE_ESTABLISHED)
          throw e
        } else {
          // max number of attempts not reached => continue with loop
          yield put(commsErrorRetrying(i))
        }
      } else {
        // not a comms issue per se => rethrow error
        logger.error(`error while trying to establish communications `, e)
        disconnect()
        throw e
      }
    }
  }
  console['groupEnd']()
}
