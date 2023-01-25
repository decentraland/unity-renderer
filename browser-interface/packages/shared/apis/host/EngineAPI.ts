import * as codegen from '@dcl/rpc/dist/codegen'
import { RpcServerPort } from '@dcl/rpc/dist/types'
import {
  EAType,
  EngineApiServiceDefinition,
  EventData,
  ManyEntityAction,
  Payload,
  queryTypeToJSON
} from '@dcl/protocol/out-ts/decentraland/kernel/apis/engine_api.gen'

import { PortContext } from './context'
import { EntityAction, EntityActionType } from 'shared/types'
import { RpcServerModule } from '@dcl/rpc/dist/codegen'

function getPayload(payloadType: EAType, payload: Payload): any {
  switch (payloadType) {
    case EAType.EAT_OPEN_EXTERNAL_URL: {
      return payload.openExternalUrl?.url
    }
    case EAType.EAT_OPEN_NFT_DIALOG: {
      return payload.openNftDialog
    }
    case EAType.EAT_CREATE_ENTITY: {
      return payload.createEntity
    }
    case EAType.EAT_REMOVE_ENTITY: {
      return payload.removeEntity
    }
    case EAType.EAT_UPDATE_ENTITY_COMPONENT: {
      return payload.updateEntityComponent
    }
    case EAType.EAT_ATTACH_ENTITY_COMPONENT: {
      return payload.attachEntityComponent
    }
    case EAType.EAT_COMPONENT_REMOVED: {
      return payload.componentRemoved
    }
    case EAType.EAT_SET_ENTITY_PARENT: {
      return payload.setEntityParent
    }
    case EAType.EAT_QUERY: {
      return { queryId: queryTypeToJSON(payload.query!.queryId), payload: JSON.parse(payload.query!.payload) }
    }
    case EAType.EAT_COMPONENT_CREATED: {
      return payload.componentCreated
    }
    case EAType.EAT_COMPONENT_DISPOSED: {
      return payload.componentDisposed
    }
    case EAType.EAT_COMPONENT_UPDATED: {
      return payload.componentUpdated
    }
    case EAType.EAT_INIT_MESSAGES_FINISHED: {
      return payload.initMessagesFinished
    }
  }
  return {}
}

export function registerEngineApiServiceServerImplementation(port: RpcServerPort<PortContext>) {
  codegen.registerService(
    port,
    EngineApiServiceDefinition,
    async (): Promise<RpcServerModule<EngineApiServiceDefinition, PortContext>> => {
      return {
        async sendBatch(req: ManyEntityAction, ctx) {
          // TODO: (2023/01/06) `sendBatch` is still used by sdk7 scenes to retreive
          // events (https://github.com/decentraland/js-sdk-toolchain/blob/7a4c5cb30db481254e4abe05de7b6c19a11fd884/packages/%40dcl/sdk/src/observables.ts#L516)
          // can't be uncomment until we remove that dependency
          // if (ctx.sdk7) throw new Error('Cannot use SDK6 APIs on SDK7 scene')

          const actions: EntityAction[] = []

          for (const action of req.actions) {
            const actionType = eaTypeToStr(action.type)
            if (actionType && action.payload) {
              actions.push({
                type: actionType,
                tag: action.tag,
                payload: getPayload(action.type, action.payload as any)
              })
            }
          }

          if (actions.length) {
            ctx.sendBatch(actions)
          }

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

          return { data: [ret.payload] }
        },

        // @deprecated
        async crdtGetMessageFromRenderer(_, ctx) {
          const response = await ctx.rpcSceneControllerService.sendCrdt({
            payload: new Uint8Array()
          })
          return { data: [response.payload] }
        },

        // TODO: implement
        async crdtGetState() {
          return { data: [] }
        }
      }
    }
  )
}
function eaTypeToStr(type: EAType): EntityActionType | null {
  switch (type) {
    case EAType.EAT_UPDATE_ENTITY_COMPONENT:
      return 'UpdateEntityComponent'
    case EAType.EAT_COMPONENT_UPDATED:
      return 'ComponentUpdated'
    case EAType.EAT_COMPONENT_CREATED:
      return 'ComponentCreated'
    case EAType.EAT_CREATE_ENTITY:
      return 'CreateEntity'
    case EAType.EAT_OPEN_EXTERNAL_URL:
      return 'OpenExternalUrl'
    case EAType.EAT_OPEN_NFT_DIALOG:
      return 'OpenNFTDialog'
    case EAType.EAT_REMOVE_ENTITY:
      return 'RemoveEntity'
    case EAType.EAT_ATTACH_ENTITY_COMPONENT:
      return 'AttachEntityComponent'
    case EAType.EAT_COMPONENT_REMOVED:
      return 'ComponentRemoved'
    case EAType.EAT_SET_ENTITY_PARENT:
      return 'SetEntityParent'
    case EAType.EAT_QUERY:
      return 'Query'
    case EAType.EAT_COMPONENT_CREATED:
      return 'ComponentCreated'
    case EAType.EAT_COMPONENT_DISPOSED:
      return 'ComponentDisposed'
    case EAType.EAT_COMPONENT_UPDATED:
      return 'ComponentUpdated'
    case EAType.EAT_INIT_MESSAGES_FINISHED:
      return 'InitMessagesFinished'
  }
  return null
}
