import { urlWithProtocol } from 'shared/realm/resolver'
import { AboutResponse } from '@dcl/protocol/out-ts/decentraland/bff/http_endpoints.gen'
import { OFFLINE_REALM } from 'shared/realm/types'

export async function resolveOfflineRealmAboutFromConnectionString(
  realmString: string
): Promise<{ about: AboutResponse; baseUrl: string } | undefined> {
  if (realmString === OFFLINE_REALM || realmString.startsWith(OFFLINE_REALM + '?')) {
    const params = new URL('decentraland:' + realmString).searchParams
    let baseUrl = urlWithProtocol(params.get('baseUrl') || 'https://peer.decentraland.org')

    if (!baseUrl.endsWith('/')) baseUrl = baseUrl + '/'

    return {
      about: {
        bff: undefined,
        comms: {
          healthy: false,
          protocol: params.get('protocol') || 'offline',
          fixedAdapter: params.get('fixedAdapter') || 'offline:offline'
        },
        configurations: {
          realmName: realmString,
          networkId: 1,
          globalScenesUrn: [],
          scenesUrn: [],
          cityLoaderContentServer: params.get('cityLoaderContentServer') ?? undefined
        },
        content: {
          healthy: true,
          publicUrl: `${baseUrl}content`
        },
        healthy: true,
        lambdas: {
          healthy: true,
          publicUrl: `${baseUrl}lambdas`
        },
        acceptingUsers: true
      },
      baseUrl: baseUrl.replace(/\/+$/, '')
    }
  }
}
