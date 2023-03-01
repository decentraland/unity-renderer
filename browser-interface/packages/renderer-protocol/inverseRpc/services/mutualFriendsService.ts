import {
  GetMutualFriendsResponse,
  MutualFriendsKernelServiceDefinition
} from '@dcl/protocol/out-ts/decentraland/renderer/kernel_services/mutual_friends_kernel.gen'
import { RpcServerPort } from '@dcl/rpc'
import * as codegen from '@dcl/rpc/dist/codegen'
import { RendererProtocolContext } from '../context'
import { getMutualFriends } from 'shared/friends/sagas'

export function registerMutualFriendsKernelService(port: RpcServerPort<RendererProtocolContext>) {
  codegen.registerService(port, MutualFriendsKernelServiceDefinition, async () => ({
    async getMutualFriends(req, _) {
      const mutualFriends = await getMutualFriends(req)

      const response: GetMutualFriendsResponse = {
        friends: mutualFriends ? mutualFriends : []
      }

      return response
    }
  }))
}
