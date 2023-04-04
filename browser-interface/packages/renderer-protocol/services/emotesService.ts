import { RpcClientPort } from '@dcl/rpc'
import * as codegen from '@dcl/rpc/dist/codegen'
import { EmotesRendererServiceDefinition } from 'shared/protocol/decentraland/renderer/renderer_services/emotes_renderer.gen'
import defaultLogger from 'lib/logger'

// eslint-disable-next-line @typescript-eslint/ban-types
export function registerEmotesService<Context extends {}>(
  clientPort: RpcClientPort
): codegen.RpcClientModule<EmotesRendererServiceDefinition, Context> | undefined {
  try {
    return codegen.loadService<Context, EmotesRendererServiceDefinition>(clientPort, EmotesRendererServiceDefinition)
  } catch (e) {
    defaultLogger.error('EmotesService could not be loaded')
    return undefined
  }
}
