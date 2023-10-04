import { ETHEREUM_NETWORK } from 'config'
import { AboutResponse } from 'shared/protocol/decentraland/renderer/about.gen'
import { LegacyServices } from '../types'
import { store } from 'shared/store/isolatedStore'
import { getSelectedNetwork } from 'shared/dao/selectors'

export function legacyServices(baseUrl: string, about: AboutResponse): LegacyServices {
  const contentServer = about.content?.publicUrl || baseUrl + '/content'
  const lambdasServer = about.lambdas?.publicUrl || baseUrl + '/lambdas'

  const state = store.getState()
  const tld = getSelectedNetwork(state) === ETHEREUM_NETWORK.MAINNET ? 'org' : 'zone'

  return {
    fetchContentServer: contentServer,
    updateContentServer: contentServer,
    lambdasServer,
    poiService: lambdasServer + '/contracts/pois',
    exploreRealmsService: `https://realm-provider.decentraland.${tld}/realms`,
    hotScenesService: `https://realm-provider.decentraland.${tld}/hot-scenes`
  }
}
