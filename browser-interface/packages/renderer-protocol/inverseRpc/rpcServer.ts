import { createRpcServer, RpcServerPort, Transport } from '@dcl/rpc'
import { RendererProtocolContext } from './context'
import { registerEmotesKernelService } from './services/emotesService'
import { registerAnalyticsKernelService } from './services/analyticsService'
import { registerFriendRequestKernelService } from './services/friendRequestService'
import { registerFriendsKernelService } from './services/friendsService'
import { registerMutualFriendsKernelService } from './services/mutualFriendsService'
import { registerSignRequestService } from './services/signRequestService'

export function createRendererProtocolInverseRpcServer(transport: Transport) {
  const server = createRpcServer<RendererProtocolContext>({})

  const context: RendererProtocolContext = {
    times: 0
  }

  server.setHandler(registerKernelServices)
  server.attachTransport(transport, context)
}

/*
 * This function is called when the TransportService works.
 * And it should register all the kernel services (Renderer->Kernel)
 */
async function registerKernelServices(serverPort: RpcServerPort<RendererProtocolContext>) {
  registerEmotesKernelService(serverPort)
  registerAnalyticsKernelService(serverPort)
  registerFriendRequestKernelService(serverPort)
  registerFriendsKernelService(serverPort)
  registerMutualFriendsKernelService(serverPort)
  registerSignRequestService(serverPort)
}
