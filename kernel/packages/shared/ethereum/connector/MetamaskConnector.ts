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
    const accounts = await window.ethereum.request({ method: 'eth_requestAccounts' })
    this.subscribeToChanges()
    return accounts
  }

  async logout() {
    return true
  }

  private subscribeToChanges() {
    window.ethereum.on('accountsChanged', (accounts: string[]) => location.reload())
    window.ethereum.on('disconnect', (code: number, reason: string) => location.reload())
  }
}
