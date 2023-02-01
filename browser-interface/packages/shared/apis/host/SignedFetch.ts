import { signedFetch } from '../../../atomicHelpers/signedFetch'
import { ETHEREUM_NETWORK } from '../../../config'
import { getSelectedNetwork } from '../../dao/selectors'
import { store } from '../../store/isolatedStore'
import { getIsGuestLogin } from '../../session/selectors'
import { onLoginCompleted } from '../../session/onLoginCompleted'

import { RpcServerPort } from '@dcl/rpc'
import { PortContext } from './context'
import * as codegen from '@dcl/rpc/dist/codegen'

import { SignedFetchServiceDefinition } from '@dcl/protocol/out-ts/decentraland/kernel/apis/signed_fetch.gen'
import { getRealmAdapter } from 'shared/realm/selectors'
import { Realm } from 'shared/dao/types'

export function registerSignedFetchServiceServerImplementation(port: RpcServerPort<PortContext>) {
  codegen.registerService(port, SignedFetchServiceDefinition, async () => ({
    async signedFetch(req, ctx) {
      const { identity } = await onLoginCompleted()

      const state = store.getState()
      const realmAdapter = getRealmAdapter(state)

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
      const isGuest = !!getIsGuestLogin(state)
      const network = getSelectedNetwork(state)

      const compatibilityRealm:
        | {
            domain: string
            layer: string
            catalystName: string
          }
        | undefined = realm ? { domain: realm.hostname, layer: '', catalystName: realm.serverName } : undefined

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

      //   return this.parcelIdentity.land.sceneJsonData
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
