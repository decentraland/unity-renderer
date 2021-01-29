import { Eth } from 'web3x/eth'
import { WebSocketProvider } from 'eth-connect'
import { LegacyProviderAdapter } from 'web3x/providers'
import { ETHEREUM_NETWORK, ethereumConfigurations } from '../../config'
import { ProviderType } from './ProviderType'
import { getNetworkFromTLDOrWeb3 } from 'atomicHelpers/getNetworkFromTLDOrWeb3'
import { ChainId, connection, ConnectionResponse } from 'decentraland-connect'

export class Web3Connector {
  private type: ProviderType | undefined
  private result: ConnectionResponse | undefined
  private readonly network: ETHEREUM_NETWORK

  constructor() {
    this.network = getNetworkFromTLDOrWeb3()
  }

  static createWeb3xWebsocketProvider() {
    const network = getNetworkFromTLDOrWeb3()
    return new WebSocketProvider(ethereumConfigurations[network].wss) as any
  }

  getType() {
    return this.type
  }

  getChainId(): ChainId {
    return this.network === ETHEREUM_NETWORK.MAINNET ? ChainId.MAINNET : ChainId.ROPSTEN
  }

  async connect(type: ProviderType) {
    this.type = type
    if (type === ProviderType.GUEST) {
      this.result = {
        chainId: this.getChainId(),
        account: null,
        provider: Web3Connector.createWeb3xWebsocketProvider()
      }
      return this.result
    }
    this.result = await connection.connect(type as any, this.getChainId())
    return this.result
  }

  isType(type: ProviderType) {
    return this.type === type
  }

  createEth(provider: any = false): Eth | undefined {
    if (provider) {
      return new Eth(provider)
    }
    if (!this.result || this.isType(ProviderType.GUEST)) {
      return undefined
    }
    if (this.isType(ProviderType.INJECTED)) {
      return new Eth(new LegacyProviderAdapter((window as any).ethereum))
    }
    return new Eth(this.result.provider)
  }
}
