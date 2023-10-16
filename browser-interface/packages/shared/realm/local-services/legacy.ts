import { ETHEREUM_NETWORK } from 'config'
import { AboutResponse } from 'shared/protocol/decentraland/renderer/about.gen'
import { LegacyServices } from '../types'

export function legacyServices(network: ETHEREUM_NETWORK, baseUrl: string, about: AboutResponse): LegacyServices {
  const contentServer = about.content?.publicUrl || baseUrl + '/content'
  const lambdasServer = about.lambdas?.publicUrl || baseUrl + '/lambdas'

  const tld = network === ETHEREUM_NETWORK.MAINNET ? 'org' : 'zone'
  const realmProviderUrl = `https://realm-provider.decentraland.${tld}`

  return {
    fetchContentServer: contentServer,
    updateContentServer: contentServer,
    lambdasServer,
    poiService: lambdasServer + '/contracts/pois',
    exploreRealmsService: `${realmProviderUrl}/realms`,
    hotScenesService: `${realmProviderUrl}/hot-scenes`
  }
}
