import * as codegen from '@dcl/rpc/dist/codegen'
import { RpcServerPort } from '@dcl/rpc/dist/types'
import {
  GetSceneRequest,
  GetSceneResponse,
  SceneServiceDefinition
} from '@dcl/protocol/out-ts/decentraland/kernel/apis/scene.gen'
import { PortContext, PortContextService } from './context'

export function registerRuntimeServiceServerImplementation(port: RpcServerPort<PortContextService<'sceneData'>>) {
  codegen.registerService(port, SceneServiceDefinition, async () => ({
    async getSceneInfo(_req: GetSceneRequest, ctx: PortContext): Promise<GetSceneResponse> {
      const sceneData = ctx.sceneData

      if (!sceneData) {
        throw new Error('There is no scene info')
      }

      return {
        cid: ctx.sceneData.id || '',
        sceneId: sceneData.id || '',
        sceneJson: sceneData.entity.metadata ? JSON.stringify(sceneData.entity.metadata) : '{}'
      }
    }
  }))
}
