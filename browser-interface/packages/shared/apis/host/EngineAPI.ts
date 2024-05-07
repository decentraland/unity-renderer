import type { RpcServerModule } from '@dcl/rpc/dist/codegen'
import * as codegen from '@dcl/rpc/dist/codegen'
import type { RpcServerPort } from '@dcl/rpc/dist/types'
import type { EventData, ManyEntityAction } from 'shared/protocol/decentraland/kernel/apis/engine_api.gen'
import { EngineApiServiceDefinition } from 'shared/protocol/decentraland/kernel/apis/engine_api.gen'
import type { PortContext } from './context'

export function registerEngineApiServiceServerImplementation(port: RpcServerPort<PortContext>) {
  codegen.registerService(
    port,
    EngineApiServiceDefinition,
    async (): Promise<RpcServerModule<EngineApiServiceDefinition, PortContext>> => {
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

          /**
           * We add this message here so the player components associated to engine.PlayerEntity
           * are present on the first tick.
           * PlayerIdentity.get(engine.PlayerEntity) // Returns the address associated to the current user
           */
          const internalAvatarMessages = (await ctx.internalEngine?.update()) ?? []

          return { data: [ret.payload, ...internalAvatarMessages] }
        },

        // @deprecated
        async crdtGetMessageFromRenderer(_, ctx) {
          const response = await ctx.rpcSceneControllerService.sendCrdt({
            payload: new Uint8Array()
          })
          return { data: [response.payload] }
        },

        async crdtGetState(_, ctx) {
          const { initialEntitiesTick0, hasMainCrdt } = ctx
          const response = await ctx.rpcSceneControllerService.getCurrentState({})

          // PlayerComponents messages
          const internalAvatarMessages = (await ctx.internalEngine?.update()) ?? []

          return {
            hasEntities: response.hasOwnEntities || hasMainCrdt,
            // send the initialEntitiesTick0 (main.crdt) and the response.payload
            data: [initialEntitiesTick0, response.payload, ...internalAvatarMessages]
          }
        },
        async isServer(_req, _ctx) {
          return { isServer: false }
        }
      }
    }
  )
}
