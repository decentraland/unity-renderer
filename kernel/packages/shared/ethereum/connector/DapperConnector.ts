import { ConnectorInterface } from './ConnectorInterface'

declare var window: Window & {
  ethereum: any
}

export class DapperConnector implements ConnectorInterface {
  isAvailable() {
    return window['ethereum'] && window.ethereum.isDapper
  }

  getProvider() {
    return window.ethereum
  }

  async login() {
    return window.ethereum.enable()
  }

  async logout() {
    return true
  }
}
