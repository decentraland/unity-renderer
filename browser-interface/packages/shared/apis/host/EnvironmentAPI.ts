import { getSelectedNetwork } from 'shared/dao/selectors'
import { getServerConfigurations, PREVIEW, RENDERER_WS } from 'config'
import { store } from 'shared/store/isolatedStore'
import { getCommsIsland } from 'shared/comms/selectors'
import { getRealmAdapter } from 'shared/realm/selectors'
import { getFeatureFlagEnabled } from 'shared/meta/selectors'
import * as codegen from '@dcl/rpc/dist/codegen'
import type { RpcServerPort } from '@dcl/rpc/dist/types'
import { EnvironmentApiServiceDefinition } from 'shared/protocol/decentraland/kernel/apis/environment_api.gen'
import type {
  AreUnsafeRequestAllowedResponse,
  BootstrapDataResponse,
  GetCurrentRealmResponse,
  GetDecentralandTimeResponse,
  GetExplorerConfigurationResponse,
  GetPlatformResponse,
  PreviewModeResponse
} from 'shared/protocol/decentraland/kernel/apis/environment_api.gen'
import type { EnvironmentRealm } from '../IEnvironmentAPI'
import { Platform } from '../IEnvironmentAPI'
import type { PortContextService } from './context'
import type { IRealmAdapter } from 'shared/realm/types'
import { realmToConnectionString, urlWithProtocol } from 'shared/realm/resolver'

export function registerEnvironmentApiServiceServerImplementation(
  port: RpcServerPort<PortContextService<'sceneData'>>
) {
  codegen.registerService(port, EnvironmentApiServiceDefinition, async () => ({
    async getBootstrapData(_req, ctx): Promise<BootstrapDataResponse> {
      return {
        id: ctx.sceneData.id,
        baseUrl: ctx.sceneData.baseUrl,
        useFPSThrottling: ctx.sceneData.useFPSThrottling ?? false,
        entity: {
          content: ctx.sceneData.entity.content,
          metadataJson: JSON.stringify(ctx.sceneData.entity.metadata)
        }
      }
    },
    async isPreviewMode(): Promise<PreviewModeResponse> {
      return { isPreview: PREVIEW }
    },
    async getPlatform(): Promise<GetPlatformResponse> {
      if (RENDERER_WS) {
        return { platform: Platform.DESKTOP }
      } else {
        return { platform: Platform.BROWSER }
      }
    },
    async areUnsafeRequestAllowed(): Promise<AreUnsafeRequestAllowedResponse> {
      return { status: getFeatureFlagEnabled(store.getState(), 'unsafe-request') }
    },
    // @deprecated use GetRealm from Runtime instead
    async getCurrentRealm(): Promise<GetCurrentRealmResponse> {
      const realmAdapter = getRealmAdapter(store.getState())
      const island = getCommsIsland(store.getState()) ?? '' // We shouldn't send undefined because it would break contract

      if (!realmAdapter) {
        return {}
      }

      return { currentRealm: toEnvironmentRealmType(realmAdapter, island) }
    },
    async getExplorerConfiguration(): Promise<GetExplorerConfigurationResponse> {
      return {
        clientUri: location.href,
        configurations: {
          questsServerUrl: getServerConfigurations(getSelectedNetwork(store.getState())).questsUrl
        }
      }
    },
    // @deprecated use GetTime from Runtime instead
    async getDecentralandTime(): Promise<GetDecentralandTimeResponse> {
      const time = getDecentralandTime()
      return { seconds: time }
    }
  }))
}

export function getDecentralandTime() {
  let time = decentralandTimeData.time

  // if time is not paused we calculate the current time to avoid
  // constantly receiving messages from the renderer
  if (!decentralandTimeData.isPaused) {
    const offsetMsecs = Date.now() - decentralandTimeData.receivedAt
    const offsetSecs = offsetMsecs / 1000
    const offsetInDecentralandUnits = offsetSecs / decentralandTimeData.timeNormalizationFactor
    time += offsetInDecentralandUnits

    if (time >= decentralandTimeData.cycleTime) {
      time = 0.01
    }
  }

  //convert time to seconds
  time = time * 3600

  return time
}

export function toEnvironmentRealmType(realm: IRealmAdapter, island: string | undefined): EnvironmentRealm {
  const serverName = realmToConnectionString(realm)
  const hostname = new URL(realm.baseUrl).hostname
  return {
    protocol: realm.about.comms?.protocol || 'v3',
    // domain explicitly expects the URL with the protocol
    domain: urlWithProtocol(hostname),
    layer: island ?? '',
    room: island ?? '',
    serverName,
    displayName: serverName
  }
}

type DecentralandTimeData = {
  timeNormalizationFactor: number
  cycleTime: number
  isPaused: number
  time: number
  receivedAt: number
}

let decentralandTimeData: DecentralandTimeData

export function setDecentralandTime(data: DecentralandTimeData) {
  decentralandTimeData = data
  decentralandTimeData.receivedAt = Date.now()
}
