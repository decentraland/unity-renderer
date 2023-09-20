import type { RpcServerModule } from '@dcl/rpc/dist/codegen'
import * as codegen from '@dcl/rpc/dist/codegen'
import type { RpcServerPort } from '@dcl/rpc/dist/types'
import type { EventData, ManyEntityAction } from 'shared/protocol/decentraland/kernel/apis/engine_api.gen'
import { EngineApiServiceDefinition } from 'shared/protocol/decentraland/kernel/apis/engine_api.gen'
import type { PortContext } from './context'

import { avatarSdk7MessageObservable } from './sdk7/avatar'
import { DeleteComponent } from './sdk7/serialization/crdt/deleteComponent'
import { ReadWriteByteBuffer } from './sdk7/serialization/ByteBuffer'
import { Sdk7ComponentIds } from './sdk7/avatar/ecs'

function getParcelNumber(x: number, z: number) {
  return z * 100e8 + x
}

export function registerEngineApiServiceServerImplementation(port: RpcServerPort<PortContext>) {
  codegen.registerService(
    port,
    EngineApiServiceDefinition,
    async (port, ctx): Promise<RpcServerModule<EngineApiServiceDefinition, PortContext>> => {
      let sdk7AvatarUpdates: Uint8Array[] = []

      if (ctx.sdk7) {
        const tempReusableBuffer = new ReadWriteByteBuffer()
        const parcels: Set<number> = new Set()
        if (!ctx.sceneData.isGlobalScene) {
          ctx.sceneData.entity.pointers.forEach((pointer) => {
            const [x, z] = pointer.split(',').map((n) => parseInt(n, 10))
            parcels.add(getParcelNumber(x, z))
          })
        }

        console.log({ parcels })

        avatarSdk7MessageObservable.on('BinaryMessage', (message) => {
          sdk7AvatarUpdates.push(message)
        })

        avatarSdk7MessageObservable.on('RemoveAvatar', (message) => {
          sdk7AvatarUpdates.push(message.data)
          ctx.avatarEntityInsideScene.delete(message.entity)
        })

        avatarSdk7MessageObservable.on('ChangePosition', (message) => {
          const isInsideScene =
            ctx.sceneData.isGlobalScene || parcels.has(getParcelNumber(message.parcel.x, message.parcel.z))
          const wasInsideScene = ctx.avatarEntityInsideScene.get(message.entity) || false
          if (isInsideScene) {
            sdk7AvatarUpdates.push(message.data)

            if (!wasInsideScene) {
              ctx.avatarEntityInsideScene.set(message.entity, true)
            }
          } else if (wasInsideScene) {
            ctx.avatarEntityInsideScene.set(message.entity, false)

            tempReusableBuffer.resetBuffer()
            DeleteComponent.write(message.entity, Sdk7ComponentIds.TRANSFORM, message.ts, tempReusableBuffer)
            sdk7AvatarUpdates.push(tempReusableBuffer.toCopiedBinary())
          }
        })
      }

      return {
        async sendBatch(_req: ManyEntityAction, ctx) {
          // TODO: (2023/01/06) `sendBatch` is still used by sdk7 scenes to retreive
          // events (https://github.com/decentraland/js-sdk-toolchain/blob/7a4c5cb30db481254e4abe05de7b6c19a11fd884/packages/%40dcl/sdk/src/observables.ts#L516)
          // can't be uncomment until we remove that dependency
          // if (ctx.sdk7) throw new Error('Cannot use SDK6 APIs on SDK7 scene')

          const events: EventData[] = ctx.events
          if (events.length) {
            ctx.events = []
          }

          return { events }
        },

        async subscribe(req, ctx) {
          ctx.subscribedEvents.add(req.eventId)
          return {}
        },
        async unsubscribe(req, ctx) {
          ctx.subscribedEvents.delete(req.eventId)
          return {}
        },
        async crdtSendToRenderer(req, ctx) {
          if (!ctx.sdk7) throw new Error('Cannot use SDK7 APIs on SDK6 scene')
          // TODO: merge sendCrdt and pullCrdt calls into one
          //  if req.data.length == 0: the send can be ignored
          //  when there is only one method, the `if` should be
          //  implemented in the renderer-side (to check if there is data)
          //  and here should always call the rpc

          const ret = await ctx.rpcSceneControllerService.sendCrdt({
            payload: req.data
          })

          const avatarStates = sdk7AvatarUpdates
          sdk7AvatarUpdates = []

          return { data: [ret.payload, ...avatarStates] }
        },

        // @deprecated
        async crdtGetMessageFromRenderer(_, ctx) {
          const response = await ctx.rpcSceneControllerService.sendCrdt({
            payload: new Uint8Array()
          })
          return { data: [response.payload] }
        },

        async crdtGetState(_, ctx) {
          const response = await ctx.rpcSceneControllerService.getCurrentState({})

          const { initialEntitiesTick0, hasMainCrdt } = ctx

          return {
            hasEntities: response.hasOwnEntities || hasMainCrdt,
            // send the initialEntitiesTick0 (main.crdt) and the response.payload
            data: [initialEntitiesTick0, response.payload]
          }
        },
        async isServer(_req, _ctx) {
          return { isServer: false }
        }
      }
    }
  )
}
