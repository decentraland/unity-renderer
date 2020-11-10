import { isMobile } from './comms/mobile'

import './apis/index'
import './events'

import { initializeUrlRealmObserver } from './dao'
import { ReportFatalError } from './loading/ReportFatalError'
import { loadingStarted, notStarted, MOBILE_NOT_SUPPORTED } from './loading/types'
import { buildStore } from './store/store'
import { initializeUrlPositionObserver } from './world/positionThings'
import { StoreContainer } from './store/rootTypes'
import { initSession } from './session/actions'

declare const globalThis: StoreContainer

export function initShared() {
  if (globalThis.globalStore) {
    return
  }
  const { store, startSagas } = buildStore()
  globalThis.globalStore = store

  startSagas()

  if (isMobile()) {
    ReportFatalError(MOBILE_NOT_SUPPORTED)
    return
  }

  store.dispatch(notStarted())
  store.dispatch(loadingStarted())

  store.dispatch(initSession())

  initializeUrlPositionObserver()
  initializeUrlRealmObserver()
}
