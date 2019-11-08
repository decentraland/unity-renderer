import { Store } from 'redux'
import {
  ENABLE_WEB3,
  ETHEREUM_NETWORK,
  getLoginConfigurationForCurrentDomain,
  getTLD,
  PREVIEW,
  setNetwork,
  STATIC_WORLD,
  WORLD_EXPLORER
} from '../config'
import { initialize, queueTrackingEvent, identifyUser } from './analytics'
import './apis/index'
import { connect, disconnect } from './comms'
import { persistCurrentUser } from './comms/index'
import { isMobile } from './comms/mobile'
import { localProfileUUID } from './comms/peers'
import './events'
import { ReportFatalError } from './loading/ReportFatalError'
import {
  AUTH_ERROR_LOGGED_OUT,
  COMMS_COULD_NOT_BE_ESTABLISHED,
  MOBILE_NOT_SUPPORTED,
  loadingStarted,
  authSuccessful,
  establishingComms,
  commsEstablished,
  commsErrorRetrying,
  notStarted
} from './loading/types'
import { defaultLogger } from './logger'
import { PassportAsPromise } from './passports/PassportAsPromise'
import { Session } from './session/index'
import { RootState } from './store/rootTypes'
import { buildStore } from './store/store'
import { getAppNetwork, getNetworkFromTLD, initWeb3 } from './web3'
import { initializeUrlPositionObserver } from './world/positionThings'
import { setWorldContext } from './protocol/actions'
import { profileToRendererFormat } from './passports/transformations/profileToRendererFormat'

// TODO fill with segment keys and integrate identity server
function initializeAnalytics() {
  const TLD = getTLD()
  switch (TLD) {
    case 'org':
      return initialize('1plAT9a2wOOgbPCrTaU8rgGUMzgUTJtU')
    case 'today':
      return initialize('a4h4BC4dL1v7FhIQKKuPHEdZIiNRDVhc')
    case 'zone':
      return initialize('a4h4BC4dL1v7FhIQKKuPHEdZIiNRDVhc')
    default:
      return initialize('a4h4BC4dL1v7FhIQKKuPHEdZIiNRDVhc')
  }
}

export let globalStore: Store<RootState>

export async function initShared(): Promise<Session | undefined> {
  if (WORLD_EXPLORER) {
    await initializeAnalytics()
  }

  const { store, startSagas, auth } = buildStore({
    ...getLoginConfigurationForCurrentDomain(),
    ephemeralKeyTTL: 24 * 60 * 60 * 1000
  })
  ;(window as any).globalStore = globalStore = store

  if (WORLD_EXPLORER) {
    startSagas()
  }

  if (isMobile()) {
    ReportFatalError(MOBILE_NOT_SUPPORTED)
    return undefined
  }

  store.dispatch(notStarted())

  const session = new Session()

  let userId: string

  console['group']('connect#login')
  store.dispatch(loadingStarted())

  if (WORLD_EXPLORER) {
    try {
      userId = await auth.getUserId()
      identifyUser(userId)
      session.auth = auth
    } catch (e) {
      defaultLogger.error(e)
      console['groupEnd']()
      ReportFatalError(AUTH_ERROR_LOGGED_OUT)
      throw e
    }
  } else {
    defaultLogger.log(`Using test user.`)
    userId = 'email|5cdd68572d5f842a16d6cc17'
  }

  defaultLogger.log(`User ${userId} logged in`)
  store.dispatch(authSuccessful())

  console['groupEnd']()

  console['group']('connect#ethereum')

  let net: ETHEREUM_NETWORK

  if (ENABLE_WEB3) {
    await initWeb3()
    net = await getAppNetwork()
  } else {
    net = getNetworkFromTLD() || ETHEREUM_NETWORK.MAINNET
  }

  queueTrackingEvent('Use network', { net })

  // Load contracts from https://contracts.decentraland.org
  await setNetwork(net)
  console['groupEnd']()

  initializeUrlPositionObserver()

  // DCL Servers connections/requests after this
  if (STATIC_WORLD) {
    return session
  }

  console['group']('connect#comms')
  store.dispatch(establishingComms())
  const maxAttemps = 5
  for (let i = 1; ; ++i) {
    try {
      defaultLogger.info(`Attempt number ${i}...`)
      const context = await connect(
        userId,
        net,
        auth
      )
      if (context !== undefined) {
        store.dispatch(setWorldContext(context))
      }
      break
    } catch (e) {
      if (e.message.startsWith('Communications link')) {
        if (i >= maxAttemps) {
          // max number of attemps reached => rethrow error
          defaultLogger.info(`Max number of attemps reached (${maxAttemps}), unsuccessful connection`)
          disconnect()
          ReportFatalError(COMMS_COULD_NOT_BE_ESTABLISHED)
          throw e
        } else {
          // max number of attempts not reached => continue with loop
          store.dispatch(commsErrorRetrying(i))
        }
      } else {
        // not a comms issue per se => rethrow error
        disconnect()
        throw e
      }
    }
  }
  store.dispatch(commsEstablished())
  console['groupEnd']()

  // initialize profile
  console['group']('connect#profile')
  if (!PREVIEW) {
    const profile = await PassportAsPromise(localProfileUUID!)
    persistCurrentUser({
      userId: localProfileUUID!,
      version: profile.version,
      profile: profileToRendererFormat(profile)
    })
  }
  console['groupEnd']()

  return session
}
