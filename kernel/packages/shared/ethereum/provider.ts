import { WebSocketProvider, RequestManager } from 'eth-connect'
import { future } from 'fp-future'

import { ethereumConfigurations, ETHEREUM_NETWORK } from 'config'
import { defaultLogger } from 'shared/logger'
import { Account } from 'web3x/account'

declare var window: Window & {
  ethereum: any
  web3: any
}

export const providerFuture = future()
export const requestManager = new RequestManager(null)

let providerRequested = false

export async function awaitWeb3Approval(): Promise<void> {
  if (!providerRequested) {
    providerRequested = true
    // Modern dapp browsers...
    if (window['ethereum']) {
      window['ethereum'].autoRefreshOnNetworkChange = false
      const element = document.getElementById('eth-login')
      if (element) {
        element.style.display = 'block'
        const button = document.getElementById('eth-login-confirm-button')

        let response = future()

        button!.onclick = async () => {
          let result
          try {
            // Request account access if needed
            await window.ethereum.enable()

            result = { successful: true, provider: window.ethereum }
          } catch (error) {
            // User denied account access...
            result = {
              successful: false,
              provider: new WebSocketProvider(ethereumConfigurations[ETHEREUM_NETWORK.MAINNET].wss)
            }
          }
          response.resolve(result)
        }

        let result
        while (true) {
          result = await response

          element.style.display = 'none'

          const button = document.getElementById('eth-relogin-confirm-button')

          response = future()

          button!.onclick = async () => {
            let result
            try {
              // Request account access if needed
              await window.ethereum.enable()

              result = { successful: true, provider: window.ethereum }
            } catch (error) {
              // User denied account access, need to retry...
              result = { successful: false }
            }
            response.resolve(result)
          }

          if (result.successful) {
            break
          } else {
            showEthConnectAdvice(true)
          }
        }
        showEthConnectAdvice(false)
        providerFuture.resolve(result)
      }
    } else if (window.web3 && window.web3.currentProvider) {
      // legacy providers (don't need for confirmation)
      providerFuture.resolve({ successful: true, provider: window.web3.currentProvider })
    } else {
      // otherwise, create a local identity
      providerFuture.resolve({
        successful: false,
        provider: new WebSocketProvider(ethereumConfigurations[ETHEREUM_NETWORK.MAINNET].wss),
        localIdentity: Account.create()
      })
    }
  }

  providerFuture.then(result => requestManager.setProvider(result.provider)).catch(defaultLogger.error)

  return providerFuture
}

function showEthConnectAdvice(show: boolean) {
  const element = document.getElementById('eth-connect-advice')
  if (element) {
    element.style.display = show ? 'block' : 'none'
  }
}
