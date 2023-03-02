import { saveReferral } from './utils'
import { call } from 'redux-saga/effects'

export function* initializeReferral() {
  yield call(saveReferral)
}
