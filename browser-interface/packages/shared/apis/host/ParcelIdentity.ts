import * as codegen from '@dcl/rpc/dist/codegen'
import type { RpcServerPort } from '@dcl/rpc/dist/types'
import { ETHEREUM_NETWORK, getAssetBundlesBaseUrl } from 'config'
import type {
  GetParcelRequest,
  GetParcelResponse,
  GetSceneIdRequest,
  GetIsEmptyResponse,
  GetIsEmptyRequest,
  GetSceneIdResponse
} from 'shared/protocol/decentraland/kernel/apis/parcel_identity.gen'
import { ParcelIdentityServiceDefinition } from 'shared/protocol/decentraland/kernel/apis/parcel_identity.gen'
import type { PortContext } from './context'

async function getParcel(_req: GetParcelRequest, ctx: PortContext): Promise<GetParcelResponse> {
  const sceneData = ctx.sceneData

  if (!sceneData) {
    throw new Error('No land assigned in the ParcelIdentity context.')
  }

  return {
    land: {
      sceneId: sceneData.id || '',
      sceneJsonData: sceneData.entity.metadata ? JSON.stringify(sceneData.entity.metadata) : '{}',
      baseUrl: sceneData.baseUrl || '',
      baseUrlBundles: getAssetBundlesBaseUrl(ETHEREUM_NETWORK.MAINNET) + '/',
      mappingsResponse: {
        parcelId: sceneData.id || '',
        rootCid: sceneData.id || '',
        contents: sceneData.entity.content || []
      }
    },
    cid: ctx.sceneData.id || ''
  }
}

async function getSceneId(_req: GetSceneIdRequest, ctx: PortContext): Promise<GetSceneIdResponse> {
  const sceneId = ctx.sceneData.id || ''
  return { sceneId }
}

async function getIsEmpty(_req: GetIsEmptyRequest, _ctx: PortContext): Promise<GetIsEmptyResponse> {
  return { isEmpty: false }
}

export function registerParcelIdentityServiceServerImplementation(port: RpcServerPort<PortContext>) {
  codegen.registerService(port, ParcelIdentityServiceDefinition, async () => ({
    getParcel,
    getSceneId,
    getIsEmpty
  }))
}
