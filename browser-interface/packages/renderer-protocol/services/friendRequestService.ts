import { RpcClientPort } from '@dcl/rpc'
import * as codegen from '@dcl/rpc/dist/codegen'
import { FriendRequestRendererServiceDefinition } from 'shared/protocol/decentraland/renderer/renderer_services/friend_request_renderer.gen'
import defaultLogger from 'lib/logger'

// eslint-disable-next-line @typescript-eslint/ban-types
export function registerFriendRequestRendererService<Context extends {}>(
  clientPort: RpcClientPort
): codegen.RpcClientModule<FriendRequestRendererServiceDefinition, Context> | undefined {
  try {
    return codegen.loadService<Context, FriendRequestRendererServiceDefinition>(
      clientPort,
      FriendRequestRendererServiceDefinition
    )
  } catch (err) {
    defaultLogger.error('FriendRequestRendererService could not be loaded', err)
    return undefined
  }
}
