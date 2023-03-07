import { PIN_CATALYST, PREVIEW, rootURLPreviewMode } from 'config'
import defaultLogger from 'lib/logger'
import { call } from 'redux-saga/effects'
import { pickCatalyst } from 'shared/catalystSelection/sagas'
import { waitForCatalystCandidates } from 'shared/catalystSelection/waitFor/candidates'
import { BringDownClientAndReportFatalError } from 'shared/loading/ReportFatalError'
import { waitForExplorerIdentity } from 'shared/session/waitForExplorerIdentity'
import { changeRealm } from './changeRealm'

/**
 * This method will try to load the candidates as well as the selected realm.
 *
 * The strategy to select the realm in terms of priority is:
 * 1- Realm configured in the URL and cached candidate for that realm (uses cache, forks async candidate initialization)
 * 2- Realm configured in the URL but no corresponding cached candidate (implies sync candidate initialization)
 * 3- Last cached realm (uses cache, forks async candidate initialization)
 * 4- Best pick from candidate scan (implies sync candidate initialization)
 */

export function* selectAndReconnectRealm() {
  try {
    yield call(tryConnectRealm)
    // if no realm was selected, then do the whole initialization dance
  } catch (e: any) {
    // if it failed, try changing the queryString
    clearQsRealm()
    try {
      // and try again
      yield call(tryConnectRealm)
    } catch (e: any) {
      debugger
      BringDownClientAndReportFatalError(e, 'comms#init')
      throw e
    }
  }
}

function qsRealm() {
  const qs = new URLSearchParams(document.location.search)
  return qs.get('realm')
}

function clearQsRealm() {
  const q = new URLSearchParams(globalThis.location.search)
  q.delete('realm')
  globalThis.history.replaceState({}, 'realm', `?${q.toString()}`)
}

function* tryConnectRealm() {
  const realm: string | undefined = yield call(selectRealm)

  if (realm) {
    yield call(waitForExplorerIdentity)
    yield call(changeRealm, realm, true)
  } else {
    throw new Error("Couldn't select a suitable realm to join.")
  }
}

function* selectRealm() {
  yield call(waitForCatalystCandidates)

  const realm: string | undefined =
    // query param (catalyst candidates & cached)
    (yield call(qsRealm)) ||
    // preview mode
    (PREVIEW ? rootURLPreviewMode() : null) ||
    // CATALYST from url parameter
    PIN_CATALYST ||
    // fetch catalysts and select one using the load balancing
    (yield call(pickCatalyst))

  if (!realm) {
    debugger
  }

  defaultLogger.log(`Selected catalyst: `, realm)

  return realm
}
