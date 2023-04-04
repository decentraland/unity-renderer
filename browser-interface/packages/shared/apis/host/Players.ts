import { store } from 'shared/store/isolatedStore'

import { getProfileFromStore } from 'shared/profiles/selectors'
import { calculateDisplayName } from 'lib/decentraland/profiles/transformations/processServerProfile'

import { getCurrentUserId } from 'shared/session/selectors'
import { getInSceneAvatarsUserId } from 'shared/social/avatarTracker'
import { lastPlayerPosition } from 'shared/world/positionThings'

import type { RpcServerPort } from '@dcl/rpc'
import * as codegen from '@dcl/rpc/dist/codegen'
import type { PortContext } from './context'

import { PlayersServiceDefinition } from 'shared/protocol/decentraland/kernel/apis/players.gen'
import { isWorldPositionInsideParcels } from 'lib/decentraland/parcels/isWorldPositionInsideParcels'
import { getVisiblePeerEthereumAddresses } from 'shared/comms/peers'
import { sceneRuntimeCompatibleAvatar } from 'lib/decentraland/profiles'

export function registerPlayersServiceServerImplementation(port: RpcServerPort<PortContext>) {
  codegen.registerService(port, PlayersServiceDefinition, async () => ({
    async getPlayerData(req) {
      const userId = req.userId
      const profile = getProfileFromStore(store.getState(), userId)

      if (!profile?.data) {
        return {}
      }

      return {
        data: {
          displayName: calculateDisplayName(profile.data),
          publicKey: profile.data.hasConnectedWeb3 ? profile.data.userId : '',
          hasConnectedWeb3: !!profile.data.hasConnectedWeb3,
          userId: userId,
          version: profile.data.version,
          avatar: sceneRuntimeCompatibleAvatar(profile.data.avatar)
        }
      }
    },
    async getPlayersInScene(req, ctx) {
      const currentUserId = getCurrentUserId(store.getState())
      const sceneParcels = ctx.sceneData.entity.metadata.scene?.parcels

      let isCurrentUserIncluded = false

      const result: { userId: string }[] = []
      for (const userId of getInSceneAvatarsUserId(ctx.sceneData.id)) {
        if (userId === currentUserId) {
          isCurrentUserIncluded = true
        }
        result.push({ userId })
      }

      // check also for current user, since it won't appear in `getInSceneAvatarsUserId` result
      if (!isCurrentUserIncluded && isWorldPositionInsideParcels(sceneParcels, lastPlayerPosition)) {
        if (currentUserId) {
          result.push({ userId: currentUserId })
        }
      }

      return { players: result }
    },
    async getConnectedPlayers() {
      const userId = getCurrentUserId(store.getState())!
      return {
        players: getVisiblePeerEthereumAddresses().concat({ userId })
      }
    }
  }))
}
