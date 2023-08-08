import type { RpcServerPort } from '@dcl/rpc'
import * as codegen from '@dcl/rpc/dist/codegen'
import type { PortContext } from './context'
import { SocialApiServiceDefinition, FriendsRequest, FriendsResponse } from 'shared/protocol/decentraland/kernel/apis/social_api.gen'
import { isWorldLoaderActive } from 'shared/realm/selectors'
import { store } from 'shared/store/isolatedStore'
import { getLivekitActiveVideoStreams } from 'shared/comms/selectors'
import { ensureRealmAdapter } from 'shared/realm/ensureRealmAdapter'
import { ActiveVideoStreams } from 'shared/comms/adapters/types'

export function registerSocialApiServiceServerImplementation(port: RpcServerPort<PortContext>) {
  codegen.registerService(port, SocialApiServiceDefinition, async () => ({
    async getFriends(req: FriendsRequest, ctx: PortContext) {

      return {} as FriendsResponse
    }
  })
  )
}
