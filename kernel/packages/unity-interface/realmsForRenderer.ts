import { StoreContainer } from '../shared/store/rootTypes'
import { getExploreRealmsService, getRealm } from '../shared/dao/selectors'
import { RealmsInfoForRenderer } from '../shared/types'
import { observeRealmChange } from '../shared/dao'
import { Realm } from '../shared/dao/types'
import { unityInterface } from './UnityInterface'
import defaultLogger from '../shared/logger'

const REPORT_INTERVAL = 2 * 60 * 1000

declare const globalThis: StoreContainer

let isReporting = false

export function startRealmsReportToRenderer() {
  if (!isReporting) {
    isReporting = true

    const realm = getRealm(globalThis.globalStore.getState())
    if (realm) {
      reportToRenderer({ current: convertRealmType(realm) })
    }

    observeRealmChange(globalThis.globalStore, (previous, current) => {
      reportToRenderer({ current: convertRealmType(current) })
    })

    fetchAndReportRealmsInfo().catch((e) => defaultLogger.log(e))

    setInterval(async () => {
      await fetchAndReportRealmsInfo()
    }, REPORT_INTERVAL)
  }
}

async function fetchAndReportRealmsInfo() {
  const url = getExploreRealmsService(globalThis.globalStore.getState())
  try {
    const response = await fetch(url)
    if (response.ok) {
      const value = await response.json()
      reportToRenderer({ realms: value })
    }
  } catch (e) {
    defaultLogger.log(e)
  }
}

function reportToRenderer(info: Partial<RealmsInfoForRenderer>) {
  unityInterface.UpdateRealmsInfo(info)
}

function convertRealmType(realm: Realm): { serverName: string; layer: string } {
  return { serverName: realm.catalystName, layer: realm.layer }
}
