import { Eth } from 'web3x/eth'
import { LegacyProviderAdapter, WebsocketProvider } from 'web3x/providers'
import { ETHEREUM_NETWORK, ethereumConfigurations, WALLET_API_KEYS } from '../../config'
import { ConnectorFactory } from './connector/ConnectorFactory'
import { ProviderType } from './ProviderType'
import { ConnectorInterface } from './connector/ConnectorInterface'
import { getNetworkFromTLDOrWeb3 } from 'atomicHelpers/getNetworkFromTLDOrWeb3'

export class Web3Connector {
  private type: ProviderType | undefined
  private factory: ConnectorFactory
  private connector: ConnectorInterface | undefined
  private readonly network: ETHEREUM_NETWORK

  constructor() {
    this.network = getNetworkFromTLDOrWeb3()
    this.factory = new ConnectorFactory(WALLET_API_KEYS.get(this.network)!)
  }

  static createWeb3xWebsocketProvider() {
    const network = getNetworkFromTLDOrWeb3()
    return new WebsocketProvider(ethereumConfigurations[network].wss)
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
    if (this.isType(ProviderType.METAMASK) || this.isType(ProviderType.DAPPER)) {
      return new Eth(new LegacyProviderAdapter((window as any).ethereum))
    }
    return new Eth(this.connector.getProvider())
  }
}
