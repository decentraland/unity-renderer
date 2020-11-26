import { MetamaskConnector } from './MetamaskConnector'
import { DapperConnector } from './DapperConnector'
import { ConnectorInterface } from './ConnectorInterface'
import { ProviderType } from '../ProviderType'
import { GuestConnector } from './GuestConnector'
import { ETHEREUM_NETWORK, ethereumConfigurations } from '../../../config'
import { FortmaticConnector } from './FortmaticConnector'

export class ConnectorFactory {
  private keys: Map<string, string>

  constructor(apiKeys: Map<string, string>) {
    this.keys = apiKeys
  }

  create(type: ProviderType, network: ETHEREUM_NETWORK): ConnectorInterface {
    if (type === ProviderType.METAMASK) {
      return new MetamaskConnector()
    }
    if (type === ProviderType.DAPPER) {
      return new DapperConnector()
    }
    if (type === ProviderType.GUEST) {
      return new GuestConnector(ethereumConfigurations[network].wss)
    }
    const config = new Map<string, string>()
    config.set('network', network)
    config.set('apiKey', this.keys.get(type) as string)
    if (type === ProviderType.FORTMATIC) {
      return new FortmaticConnector(config)
    }
    throw new Error('Invalid provider type')
  }
}
