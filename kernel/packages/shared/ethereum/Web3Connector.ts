import { Eth } from 'web3x/eth'
import { LegacyProviderAdapter } from 'web3x/providers'
import { ETHEREUM_NETWORK, ethereumConfigurations, getTLD, WALLET_API_KEYS } from '../../config'
import { WebSocketProvider } from 'eth-connect'
import { ConnectorFactory } from './connector/ConnectorFactory'
import { ProviderType } from './ProviderType'
import { ConnectorInterface } from './connector/ConnectorInterface'

export class Web3Connector {
  private type: ProviderType | undefined
  private factory: ConnectorFactory
  private connector: ConnectorInterface | undefined
  private readonly network: ETHEREUM_NETWORK

  constructor() {
    this.network = getTLD() === 'zone' ? ETHEREUM_NETWORK.ROPSTEN : ETHEREUM_NETWORK.MAINNET
    this.factory = new ConnectorFactory(WALLET_API_KEYS.get(this.network)!)
  }

  static createWebSocketProvider() {
    const network = getTLD() === 'zone' ? ETHEREUM_NETWORK.ROPSTEN : ETHEREUM_NETWORK.MAINNET
    return new WebSocketProvider(ethereumConfigurations[network].wss)
  }

  getType() {
    return this.type
  }

  async connect(type: ProviderType) {
    this.type = type
    this.connector = this.factory.create(this.type, this.network)
    if (!this.connector.isAvailable()) {
      // guest
      this.type = ProviderType.GUEST
      this.connector = this.factory.create(this.type, this.network)
    }
    await this.connector.login()
    return this.connector.getProvider()
  }

  isType(type: ProviderType) {
    return this.type === type
  }

  createEth(provider: any = false): Eth | undefined {
    if (provider) {
      return new Eth(provider)
    }
    if (!this.connector || this.isType(ProviderType.GUEST)) {
      return undefined
    }
    if (this.isType(ProviderType.METAMASK)) {
      return new Eth(new LegacyProviderAdapter((window as any).ethereum))
    }
    return new Eth(this.connector.getProvider())
  }
}
