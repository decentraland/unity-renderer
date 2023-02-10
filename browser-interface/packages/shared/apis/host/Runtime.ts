import { store } from 'shared/store/isolatedStore'
import { getRealmAdapter } from 'shared/realm/selectors'
import * as codegen from '@dcl/rpc/dist/codegen'
import type { RpcServerPort } from '@dcl/rpc/dist/types'
import type { GetRealmResponse, GetWorldTimeResponse } from '@dcl/protocol/out-ts/decentraland/kernel/apis/runtime.gen'
import { RuntimeServiceDefinition } from '@dcl/protocol/out-ts/decentraland/kernel/apis/runtime.gen'
import type { PortContextService } from './context'
import { getDecentralandTime } from './EnvironmentAPI'
import { urlWithProtocol } from 'shared/realm/resolver'
import { PREVIEW } from 'config'

export function registerRuntimeServiceServerImplementation(port: RpcServerPort<PortContextService<'sceneData'>>) {
  codegen.registerService(port, RuntimeServiceDefinition, async () => ({
    async getWorldTime(): Promise<GetWorldTimeResponse> {
      const time = getDecentralandTime()

      return { seconds: time }
    },
    async getRealm(): Promise<GetRealmResponse> {
      const realmAdapter = getRealmAdapter(store.getState())

      if (!realmAdapter) {
        return {}
      }
      const baseUrl = urlWithProtocol(new URL(realmAdapter.baseUrl).hostname)

      return {
        realmInfo: {
          baseUrl,
          realmName: realmAdapter.about.configurations?.realmName ?? '',
          networkId: realmAdapter.about.configurations?.networkId ?? 1,
          commsAdapter: realmAdapter.about.comms?.fixedAdapter ?? '',
          isPreview: PREVIEW
        }
      }
    }
  }))
}
