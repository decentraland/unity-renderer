import type { RpcServerModule } from '@dcl/rpc/dist/codegen'
import * as codegen from '@dcl/rpc/dist/codegen'
import type { RpcServerPort } from '@dcl/rpc/dist/types'
import type { EventData, ManyEntityAction } from 'shared/protocol/decentraland/kernel/apis/engine_api.gen'
import { EngineApiServiceDefinition } from 'shared/protocol/decentraland/kernel/apis/engine_api.gen'
import type { PortContext } from './context'

import { avatarSdk7Ecs, avatarSdk7MessageObservable } from './runtime7/avatar'
import { DeleteComponent } from './runtime7/serialization/crdt/deleteComponent'
import { ReadWriteByteBuffer } from './runtime7/serialization/ByteBuffer'
import { PBPointerEventsResult, Sdk7ComponentIds } from './runtime7/avatar/ecs'
import { buildAvatarTransformMessage } from './runtime7/serialization/transform'
import { AppendValueOperation } from './runtime7/serialization/crdt/appendValue'
import { InputAction, PointerEventType } from 'shared/protocol/decentraland/sdk/components/common/input_action.gen'

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

        const [baseX, baseZ] = ctx.sceneData.entity.metadata?.scene?.base?.split(',').map((n) => parseInt(n, 10)) ?? [
          0, 0
        ]
        const offset = { x: baseX * 16.0, y: 0, z: baseZ * 16.0 }

        sdk7AvatarUpdates = avatarSdk7Ecs.getState()

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
            sdk7AvatarUpdates.push(buildAvatarTransformMessage(message.entity, message.ts, message.data, offset))

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

        ctx.subscribedEvents.add('playerClicked')
      }

      const crdtReusableBuffer = new ReadWriteByteBuffer()
      let localTimestamp = 0
      function getPlayerClickedEvents(): Uint8Array[] {
        const msgs: Uint8Array[] = []
        const playerClickedEvents = ctx.events.filter((value) => value.generic?.eventId === 'playerClicked')
        for (const event of playerClickedEvents) {
          event.generic?.eventData
          const { userId, ray } = JSON.parse(event.generic!.eventData)
          const { origin, direction, distance } = ray

          localTimestamp++
          const entityId = avatarSdk7Ecs.ensureAvatarEntityId(userId)

          {
            crdtReusableBuffer.resetBuffer()

            const writer = PBPointerEventsResult.encode({
              button: InputAction.IA_POINTER,
              hit: {
                position: origin,
                globalOrigin: origin,
                direction,
                normalHit: undefined,
                length: distance,
                entityId: entityId
              },
              state: PointerEventType.PET_DOWN,
              timestamp: localTimestamp,
              tickNumber: ctx.tickNumber
            })
            const buffer = new Uint8Array(writer.finish(), 0, writer.len)
            AppendValueOperation.write(
              entityId,
              localTimestamp,
              Sdk7ComponentIds.POINTER_EVENTS_RESULT,
              buffer,
              crdtReusableBuffer
            )

            const messageData = crdtReusableBuffer.toCopiedBinary()
            msgs.push(messageData)
          }
        }
        return msgs
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
          ctx.sendBatchCalled = true
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

          const avatarPointerEvents = getPlayerClickedEvents()
          const data: Uint8Array[] = [ret.payload, ...avatarPointerEvents, ...sdk7AvatarUpdates]

          sdk7AvatarUpdates = []
          ctx.tickNumber++

          // If the sendBatch is not being called after 10 ticks, clean the events her (after getting data from playerClickedEvents)
          // Corner case: if the player click other player in this windows of 10 ticks, it'll receive a bunch of times
          if (!ctx.sendBatchCalled && ctx.tickNumber > 10) {
            ctx.events = []
          }

          return { data }
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
