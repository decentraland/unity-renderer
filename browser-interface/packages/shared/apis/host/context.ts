import { ILogger } from '../../logger'
import { EntityAction, LoadableScene } from '../../types'
import { PermissionItem } from '@dcl/protocol/out-ts/decentraland/kernel/apis/permissions.gen'
import { EventData } from '@dcl/protocol/out-ts/decentraland/kernel/apis/engine_api.gen'
import { RpcClientPort } from '@dcl/rpc'

type WithRequired<T, K extends keyof T> = T & { [P in K]-?: T[P] }

export type PortContextService<K extends keyof PortContext> = WithRequired<PortContext, K>

export type PortContext = {
  sdk7: boolean
  permissionGranted: Set<PermissionItem>
  sceneData: LoadableScene & {
    isPortableExperience: boolean
    useFPSThrottling: boolean
    readonly sceneNumber: number
  }
  // this only applies to SDK7. It should be removed by https://github.com/decentraland/sdk/issues/474
  __hack_sentInitialEventToUnity: boolean
  subscribedEvents: Set<string>
  events: EventData[]

  // @deprecated
  sendBatch(actions: EntityAction[]): void
  sendSceneEvent<K extends keyof IEvents>(id: K, event: IEvents[K]): void
  sendProtoSceneEvent(event: EventData): void
  logger: ILogger

  // port used for this specific scene in the renderer
  rendererPort: RpcClientPort
}
