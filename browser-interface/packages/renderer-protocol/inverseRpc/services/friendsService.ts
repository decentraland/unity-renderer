import { RpcServerPort } from '@dcl/rpc'
import { RendererProtocolContext } from '../context'
import * as codegen from '@dcl/rpc/dist/codegen'
import {
  FriendsKernelServiceDefinition,
  GetFriendshipStatusResponse
} from 'shared/protocol/decentraland/renderer/kernel_services/friends_kernel.gen'
import { getFriendshipStatus } from 'shared/friends/sagas'

export function registerFriendsKernelService(port: RpcServerPort<RendererProtocolContext>) {
  codegen.registerService(port, FriendsKernelServiceDefinition, async () => ({
    async getFriendshipStatus(req, _) {
      const status = getFriendshipStatus(req)

      const response: GetFriendshipStatusResponse = {
        status
      }

      return response
    }
  }))
}
