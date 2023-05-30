import { Authenticator } from '@dcl/crypto'
import { getSignedHeaders, signedFetch } from 'lib/decentraland/authentication/signedFetch'
import { ETHEREUM_NETWORK } from 'config'
import { getSelectedNetwork } from 'shared/dao/selectors'
import { store } from 'shared/store/isolatedStore'
import { isGuestLogin } from 'shared/session/selectors'
import { onLoginCompleted } from 'shared/session/onLoginCompleted'

import type { RpcServerPort } from '@dcl/rpc'
import type { PortContext } from './context'
import * as codegen from '@dcl/rpc/dist/codegen'

import { SignedFetchServiceDefinition } from 'shared/protocol/decentraland/kernel/apis/signed_fetch.gen'
import { getRealmAdapter } from 'shared/realm/selectors'
import type { Realm } from 'shared/dao/types'

function getMetadata(ctx: Pick<PortContext, 'sceneData'>) {
  const state = store.getState()
  const realmAdapter = getRealmAdapter(state)
  const isGuest = !!isGuestLogin(state)
  const network = getSelectedNetwork(state)
  const realm: Realm = realmAdapter
    ? {
      hostname: new URL(realmAdapter?.baseUrl).hostname,
      protocol: realmAdapter.about.comms?.protocol || 'v3',
      serverName: realmAdapter.about.configurations?.realmName || realmAdapter.baseUrl
    }
    : {
      hostname: 'offline',
      protocol: 'offline',
      serverName: 'offline'
    }
  const compatibilityRealm: { domain: string; layer: string; catalystName: string } | undefined = realm
    ? { domain: realm.hostname, layer: '', catalystName: realm.serverName }
    : undefined
  const additionalMetadata: Record<string, any> = {
    sceneId: ctx.sceneData.id,
    parcel: ctx.sceneData.entity.metadata.scene.base,
    // THIS WILL BE DEPRECATED
    tld: network === ETHEREUM_NETWORK.MAINNET ? 'org' : 'zone',
    network,
    isGuest,
    realm: realm?.protocol === 'v2' || realm?.protocol === 'v1' ? compatibilityRealm : realm,
    signer: 'decentraland-kernel-scene'
  }

  return additionalMetadata
}

export function registerSignedFetchServiceServerImplementation(port: RpcServerPort<PortContext>) {
  codegen.registerService(port, SignedFetchServiceDefinition, async () => ({
    async getHeaders(req, ctx) {
      const { identity } = await onLoginCompleted()

      if (!identity) {
        return { headers: {} }
      }

      const method = req.init?.method ?? 'get'
      const path = new URL(req.url).pathname
      const metadata = getMetadata(ctx)
      const headers = getSignedHeaders(
        method,
        path,
        {
          origin: location.origin,
          ...metadata
        },
        (payload) => Authenticator.signPayload(identity, payload)
      )
      return { headers }
    },
    async signedFetch(req, ctx) {
      const { identity } = await onLoginCompleted()
      const additionalMetadata = getMetadata(ctx)

      const result = await signedFetch(req.url, identity!, req.init, additionalMetadata)

      return {
        ok: result.ok,
        status: result.status,
        statusText: result.statusText,
        headers: result.headers,
        body: result.text || '{}'
      }
    }
  }))
}
