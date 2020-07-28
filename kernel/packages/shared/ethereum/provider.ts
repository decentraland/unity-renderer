import { WebSocketProvider, RequestManager } from 'eth-connect'
import { future, IFuture } from 'fp-future'

import { ethereumConfigurations, ETHEREUM_NETWORK } from 'config'
import { defaultLogger } from 'shared/logger'
import { Account } from 'web3x/account'
import { getUserProfile } from 'shared/comms/peers'
import { getTLD } from '../../config/index'
import { removeUserProfile } from '../comms/peers'
import { Eth } from 'web3x/eth'

declare var window: Window & {
  ethereum: any
  web3: any
}

export const providerFuture = future()
export const requestManager = new RequestManager(null)

export const loginCompleted = future<void>()
;(window as any).loginCompleted = loginCompleted

let providerRequested = false

type LoginData = { successful: boolean; provider: any; localIdentity?: Account }

function processLoginAttempt(response: IFuture<LoginData>, backgroundLogin: IFuture<LoginData>) {
  return async () => {
    if (!backgroundLogin.isPending) {
      backgroundLogin
        .then((result) => response.resolve(result))
        .catch((e) => defaultLogger.error('could not resolve login', e))
      return
    }

    // TODO - look for user id matching account - moliva - 18/02/2020
    let userData = getUserProfile()

    // Modern dapp browsers...
    if (window['ethereum'] && isSessionExpired(userData)) {
      showEthConnectAdvice(false)

      let result
      try {
        // Request account access if needed
        const accounts: string[] | undefined = await window.ethereum.enable()

        if (accounts && accounts.length > 0) {
          result = { successful: true, provider: window.ethereum }
        } else {
          // whether accounts is undefined or empty array => provider not enabled
          result = {
            successful: false,
            provider: createProvider()
          }
        }
      } catch (error) {
        // User denied account access...
        result = {
          successful: false,
          provider: createProvider()
        }
      }
      backgroundLogin.resolve(result)
      response.resolve(result)
    } else {
      defaultLogger.error('invalid login state!')
    }
  }
}

function processLoginBackground() {
  const response = future()

  const userData = getUserProfile()
  if (window['ethereum']) {
    if (!isSessionExpired(userData)) {
      response.resolve({ successful: true, provider: window.ethereum })
    }
  } else if (window.web3 && window.web3.currentProvider) {
    removeSessionIfNotValid()
      .then(() => {
        // legacy providers (don't need for confirmation)
        response.resolve({ successful: true, provider: window.web3.currentProvider })
      })
      .catch((e) => response.reject(e))
  } else {
    // otherwise, create a local identity
    response.resolve({
      successful: false,
      provider: createProvider(),
      localIdentity: Account.create()
    })
  }

  return response
}

export function awaitWeb3Approval(): Promise<void> {
  if (!providerRequested) {
    providerRequested = true

    new Promise(async () => {
      const element = document.getElementById('eth-login')
      if (element) {
        element.style.display = 'block'

        if (window['ethereum']) {
          await removeSessionIfNotValid()
          window['ethereum'].autoRefreshOnNetworkChange = false
        }

        const background = processLoginBackground()
        background.then((result) => providerFuture.resolve(result)).catch((e) => providerFuture.reject(e))

        const button = document.getElementById('eth-login-confirm-button')

        let response = future()

        button!.onclick = processLoginAttempt(response, background)

        let result
        while (true) {
          result = await response

          element.style.display = 'none'

          const button = document.getElementById('eth-relogin-confirm-button')

          response = future()

          button!.onclick = processLoginAttempt(response, background)

          // if the user signed properly or doesn't have a wallet => move on with login
          if (result.successful || !window['ethereum']) {
            break
          } else {
            showEthConnectAdvice(true)
          }
        }

        showEthConnectAdvice(false)

        // post check
        if (window['ethereum']) {
          registerProviderChanges()
        }
      } else {
        // otherwise, login element not found (preview, builder)
        providerFuture.resolve({
          successful: false,
          provider: createProvider(),
          localIdentity: Account.create()
        })
      }

      loginCompleted.resolve()
    }).catch((e) => defaultLogger.error('error in login process', e))
  }

  providerFuture.then((result) => requestManager.setProvider(result.provider)).catch(defaultLogger.error)

  return providerFuture
}

/**
 * Remove local session if persisted account does not match with one or ephemeral key is expired
 */
async function removeSessionIfNotValid() {
  const account = await getUserAccount()

  // TODO - look for user id matching account - moliva - 18/02/2020
  let userData = getUserProfile()

  if ((userData && userData.userId !== account) || isSessionExpired(userData)) {
    removeUserProfile()
  }
}

/**
 * Register to any change in the configuration of the wallet to reload the app and avoid wallet changes in-game.
 */
function registerProviderChanges() {
  if (window.ethereum && typeof window.ethereum.on === 'function') {
    window.ethereum.on('accountsChanged', (accounts: string[]) => location.reload())
    window.ethereum.on('networkChanged', (networkId: string) => location.reload())
    window.ethereum.on('close', (code: number, reason: string) => location.reload())
  }
}

function createProvider() {
  const network = getTLD() === 'zone' ? ETHEREUM_NETWORK.ROPSTEN : ETHEREUM_NETWORK.MAINNET
  return new WebSocketProvider(ethereumConfigurations[network].wss)
}

function showEthConnectAdvice(show: boolean) {
  const element = document.getElementById('eth-connect-advice')
  if (element) {
    element.style.display = show ? 'block' : 'none'
  }
}

export function isSessionExpired(userData: any) {
  return !userData || !userData.identity || new Date(userData.identity.expiration) < new Date()
}

export async function getUserAccount(): Promise<string | undefined> {
  try {
    const eth = Eth.fromCurrentProvider()!
    const accounts = await eth.getAccounts()

    if (!accounts || accounts.length === 0) {
      return undefined
    }

    return accounts[0].toJSON().toLocaleLowerCase()
  } catch (error) {
    throw new Error(`Could not access eth_accounts: "${error.message}"`)
  }
}
