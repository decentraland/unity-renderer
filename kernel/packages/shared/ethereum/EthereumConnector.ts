import { Eth } from 'web3x/eth'
import { WebSocketProvider } from 'eth-connect'
import { LegacyProviderAdapter } from 'web3x/providers'
import { getNetworkFromTLDOrWeb3 } from 'atomicHelpers/getNetworkFromTLDOrWeb3'
import { connection, ConnectionResponse, ProviderType } from 'decentraland-connect'
import { ChainId } from '@dcl/schemas'
import { ETHEREUM_NETWORK, ethereumConfigurations } from '../../config'

export class EthereumConnector {
  private type: ProviderType | null = null
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
    return this.network === ETHEREUM_NETWORK.MAINNET ? ChainId.ETHEREUM_MAINNET : ChainId.ETHEREUM_ROPSTEN
  }

  async restoreConnection() {
    try {
      this.result = await connection.tryPreviousConnection()
      this.type = connection.getConnectionData()!.providerType
      return true
    } catch (err) {
      return false
    }
  }

  async connect(type: ProviderType | null) {
    this.type = type
    if (type === null) {
      this.result = {
        chainId: this.getChainId(),
        providerType: ProviderType.INJECTED,
        account: null,
        provider: EthereumConnector.createWeb3xWebsocketProvider()
      }

      return this.result
    }

    this.result = await connection.connect(type, this.getChainId())
    return this.result
  }

  isConnected() {
    return !!this.result
  }

  async disconnect() {
    await connection.disconnect()
    this.type = null
    this.result = undefined
  }

  isType(type: ProviderType | null) {
    return this.type === type
  }

  isGuest() {
    return this.type === null
  }

  createEth(provider: any = false): Eth | null {
    if (provider) {
      return new Eth(provider)
    }

    if (!this.result || this.isGuest()) {
      return null
    }

    if (this.isType(ProviderType.INJECTED)) {
      return new Eth(new LegacyProviderAdapter((window as any).ethereum))
    }

    return new Eth(this.result.provider)
  }
}
