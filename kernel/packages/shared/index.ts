import { Store } from 'redux'
import {
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
import { connect, persistCurrentUser, disconnect } from './comms'
import { isMobile } from './comms/mobile'
import { setLocalProfile, getUserProfile } from './comms/peers'
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
import { getAppNetwork } from './web3'
import { initializeUrlPositionObserver } from './world/positionThings'
import { setWorldContext } from './protocol/actions'
import { profileToRendererFormat } from './passports/transformations/profileToRendererFormat'
import { awaitWeb3Approval, providerFuture, isSessionExpired } from './ethereum/provider'
import { createIdentity } from 'eth-crypto'
import { Authenticator, AuthIdentity } from './crypto/Authenticator'
import { Eth } from 'web3x/eth'
import { Personal } from 'web3x/personal/personal'
import { Account } from 'web3x/account'
import { web3initialized } from './dao/actions'
import { realmInitialized } from './dao'

enum AnalyticsAccount {
  PRD = '1plAT9a2wOOgbPCrTaU8rgGUMzgUTJtU',
  DEV = 'a4h4BC4dL1v7FhIQKKuPHEdZIiNRDVhc'
}

declare const window: any

// TODO fill with segment keys and integrate identity server
function initializeAnalytics() {
  const TLD = getTLD()
  switch (TLD) {
    case 'org':
      if (window.location.host === 'explorer.decentraland.org') {
        return initialize(AnalyticsAccount.PRD)
      }
      return initialize(AnalyticsAccount.DEV)
    case 'today':
      return initialize(AnalyticsAccount.DEV)
    case 'zone':
      return initialize(AnalyticsAccount.DEV)
    default:
      return initialize(AnalyticsAccount.DEV)
  }
}

export let globalStore: Store<RootState>
export let identity: AuthIdentity

async function createAuthIdentity() {
  const ephemeral = createIdentity()

  const result = await providerFuture
  const ephemeralLifespanMinutes = 7 * 24 * 60 // 1 week

  let address
  let signer
  if (result.successful) {
    const eth = Eth.fromCurrentProvider()!
    const account = (await eth.getAccounts())[0]

    address = account.toJSON()
    signer = async (message: string) => {
      let result
      while (!result) {
        try {
          result = await new Personal(eth.provider).sign(message, account, '')
        } catch (e) {
          if (e.message && e.message.includes('User denied message signature')) {
            showEthSignAdvice(true)
          }
        }
      }
      return result
    }
  } else {
    const account: Account = result.localIdentity

    address = account.address.toJSON()
    signer = async (message: string) => account.sign(message).signature
  }

  const identity = await Authenticator.initializeAuthChain(address, ephemeral, ephemeralLifespanMinutes, signer)

  return identity
}

export async function initShared(): Promise<Session | undefined> {
  if (WORLD_EXPLORER) {
    await initializeAnalytics()
  }

  const { store, startSagas } = buildStore({
    ...getLoginConfigurationForCurrentDomain(),
    ephemeralKeyTTL: 24 * 60 * 60 * 1000
  })
  window.globalStore = globalStore = store

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

  let net: ETHEREUM_NETWORK = ETHEREUM_NETWORK.MAINNET

  if (WORLD_EXPLORER) {
    await awaitWeb3Approval()

    try {
      const userData = getUserProfile()

      // check that user data is stored & key is not expired
      if (isSessionExpired(userData)) {
        identity = await createAuthIdentity()
        userId = identity.address

        identifyUser(userId)

        setLocalProfile(userId, {
          userId,
          identity
        })
      } else {
        identity = userData.identity
        userId = userData.identity.address

        setLocalProfile(userId, {
          userId,
          identity
        })
      }
    } catch (e) {
      defaultLogger.error(e)
      console['groupEnd']()
      ReportFatalError(AUTH_ERROR_LOGGED_OUT)
      throw e
    }
  } else {
    defaultLogger.log(`Using test user.`)
    userId = '0x0000000000000000000000000000000000000000'
  }

  defaultLogger.log(`User ${userId} logged in`)
  store.dispatch(authSuccessful())

  console['groupEnd']()

  console['group']('connect#ethereum')

  net = await getAppNetwork()

  // Load contracts from https://contracts.decentraland.org
  await setNetwork(net)
  queueTrackingEvent('Use network', { net })
  store.dispatch(web3initialized())
  console['groupEnd']()

  initializeUrlPositionObserver()

  // DCL Servers connections/requests after this
  if (STATIC_WORLD) {
    return session
  }

  await realmInitialized()
  defaultLogger.info(`Using Catalyst configuration: `, globalStore.getState().dao)

  // initialize profile
  console['group']('connect#profile')
  if (!PREVIEW) {
    const profile = await PassportAsPromise(userId)
    persistCurrentUser({
      version: profile.version,
      profile: profileToRendererFormat(profile)
    })
  }
  console['groupEnd']()

  console['group']('connect#comms')
  store.dispatch(establishingComms())
  const maxAttemps = 5
  for (let i = 1; ; ++i) {
    try {
      defaultLogger.info(`Attempt number ${i}...`)
      const context = await connect(identity.address)
      if (context !== undefined) {
        store.dispatch(setWorldContext(context))
      }

      showEthSignAdvice(false)

      break
    } catch (e) {
      if (e.message && e.message.startsWith('error establishing comms')) {
        if (i >= maxAttemps) {
          // max number of attemps reached => rethrow error
          defaultLogger.info(`Max number of attemps reached (${maxAttemps}), unsuccessful connection`)
          disconnect()
          showEthSignAdvice(false)
          ReportFatalError(COMMS_COULD_NOT_BE_ESTABLISHED)
          throw e
        } else {
          // max number of attempts not reached => continue with loop
          store.dispatch(commsErrorRetrying(i))
        }
      } else {
        // not a comms issue per se => rethrow error
        defaultLogger.error(`error while trying to establish communications `, e)
        disconnect()
        showEthSignAdvice(false)
        throw e
      }
    }
  }
  store.dispatch(commsEstablished())
  console['groupEnd']()

  return session
}

function showEthSignAdvice(show: boolean) {
  const element = document.getElementById('eth-sign-advice')
  if (element) {
    element.style.display = show ? 'block' : 'none'
  }
}
