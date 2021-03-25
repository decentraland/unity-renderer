import { ExposableAPI } from './ExposableAPI'
import { exposeMethod, registerAPI } from 'decentraland-rpc/lib/host'
import { AuthChain, Authenticator } from 'dcl-crypto'
import { ParcelIdentity } from './ParcelIdentity'
import { userAuthentified } from 'shared/session'
import { getIdentity } from 'shared/session'
import { flatFetch, FlatFetchInit, FlatFetchResponse } from 'atomicHelpers/flatFetch'
import { getTLD } from '../../config'
import { Store } from 'redux'
import { RootState } from 'shared/store/rootTypes'
import { getRealm } from 'shared/dao/selectors'
import { isGuest } from 'shared/ethereum/provider'

declare const window: any

const AUTH_CHAIN_HEADER_PREFIX = 'x-identity-auth-chain-'
const AUTH_TIMESTAMP_HEADER = 'x-identity-timestamp'
const AUTH_METADATA_HEADER = 'x-identity-metadata'

function getAuthHeaders(
  method: string,
  path: string,
  metadata: Record<string, any>,
  chainProvider: (payload: string) => AuthChain
) {
  const headers: Record<string, string> = {}
  const timestamp = Date.now()
  const metadataJSON = JSON.stringify(metadata)
  const payloadParts = [method.toLowerCase(), path.toLowerCase(), timestamp.toString(), metadataJSON]
  const payloadToSign = payloadParts.join(':').toLowerCase()

  const chain = chainProvider(payloadToSign)

  chain.forEach((link, index) => {
    headers[`${AUTH_CHAIN_HEADER_PREFIX}${index}`] = JSON.stringify(link)
  })

  headers[AUTH_TIMESTAMP_HEADER] = timestamp.toString()
  headers[AUTH_METADATA_HEADER] = metadataJSON

  return headers
}

@registerAPI('SignedFetch')
export class SignedFetch extends ExposableAPI {
  parcelIdentity = this.options.getAPIInstance(ParcelIdentity)

  @exposeMethod
  async signedFetch(url: string, init?: FlatFetchInit): Promise<FlatFetchResponse> {
    await userAuthentified()

    const store: Store<RootState> = window['globalStore']
    const realm = getRealm(store.getState())

    const identity = getIdentity()!
    const path = new URL(url).pathname
    const actualInit = {
      ...init,
      headers: {
        ...getAuthHeaders(
          init?.method ?? 'get',
          path,
          {
            sceneId: this.parcelIdentity.cid,
            parcel: this.getSceneData().scene.base,
            tld: getTLD(),
            isGuest: isGuest(),
            origin: location.origin,
            realm
          },
          (payload) => Authenticator.signPayload(identity, payload)
        ),
        ...init?.headers
      }
    } as FlatFetchInit
    return flatFetch(url, actualInit)
  }

  private getSceneData() {
    return this.parcelIdentity.land.sceneJsonData
  }
}
