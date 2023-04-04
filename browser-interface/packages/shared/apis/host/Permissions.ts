import * as codegen from '@dcl/rpc/dist/codegen'
import type { RpcServerPort } from '@dcl/rpc/dist/types'
import type { Scene } from '@dcl/schemas'
import {
  PermissionsServiceDefinition,
  permissionItemFromJSON,
  permissionItemToJSON
} from 'shared/protocol/decentraland/kernel/apis/permissions.gen'
import { PermissionItem } from 'shared/protocol/decentraland/kernel/apis/permissions.gen'
import type { PortContext } from './context'

export const defaultParcelPermissions: PermissionItem[] = [
  PermissionItem.PI_USE_WEB3_API,
  PermissionItem.PI_USE_FETCH,
  PermissionItem.PI_USE_WEBSOCKET
]
export const defaultPortableExperiencePermissions: PermissionItem[] = []

export function assertHasPermission(test: PermissionItem, ctx: PortContext) {
  if (!hasPermission(test, ctx)) {
    throw new Error(`This scene doesn't have some of the next permissions: ${permissionItemToJSON(test)}.`)
  }

  return true
}

export function hasPermission(test: PermissionItem, ctx: PortContext) {
  // Backward compatibility with parcel scene with 'requiredPermissions' in the scene.json
  //  Only the two permissions that start with ALLOW_TO_... can be conceed without user
  //  interaction

  const isOneOfFirstPermissions =
    test === PermissionItem.PI_ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE ||
    test === PermissionItem.PI_ALLOW_TO_TRIGGER_AVATAR_EMOTE

  if (ctx.sceneData.entity?.metadata) {
    const sceneJsonData: Scene = ctx.sceneData.entity.metadata
    const list: PermissionItem[] = []

    if (sceneJsonData && sceneJsonData.requiredPermissions) {
      for (const permissionItemString of sceneJsonData.requiredPermissions) {
        const permissionItem = permissionItemFromJSON(`PI_${permissionItemString}`)
        if (permissionItem !== PermissionItem.UNRECOGNIZED) {
          list.push(permissionItem)
        }
      }
    }

    if (list.includes(test) && isOneOfFirstPermissions) {
      return true
    }

    // Workaround to give old default permissions, remove when
    //  a method for grant permissions exist.
    if (ctx.sceneData.isPortableExperience) {
      if (isOneOfFirstPermissions) {
        return true
      }
    }
  }

  return ctx.permissionGranted.has(test)
}

export function registerPermissionServiceServerImplementation(port: RpcServerPort<PortContext>) {
  codegen.registerService(port, PermissionsServiceDefinition, async () => ({
    async hasPermission(req, ctx) {
      return { hasPermission: hasPermission(req.permission, ctx) }
    },
    async hasManyPermissions(req, ctx) {
      const hasManyPermission = await Promise.all(req.permissions.map((item) => hasPermission(item, ctx)))
      return { hasManyPermission }
    }
  }))
}
