import { calculateDisplayName } from 'lib/decentraland/profiles/transformations/processServerProfile'

import { sceneRuntimeCompatibleAvatar } from 'lib/decentraland/profiles/sceneRuntime'
import { retrieveProfile } from 'shared/profiles/retrieveProfile'
import { onLoginCompleted } from 'shared/session/onLoginCompleted'

import { UserIdentityServiceDefinition } from 'shared/protocol/decentraland/kernel/apis/user_identity.gen'
import type { RpcServerPort } from '@dcl/rpc'
import * as codegen from '@dcl/rpc/dist/codegen'
import type { PortContext } from './context'

export function registerUserIdentityServiceServerImplementation(port: RpcServerPort<PortContext>) {
  codegen.registerService(port, UserIdentityServiceDefinition, async () => ({
    async getUserPublicKey() {
      const { identity } = await onLoginCompleted()
      if (!identity || !identity.address) {
        debugger
      }
      if (identity && identity.hasConnectedWeb3) {
        return { address: identity.address }
      } else {
        return {}
      }
    },
    async getUserData() {
      const { identity } = await onLoginCompleted()

      if (!identity || !identity.address) {
        debugger
        return {}
      }

      const profile = await retrieveProfile(identity.address)

      return {
        data: {
          displayName: calculateDisplayName(profile),
          publicKey: identity.hasConnectedWeb3 ? identity.address : undefined,
          hasConnectedWeb3: !!identity.hasConnectedWeb3,
          userId: identity.address,
          version: profile.version,
          avatar: sceneRuntimeCompatibleAvatar(profile.avatar)
        }
      }
    }
  }))
}
