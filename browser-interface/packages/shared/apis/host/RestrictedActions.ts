import { Vector3, Quaternion } from '@dcl/ecs-math'
import {
  gridToWorld,
  isWorldPositionInsideParcels,
  parseParcelPosition
} from '../../../atomicHelpers/parcelScenePositions'
import { lastPlayerPosition } from '../../world/positionThings'
import { browserInterface } from '../../../unity-interface/BrowserInterface'
import { getUnityInstance } from '../../../unity-interface/IUnityInterface'
import { RpcServerPort } from '@dcl/rpc'
import { PortContext } from './context'
import * as codegen from '@dcl/rpc/dist/codegen'

import {
  ChangeRealmRequest,
  CommsAdapterRequest,
  MovePlayerToRequest,
  MovePlayerToResponse,
  OpenExternalUrlRequest,
  OpenNftDialogRequest,
  RestrictedActionsServiceDefinition,
  TeleportToRequest,
  TriggerEmoteRequest,
  TriggerEmoteResponse
} from '@dcl/protocol/out-ts/decentraland/kernel/apis/restricted_actions.gen'
import { assertHasPermission } from './Permissions'
import { PermissionItem } from '@dcl/protocol/out-ts/decentraland/kernel/apis/permissions.gen'
import { getRendererModules } from 'shared/renderer/selectors'
import { store } from 'shared/store/isolatedStore'
import defaultLogger from 'shared/logger'
import { changeRealm } from 'shared/dao'

export function movePlayerTo(req: MovePlayerToRequest, ctx: PortContext): MovePlayerToResponse {
  //   checks permissions
  assertHasPermission(PermissionItem.PI_ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE, ctx)

  if (!ctx.sceneData) return {}

  const base = parseParcelPosition(ctx.sceneData.entity.metadata.scene?.base || '0,0')
  const basePosition = new Vector3()
  gridToWorld(base.x, base.y, basePosition)

  // newRelativePosition is the position relative to the scene in meters
  // newAbsolutePosition is the absolute position in the world in meters
  const newAbsolutePosition = basePosition.add(req.newRelativePosition!)

  // validate new position is inside one of the scene's parcels
  if (!isPositionValid(newAbsolutePosition, ctx)) {
    ctx.logger.error('Error: Position is out of scene', newAbsolutePosition)
    return {}
  }
  if (!isPositionValid(lastPlayerPosition, ctx)) {
    ctx.logger.error('Error: Player is not inside of scene', lastPlayerPosition)
    return {}
  }

  getUnityInstance().Teleport(
    {
      position: newAbsolutePosition,
      cameraTarget: req.cameraTarget ? basePosition.add(req.cameraTarget) : undefined
    },
    false
  )

  // Get ahead of the position report that will be done automatically later and report
  // position right now, also marked as an immediate update (last bool in Position structure)
  browserInterface.ReportPosition({
    position: newAbsolutePosition,
    rotation: Quaternion.Identity,
    immediate: true
  })
  return {}
}

export function triggerEmote(req: TriggerEmoteRequest, ctx: PortContext): TriggerEmoteResponse {
  // checks permissions
  assertHasPermission(PermissionItem.PI_ALLOW_TO_TRIGGER_AVATAR_EMOTE, ctx)

  if (!isPositionValid(lastPlayerPosition, ctx)) {
    ctx.logger.error('Error: Player is not inside of scene', lastPlayerPosition)
    return {}
  }

  getUnityInstance().TriggerSelfUserExpression(req.predefinedEmote)
  getRendererModules(store.getState())
    ?.emotes?.triggerSelfUserExpression({ id: req.predefinedEmote })
    .catch(defaultLogger.error)

  return {}
}

function isPositionValid(position: Vector3, ctx: PortContext) {
  return (
    ctx.sceneData!.isPortableExperience ||
    isWorldPositionInsideParcels(ctx.sceneData.entity.metadata.scene?.parcels || [], position)
  )
}

export function registerRestrictedActionsServiceServerImplementation(port: RpcServerPort<PortContext>) {
  codegen.registerService(port, RestrictedActionsServiceDefinition, async () => ({
    async triggerEmote(req: TriggerEmoteRequest, ctx: PortContext) {
      return triggerEmote(req, ctx)
    },
    async movePlayerTo(req: MovePlayerToRequest, ctx: PortContext) {
      return movePlayerTo(req, ctx)
    },
    async changeRealm(req: ChangeRealmRequest, ctx: PortContext) {
      if (!ctx.sdk7) throw new Error('API only available for SDK7')
      if (!isPositionValid(lastPlayerPosition, ctx)) {
        ctx.logger.error('Error: Player is not inside of scene', lastPlayerPosition)
        return { success: false }
      }

      // TODO: add visual prompt for the user
      const userApprovedChangingRealm = true // await unity.promptChangeRealm(sceneId, req.message, req.realm)

      if (userApprovedChangingRealm) {
        try {
          await changeRealm(req.realm)
          return { success: true }
        } catch (err: any) {
          ctx.logger.error(err)
        }
      }

      return { success: false }
    },
    async openExternalUrl(req: OpenExternalUrlRequest, ctx: PortContext) {
      if (!ctx.sdk7) throw new Error('API only available for SDK7')
      if (!isPositionValid(lastPlayerPosition, ctx)) {
        ctx.logger.error('Error: Player is not inside of scene', lastPlayerPosition)
        return { success: false }
      }
      // TODO: implement this fn in renderer-protocol RPC
      const success = false // await unity.openExternalUrl(sceneId, req.message, req.realm)
      return { success }
    },
    async openNftDialog(req: OpenNftDialogRequest, ctx: PortContext) {
      if (!ctx.sdk7) throw new Error('API only available for SDK7')
      if (!isPositionValid(lastPlayerPosition, ctx)) {
        ctx.logger.error('Error: Player is not inside of scene', lastPlayerPosition)
        return { success: false }
      }

      // TODO: implement this fn in renderer-protocol RPC
      const success = false // await unity.openExternalUrl(sceneId, req.urn)
      return { success }
    },
    async setCommunicationsAdapter(req: CommsAdapterRequest, ctx: PortContext) {
      if (!ctx.sdk7) throw new Error('API only available for SDK7')
      if (!isPositionValid(lastPlayerPosition, ctx)) {
        ctx.logger.error('Error: Player is not inside of scene', lastPlayerPosition)
        return { success: false }
      }

      /**
       * 1 First we must store the current realm as OLD_REALM
       * 2 Then disconnect the BFF if we are connected.
       * 3 Then dispatch a set adapter
       * 4 Then wait for successful connection
       *   4.ok   If OK, then return {success:true}
       *   4.else Otherwise reconnect OLD_REALM, return {success:false}
       */

      return { success: false }
    },
    async teleportTo(req: TeleportToRequest, ctx: PortContext) {
      if (!ctx.sdk7) throw new Error('API only available for SDK7')
      if (!isPositionValid(lastPlayerPosition, ctx) || !req.worldPosition) {
        ctx.logger.error('Error: Player is not inside of scene', lastPlayerPosition)
        return { success: false }
      }

      getUnityInstance().Teleport(
        {
          position: req.worldPosition,
          cameraTarget: req.cameraTarget
        },
        false
      )

      // Get ahead of the position report that will be done automatically later and report
      // position right now, also marked as an immediate update (last bool in Position structure)
      browserInterface.ReportPosition({
        position: req.worldPosition,
        rotation: Quaternion.Identity,
        immediate: true
      })

      return { success: true }
    }
  }))
}
