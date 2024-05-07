import { store } from 'shared/store/isolatedStore'
import { getRealmAdapter } from 'shared/realm/selectors'
import * as codegen from '@dcl/rpc/dist/codegen'
import type { RpcServerPort } from '@dcl/rpc/dist/types'
import type { GetRealmResponse, GetWorldTimeResponse } from 'shared/protocol/decentraland/kernel/apis/runtime.gen'
import { RuntimeServiceDefinition } from 'shared/protocol/decentraland/kernel/apis/runtime.gen'
import type { PortContextService } from './context'
import { getDecentralandTime } from './EnvironmentAPI'
import { urlWithProtocol } from 'shared/realm/resolver'
import { PREVIEW, RENDERER_WS, getServerConfigurations } from 'config'
import { Platform } from '../IEnvironmentAPI'
import { getSelectedNetwork } from '../../dao/selectors'

export function registerRuntimeServiceServerImplementation(port: RpcServerPort<PortContextService<'sceneData'>>) {
  codegen.registerService(port, RuntimeServiceDefinition, async () => ({
    async getExplorerInformation() {
      const questsServerUrl = getServerConfigurations(getSelectedNetwork(store.getState())).questsUrl
      const platform = RENDERER_WS ? Platform.DESKTOP : Platform.BROWSER

      return {
        agent: 'explorer-kernel',
        platform,
        configurations: { questsServerUrl }
      }
    },
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
          commsAdapter: realmAdapter.about.comms?.adapter ?? '',
          isPreview: PREVIEW
        }
      }
    },
    async getSceneInformation(_, ctx) {
      return {
        urn: ctx.sceneData.id,
        baseUrl: ctx.sceneData.baseUrl,
        content: ctx.sceneData.entity.content,
        metadataJson: JSON.stringify(ctx.sceneData.entity.metadata)
      }
    },
    async readFile(req, ctx) {
      return ctx.readFile(req.fileName)
    }
  }))
}
