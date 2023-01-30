import { store } from 'shared/store/isolatedStore'

import { getProfileFromStore } from 'shared/profiles/selectors'
import { calculateDisplayName } from 'shared/profiles/transformations/processServerProfile'

import { getInSceneAvatarsUserId } from 'shared/social/avatarTracker'
import { lastPlayerPosition } from 'shared/world/positionThings'
import { getCurrentUserId } from 'shared/session/selectors'
import { isWorldPositionInsideParcels } from 'atomicHelpers/parcelScenePositions'
import { AvatarInfo, Snapshots, WearableId } from '@dcl/schemas'
import { rgbToHex } from 'shared/profiles/transformations/convertToRGBObject'

export type AvatarForUserData = {
  bodyShape: WearableId
  skinColor: string
  hairColor: string
  eyeColor: string
  wearables: WearableId[]
  emotes?: {
    slot: number
    urn: string
  }[]
  snapshots: Snapshots
}

export function sdkCompatibilityAvatar(avatar: AvatarInfo): AvatarForUserData {
  return {
    ...avatar,
    bodyShape: avatar?.bodyShape,
    wearables: avatar?.wearables || [],
    snapshots: {
      ...avatar.snapshots,
      face: avatar.snapshots.face256,
      face128: avatar.snapshots.face256
    } as any,
    eyeColor: rgbToHex(avatar.eyes.color),
    hairColor: rgbToHex(avatar.hair.color),
    skinColor: rgbToHex(avatar.skin.color)
  }
}

import { RpcServerPort } from '@dcl/rpc'
import { PortContext } from './context'
import * as codegen from '@dcl/rpc/dist/codegen'

import { PlayersServiceDefinition } from '@dcl/protocol/out-ts/decentraland/kernel/apis/players.gen'
import { getAllPeers } from 'shared/comms/peers'

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
          avatar: sdkCompatibilityAvatar(profile.data.avatar)
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
      return {
        players: Array.from(getAllPeers().values())
          .filter(($) => $.visible)
          .map((peer) => {
            return { userId: peer.ethereumAddress }
          })
      }
    }
  }))
}
