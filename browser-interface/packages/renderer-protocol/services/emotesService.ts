import { RpcClientPort } from '@dcl/rpc'
import * as codegen from '@dcl/rpc/dist/codegen'
import { EmotesRendererServiceDefinition } from '@dcl/protocol/out-ts/decentraland/renderer/renderer_services/emotes_renderer.gen'
import defaultLogger from 'shared/logger'

export function registerEmotesService<Context>(
  clientPort: RpcClientPort
): codegen.RpcClientModule<EmotesRendererServiceDefinition, Context> | undefined {
  try {
    return codegen.loadService<Context, EmotesRendererServiceDefinition>(clientPort, EmotesRendererServiceDefinition)
  } catch (e) {
    defaultLogger.error('EmotesService could not be loaded')
    return undefined
  }
}
