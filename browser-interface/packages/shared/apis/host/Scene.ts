import * as codegen from '@dcl/rpc/dist/codegen'
import type { RpcServerPort } from '@dcl/rpc/dist/types'
import type { GetSceneRequest, GetSceneResponse } from 'shared/protocol/decentraland/kernel/apis/scene.gen'
import { SceneServiceDefinition } from 'shared/protocol/decentraland/kernel/apis/scene.gen'
import type { PortContext, PortContextService } from './context'

export function registerSceneServiceServerImplementation(port: RpcServerPort<PortContextService<'sceneData'>>) {
  codegen.registerService(port, SceneServiceDefinition, async () => ({
    async getSceneInfo(_req: GetSceneRequest, ctx: PortContext): Promise<GetSceneResponse> {
      const sceneData = ctx.sceneData

      if (!sceneData) {
        throw new Error('There is no scene info')
      }

      return {
        cid: ctx.sceneData.id || '',
        metadata: sceneData.entity.metadata ? JSON.stringify(sceneData.entity.metadata) : '{}',
        baseUrl: sceneData.baseUrl || '',
        contents: sceneData.entity.content || []
      }
    }
  }))
}
