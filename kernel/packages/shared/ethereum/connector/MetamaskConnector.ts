import { ConnectorInterface } from './ConnectorInterface'

declare var window: Window & {
  ethereum: any
  web3: any
}

export class MetamaskConnector implements ConnectorInterface {
  isAvailable() {
    return window['ethereum'] && window.ethereum.isMetaMask
  }

  getProvider() {
    return window.ethereum
  }

  async login() {
    return window.ethereum.request({ method: 'eth_requestAccounts' })
  }

  async logout() {
    return true
  }
}
