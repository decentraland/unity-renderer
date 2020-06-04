import { Authenticator, AuthIdentity } from 'dcl-crypto'
import { createIdentity } from 'eth-crypto'
import { Account } from 'web3x/account'
import { Eth } from 'web3x/eth'
import { Personal } from 'web3x/personal/personal'
import { ETHEREUM_NETWORK, getTLD, PREVIEW, setNetwork, STATIC_WORLD, WORLD_EXPLORER } from '../config'
import { identifyUser, queueTrackingEvent, initializeAnalytics } from './analytics'
import './apis/index'
import { connect, disconnect, persistCurrentUser } from './comms'
import { ConnectionEstablishmentError, IdTakenError } from './comms/interface/types'
import { isMobile } from './comms/mobile'
import { getUserProfile, setLocalProfile } from './comms/peers'
import { initializeUrlRealmObserver, realmInitialized } from './dao'
import { web3initialized } from './dao/actions'
import { getNetwork } from './ethereum/EthereumService'
import { awaitWeb3Approval, isSessionExpired, providerFuture } from './ethereum/provider'
import './events'
import { ReportFatalError } from './loading/ReportFatalError'
import {
  authSuccessful,
  AUTH_ERROR_LOGGED_OUT,
  commsErrorRetrying,
  commsEstablished,
  COMMS_COULD_NOT_BE_ESTABLISHED,
  establishingComms,
  loadingStarted,
  MOBILE_NOT_SUPPORTED,
  NETWORK_MISMATCH,
  NEW_LOGIN,
  notStarted
} from './loading/types'
import { defaultLogger } from './logger'
import { ProfileAsPromise } from './profiles/ProfileAsPromise'
import { profileToRendererFormat } from './profiles/transformations/profileToRendererFormat'
import { setWorldContext } from './protocol/actions'
import { Session } from './session/index'
import { buildStore } from './store/store'
import { getAppNetwork, getNetworkFromTLD, fetchOwnedENS } from './web3'
import { initializeUrlPositionObserver } from './world/positionThings'
import { saveProfileRequest } from './profiles/actions'
import { ethereumConfigurations } from 'config'
import { tutorialStepId } from 'decentraland-loader/lifecycle/tutorial/tutorial'
import { ENABLE_WEB3 } from '../config/index'
import future, { IFuture } from 'fp-future'
import { RootState } from './store/rootTypes'
import { AnyAction, Store } from 'redux'

declare const globalThis: any

export type ExplorerIdentity = AuthIdentity & {
  address: string
  hasConnectedWeb3: boolean
}

export let identity: ExplorerIdentity

function initEssentials(): [Session | undefined, Store<RootState, AnyAction>] {
  const { store, startSagas } = buildStore()
  globalThis.globalStore = store

  startSagas()

  if (isMobile()) {
    ReportFatalError(MOBILE_NOT_SUPPORTED)
    return [undefined, store]
  }

  store.dispatch(notStarted())

  const session = new Session()

  console['group']('connect#login')
  store.dispatch(loadingStarted())

  return [session, store]
}

export type InitFutures = {
  essentials: IFuture<Session | undefined>
  all: IFuture<void>
}

export function initShared(): InitFutures {
  const futures: InitFutures = { essentials: future(), all: future() }
  const [session, store] = initEssentials()

  const userData = getUserProfile()

  if (!isSessionExpired(userData)) {
    futures.essentials.resolve(session)
  }

  let net: ETHEREUM_NETWORK = ETHEREUM_NETWORK.MAINNET
  let userId: string
  ;(async function() {
    if (WORLD_EXPLORER) {
      await initializeAnalytics()
    }

    if (ENABLE_WEB3) {
      await awaitWeb3Approval()

      if (WORLD_EXPLORER && (await checkTldVsNetwork())) {
        throw new Error('Network mismatch')
      }

      if (PREVIEW && ETHEREUM_NETWORK.MAINNET === (await getNetworkValue())) {
        showNetworkWarning()
      }

      try {
        // check that user data is stored & key is not expired
        if (isSessionExpired(userData)) {
          identity = await createAuthIdentity()
          userId = identity.address

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

      if (identity.hasConnectedWeb3) {
        identifyUser(userId)
      }
    } else {
      defaultLogger.log(`Using test user.`)
      identity = await createAuthIdentity()
      userId = identity.address

      setLocalProfile(userId, {
        userId,
        identity
      })
    }

    defaultLogger.log(`User ${userId} logged in`)
    store.dispatch(authSuccessful())

    if (futures.essentials.isPending) {
      futures.essentials.resolve(session)
    }

    console['groupEnd']()

    console['group']('connect#ethereum')

    if (WORLD_EXPLORER) {
      net = await getAppNetwork()

      // Load contracts from https://contracts.decentraland.org
      await setNetwork(net)
      queueTrackingEvent('Use network', { net })
    }

    store.dispatch(web3initialized())

    console['groupEnd']()

    initializeUrlPositionObserver()
    initializeUrlRealmObserver()

    await realmInitialized()

    defaultLogger.info(`Using Catalyst configuration: `, store.getState().dao)

    // initialize profile
    console['group']('connect#profile')
    let profile = await ProfileAsPromise(userId)

    if (!PREVIEW) {
      let profileDirty: boolean = false

      if (!profile.hasClaimedName) {
        const names = await fetchOwnedENS(ethereumConfigurations[net].names, userId)

        // patch profile to readd missing name
        profile = { ...profile, name: names[0], hasClaimedName: true, version: (profile.version || 0) + 1 }

        if (names && names.length > 0) {
          defaultLogger.info(
            `Found missing claimed name '${names[0]}' for profile ${userId}, consolidating profile... `
          )
          profileDirty = true
        }
      }

      const localTutorialStep = getUserProfile().profile
        ? getUserProfile().profile.tutorialStep
        : tutorialStepId.INITIAL_SCENE

      if (localTutorialStep !== profile.tutorialStep) {
        let finalTutorialStep = Math.max(localTutorialStep, profile.tutorialStep)
        profile = { ...profile, tutorialStep: finalTutorialStep }
        profileDirty = true
      }

      if (profileDirty) {
        store.dispatch(saveProfileRequest(profile))
      }
    }

    persistCurrentUser({
      version: profile.version,
      profile: profileToRendererFormat(profile, identity)
    })
    console['groupEnd']()

    // DCL Servers connections/requests after this
    if (STATIC_WORLD) {
      return
    }

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
        if (e instanceof IdTakenError) {
          disconnect()
          ReportFatalError(NEW_LOGIN)
          throw e
        } else if (e instanceof ConnectionEstablishmentError) {
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

    return
  })().then(futures.all.resolve, futures.all.reject)

  return futures
}

function showEthSignAdvice(show: boolean) {
  const element = document.getElementById('eth-sign-advice')
  if (element) {
    element.style.display = show ? 'block' : 'none'
  }
}

async function createAuthIdentity() {
  const ephemeral = createIdentity()

  const ephemeralLifespanMinutes = 7 * 24 * 60 // 1 week

  let address
  let signer
  let hasConnectedWeb3 = false

  if (ENABLE_WEB3) {
    const result = await providerFuture
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
      hasConnectedWeb3 = true
    } else {
      const account: Account = result.localIdentity

      address = account.address.toJSON()
      signer = async (message: string) => account.sign(message).signature
    }
  } else {
    const account = Account.create()

    address = account.address.toJSON()
    signer = async (message: string) => account.sign(message).signature
  }

  const auth = await Authenticator.initializeAuthChain(address, ephemeral, ephemeralLifespanMinutes, signer)
  const identity: ExplorerIdentity = { ...auth, address: address.toLocaleLowerCase(), hasConnectedWeb3 }

  return identity
}

async function getNetworkValue() {
  const web3Network = await getNetwork()
  const web3Net = web3Network === '1' ? ETHEREUM_NETWORK.MAINNET : ETHEREUM_NETWORK.ROPSTEN
  return web3Net
}

async function checkTldVsNetwork() {
  const web3Net = await getNetworkValue()

  const tld = getTLD()
  const tldNet = getNetworkFromTLD()

  if (tld === 'localhost') {
    // localhost => allow any network
    return false
  }

  if (tldNet !== web3Net) {
    document.getElementById('tld')!.textContent = tld
    document.getElementById('web3Net')!.textContent = web3Net
    document.getElementById('web3NetGoal')!.textContent = tldNet

    ReportFatalError(NETWORK_MISMATCH)
    return true
  }

  return false
}

function showNetworkWarning() {
  const element = document.getElementById('network-warning')
  if (element) {
    element.style.display = 'block'
  }
}
