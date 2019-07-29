import { WebSocketProvider, RequestManager } from 'eth-connect'
import { future } from 'fp-future'

import { ethereumConfigurations, ETHEREUM_NETWORK } from 'config'
import { defaultLogger } from 'shared/logger'

declare var window: Window & {
  ethereum: any
  web3: any
}

export const providerFuture = future()
export const requestManager = new RequestManager(null)

let providerRequested = false

export async function awaitWeb3Approval() {
  if (!providerRequested) {
    providerRequested = true
    // Modern dapp browsers...
    if (window['ethereum']) {
      try {
        // Request account access if needed
        await window.ethereum.enable()
        providerFuture.resolve(window.ethereum)
      } catch (error) {
        // User denied account access...
        providerFuture.resolve(new WebSocketProvider(ethereumConfigurations[ETHEREUM_NETWORK.MAINNET].wss))
      }
    } else if (window.web3 && window.web3.currentProvider) {
      providerFuture.resolve(window.web3.currentProvider)
    } else {
      providerFuture.resolve(new WebSocketProvider(ethereumConfigurations[ETHEREUM_NETWORK.MAINNET].wss))
    }
  }

  providerFuture.then(provider => requestManager.setProvider(provider)).catch(defaultLogger.error)
}
