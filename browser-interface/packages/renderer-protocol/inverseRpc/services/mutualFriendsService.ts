import {
  GetMutualFriendsResponse,
  MutualFriendsKernelServiceDefinition
} from '@dcl/protocol/out-ts/decentraland/renderer/kernel_services/mutual_friends_kernel.gen'
import * as codegen from '@dcl/rpc/dist/codegen'
import { RpcServerPort } from '@dcl/rpc/dist/types'
import { getMutualFriends } from 'shared/friends/sagas'
import { RendererProtocolContext } from '../context'

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
