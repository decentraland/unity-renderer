import { put, takeLatest, call, delay, select } from 'redux-saga/effects'
import { createIdentity } from 'eth-crypto'
import { Personal } from 'web3x/personal/personal'
import { Account } from 'web3x/account'
import { Authenticator } from 'dcl-crypto'

import { ENABLE_WEB3, WORLD_EXPLORER, PREVIEW, ETHEREUM_NETWORK, getTLD, setNetwork } from 'config'

import { createLogger } from 'shared/logger'
import { referUser, initializeReferral } from 'shared/referral'
import {
  awaitWeb3Approval,
  isSessionExpired,
  providerFuture,
  loginCompleted,
  getUserEthAccountIfAvailable,
  createEthUsingWalletProvider
} from 'shared/ethereum/provider'
import { setLocalInformationForComms } from 'shared/comms/peers'
import { ReportFatalError } from 'shared/loading/ReportFatalError'
import {
  AUTH_ERROR_LOGGED_OUT,
  NETWORK_MISMATCH,
  awaitingUserSignature,
  AWAITING_USER_SIGNATURE
} from 'shared/loading/types'
import { identifyUser, queueTrackingEvent } from 'shared/analytics'
import { getNetworkFromTLD, getAppNetwork } from 'shared/web3'
import { getNetwork } from 'shared/ethereum/EthereumService'

import { getFromLocalStorage, saveToLocalStorage } from 'atomicHelpers/localStorage'

import { ExplorerIdentity } from './types'
import { userAuthentified, LOGOUT, LOGIN, loginCompleted as loginCompletedAction } from './actions'
import { getLastSessionWithoutWallet, getStoredSession, Session, setStoredSession } from './index'

const logger = createLogger('session: ')

export function* sessionSaga(): any {
  yield call(initializeTos)
  yield call(initializeReferral)

  yield takeLatest(LOGIN, login)
  yield takeLatest(LOGOUT, logout)
  yield takeLatest(AWAITING_USER_SIGNATURE, scheduleAwaitingSignaturePrompt)
}

function* initializeTos() {
  const TOS_KEY = 'tos'
  const tosAgreed: boolean = getFromLocalStorage(TOS_KEY) ?? false

  const agreeCheck = document.getElementById('agree-check') as HTMLInputElement | undefined
  if (agreeCheck) {
    agreeCheck.checked = tosAgreed
    // @ts-ignore
    agreeCheck.onchange && agreeCheck.onchange()

    const originalOnChange = agreeCheck.onchange
    agreeCheck.onchange = (e) => {
      saveToLocalStorage(TOS_KEY, agreeCheck.checked)
      // @ts-ignore
      originalOnChange && originalOnChange(e)
    }

    // enable agree check after initialization
    enableLogin()
  }
}

function* scheduleAwaitingSignaturePrompt() {
  yield delay(10000)
  const isStillWaiting = yield select((state) => !state.session?.initialized)

  if (isStillWaiting) {
    showAwaitingSignaturePrompt(true)
  }
}

function* login() {
  let userId: string
  let identity: ExplorerIdentity

  if (ENABLE_WEB3) {
    yield awaitWeb3Approval()

    if (WORLD_EXPLORER && (yield checkTldVsNetwork())) {
      throw new Error('Network mismatch')
    }

    if (PREVIEW && ETHEREUM_NETWORK.MAINNET === (yield getNetworkValue())) {
      showNetworkWarning()
    }

    try {
      const address = yield getUserEthAccountIfAvailable()
      const userData = address ? getStoredSession(address) : getLastSessionWithoutWallet()

      // check that user data is stored & key is not expired
      if (isSessionExpired(userData)) {
        yield put(awaitingUserSignature())
        identity = yield createAuthIdentity()
        showAwaitingSignaturePrompt(false)
        userId = identity.address

        saveSession(userId, identity)
      } else {
        identity = userData!.identity
        userId = userData!.identity.address

        saveSession(userId, identity)
      }
    } catch (e) {
      logger.error(e)
      ReportFatalError(AUTH_ERROR_LOGGED_OUT)
      throw e
    }

    if (identity.hasConnectedWeb3) {
      identifyUser(userId)
      referUser(identity)
    }
  } else {
    logger.log(`Using test user.`)
    identity = yield createAuthIdentity()
    userId = identity.address

    saveSession(userId, identity)

    loginCompleted.resolve()
  }

  logger.log(`User ${userId} logged in`)

  let net: ETHEREUM_NETWORK = ETHEREUM_NETWORK.MAINNET
  if (WORLD_EXPLORER) {
    net = yield getAppNetwork()

    // Load contracts from https://contracts.decentraland.org
    yield setNetwork(net)
    queueTrackingEvent('Use network', { net })
  }
  yield put(userAuthentified(userId, identity, net))

  yield loginCompleted
  yield put(loginCompletedAction())
}

function saveSession(userId: string, identity: ExplorerIdentity) {
  setStoredSession({
    userId,
    identity
  })

  setLocalInformationForComms(userId, {
    userId,
    identity
  })
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

async function getNetworkValue() {
  const web3Network = await getNetwork()
  const web3Net = web3Network === '1' ? ETHEREUM_NETWORK.MAINNET : ETHEREUM_NETWORK.ROPSTEN
  return web3Net
}

function showNetworkWarning() {
  const element = document.getElementById('network-warning')
  if (element) {
    element.style.display = 'block'
  }
}

async function createAuthIdentity() {
  const ephemeral = createIdentity()

  let ephemeralLifespanMinutes = 7 * 24 * 60 // 1 week

  let address
  let signer
  let hasConnectedWeb3 = false

  if (ENABLE_WEB3) {
    const result = await providerFuture
    if (result.successful) {
      const eth = createEthUsingWalletProvider()!
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
        showEthSignAdvice(false)
        return result
      }
      hasConnectedWeb3 = true
    } else {
      const account: Account = result.localIdentity

      address = account.address.toJSON()
      signer = async (message: string) => account.sign(message).signature

      // If we are using a local profile, we don't want the identity to expire.
      // Eventually, if a wallet gets created, we can migrate the profile to the wallet.
      ephemeralLifespanMinutes = 365 * 24 * 60 * 99
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

function showEthSignAdvice(show: boolean) {
  showElementById('eth-sign-advice', show)
}

function showElementById(id: string, show: boolean) {
  const element = document.getElementById(id)
  if (element) {
    element.style.display = show ? 'block' : 'none'
  }
}

function showAwaitingSignaturePrompt(show: boolean) {
  showElementById('check-wallet-prompt', show)
}

function* logout() {
  Session.current.logout().catch((e) => logger.error('error while logging out', e))
}

function enableLogin() {
  const wrapper = document.getElementById('eth-login-confirmation-wrapper')
  const spinner = document.getElementById('eth-login-confirmation-spinner')
  if (wrapper && spinner) {
    spinner.style.cssText = 'display: none;'
    wrapper.style.cssText = 'display: flex;'
  }
}
