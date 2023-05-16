import { AboutResponse } from 'shared/protocol/decentraland/realm/about.gen'
import { LegacyServices } from '../types'

export function legacyServices(baseUrl: string, about: AboutResponse): LegacyServices {
  const contentServer = about.content?.publicUrl || baseUrl + '/content'
  const lambdasServer = about.lambdas?.publicUrl || baseUrl + '/lambdas'

  return {
    fetchContentServer: contentServer,
    updateContentServer: contentServer,
    lambdasServer,
    hotScenesService: lambdasServer + '/explore/hot-scenes',
    poiService: lambdasServer + '/contracts/pois',
    exploreRealmsService: lambdasServer + '/explore/realms'
  }
}
