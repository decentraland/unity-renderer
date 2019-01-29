import { WebSocketProvider, RequestManager } from 'eth-connect'
import { error } from 'engine/logger'
import { future } from 'fp-future'
import { EDITOR } from 'config'

export const providerFuture = future()
export const requestManager = new RequestManager(null)

{
  if (!EDITOR) {
    window.addEventListener('load', async () => {
      // Modern dapp browsers...
      if (window['ethereum']) {
        try {
          // Request account access if needed
          await window['ethereum'].enable()
          providerFuture.resolve(window['ethereum'])
        } catch (error) {
          // User denied account access...
          providerFuture.reject(error)
        }
      } else if (window['web3']) {
        providerFuture.resolve(window['web3'].currentProvider)
      } else {
        // tslint:disable-next-line:no-console
        console.log('Non-Ethereum browser detected. You should consider trying MetaMask!')
        providerFuture.resolve(new WebSocketProvider('wss://mainnet.infura.io/ws'))
      }
    })

    providerFuture.then(provider => requestManager.setProvider(provider)).catch(error)
  }
}

export async function awaitWeb3Approval() {
  await providerFuture
}
