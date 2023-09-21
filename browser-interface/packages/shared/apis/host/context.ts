import type { RpcClientPort } from '@dcl/rpc'
import type { RpcClientModule } from '@dcl/rpc/dist/codegen'
import type { ILogger } from 'lib/logger'
import type { EventData } from 'shared/protocol/decentraland/kernel/apis/engine_api.gen'
import type { PermissionItem } from 'shared/protocol/decentraland/kernel/apis/permissions.gen'
import type { RpcSceneControllerServiceDefinition } from 'shared/protocol/decentraland/renderer/renderer_services/scene_controller.gen'
import { EntityAction } from 'shared/protocol/decentraland/sdk/ecs6/engine_interface_ecs6.gen'
import type { LoadableScene } from 'shared/types'
import type { Entity } from './runtime7/engine/entity'

type WithRequired<T, K extends keyof T> = T & { [P in K]-?: T[P] }

export type PortContextService<K extends keyof PortContext> = WithRequired<PortContext, K>

export type PortContext = {
  sdk7: boolean
  permissionGranted: Set<PermissionItem>
  sceneData: LoadableScene & {
    readonly sceneNumber: number
  }
  subscribedEvents: Set<string>
  events: EventData[]

  tickNumber: number
  sendBatchCalled: boolean
  avatarEntityInsideScene: Map<Entity, boolean>

  // @deprecated
  sendBatch(actions: EntityAction[]): void
  sendSceneEvent<K extends keyof IEvents>(id: K, event: IEvents[K]): void
  sendProtoSceneEvent(event: EventData): void
  logger: ILogger

  // port used for this specific scene in the renderer
  scenePort: RpcClientPort
  rpcSceneControllerService: RpcClientModule<RpcSceneControllerServiceDefinition, unknown>

  initialEntitiesTick0: Uint8Array
  hasMainCrdt: boolean

  readFile(path: string): Promise<{ content: Uint8Array; hash: string }>
}
