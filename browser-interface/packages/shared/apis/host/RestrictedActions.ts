import { Quaternion, Vector3 } from '@dcl/ecs-math'
import type { RpcServerPort } from '@dcl/rpc'
import * as codegen from '@dcl/rpc/dist/codegen'
import { gridToWorld } from 'lib/decentraland/parcels/gridToWorld'
import { parseParcelPosition } from 'lib/decentraland/parcels/parseParcelPosition'
import { isWorldPositionInsideParcels } from 'lib/decentraland/parcels/isWorldPositionInsideParcels'
import { lastPlayerPosition } from 'shared/world/positionThings'
import { browserInterface } from 'unity-interface/BrowserInterface'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import type { PortContext } from './context'

import { PermissionItem } from 'shared/protocol/decentraland/kernel/apis/permissions.gen'
import type {
  ChangeRealmRequest,
  CommsAdapterRequest,
  MovePlayerToRequest,
  MovePlayerToResponse,
  OpenExternalUrlRequest,
  OpenNftDialogRequest,
  SuccessResponse,
  TeleportToRequest,
  TriggerEmoteRequest,
  TriggerEmoteResponse,
  TriggerSceneEmoteRequest
} from 'shared/protocol/decentraland/kernel/apis/restricted_actions.gen'
import { RestrictedActionsServiceDefinition } from 'shared/protocol/decentraland/kernel/apis/restricted_actions.gen'
import { changeRealm } from 'shared/dao'
import defaultLogger from 'lib/logger'
import { getRendererModules } from 'shared/renderer/selectors'
import { store } from 'shared/store/isolatedStore'
import { assertHasPermission } from './Permissions'

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

export async function triggerSceneEmote(req: TriggerSceneEmoteRequest, ctx: PortContext): Promise<SuccessResponse> {
  // checks permissions
  assertHasPermission(PermissionItem.PI_ALLOW_TO_TRIGGER_AVATAR_EMOTE, ctx)

  if (!isPositionValid(lastPlayerPosition, ctx)) {
    ctx.logger.error('Error: Player is not inside of scene', lastPlayerPosition)
    return { success: false }
  }

  const emoteService = getRendererModules(store.getState())?.emotes

  if (!emoteService) {
    return { success: false }
  }

  const request = {
    path: req.src,
    sceneNumber: ctx.sceneData.sceneNumber,
    loop: req.loop ?? false
  }

  const response = await emoteService.triggerSceneExpression({ ...request })

  return response
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
      if (ctx.sceneData.isPortableExperience){
        assertHasPermission(PermissionItem.PI_OPEN_EXTERNAL_LINK, ctx)
      }
      if (!isPositionValid(lastPlayerPosition, ctx)) {
        ctx.logger.error('Error: Player is not inside of scene', lastPlayerPosition)
        return { success: false }
      }
      const response = await getRendererModules(store.getState())?.restrictedActions?.openExternalUrl({
        url: req.url,
        sceneNumber: ctx.sceneData.sceneNumber
      })
      return { success: response?.success ?? false }
    },
    async openNftDialog(req: OpenNftDialogRequest, ctx: PortContext) {
      if (!ctx.sdk7) throw new Error('API only available for SDK7')
      if (!isPositionValid(lastPlayerPosition, ctx)) {
        ctx.logger.error('Error: Player is not inside of scene', lastPlayerPosition)
        return { success: false }
      }

      const response = await getRendererModules(store.getState())?.restrictedActions?.openNftDialog({ urn: req.urn, sceneNumber: ctx.sceneData.sceneNumber })
      return { success: response?.success ?? false }
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

      if (!isPositionValid(lastPlayerPosition, ctx) || !req.worldCoordinates)
        ctx.logger.error('Error: Player is not inside of scene', lastPlayerPosition)
      else
        getRendererModules(store.getState())?.restrictedActions?.teleportTo({ worldCoordinates: req.worldCoordinates, sceneNumber: ctx.sceneData.sceneNumber })

      return {}
    },
    async triggerSceneEmote(req: TriggerSceneEmoteRequest, ctx: PortContext) {
      const response = await triggerSceneEmote(req, ctx)
      return response
    }
  }))
}
