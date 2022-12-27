import { RpcClientModule } from '@dcl/rpc/dist/codegen'
import { Emitter } from 'mitt'
import { CommsServiceDefinition } from '@dcl/protocol/out-ts/decentraland/bff/comms_service.gen'
import { AboutResponse } from '@dcl/protocol/out-ts/decentraland/bff/http_endpoints.gen'
import { IslandChangedMessage } from '@dcl/protocol/out-ts/decentraland/kernel/comms/v3/archipelago.gen'

export const OFFLINE_REALM = 'offline'

export type RealmState = {
  realmAdapter: IRealmAdapter | undefined
}

export type RootRealmState = {
  realm: RealmState
}

export type RealmConnectionEvents = {
  DISCONNECTION: { error?: Error }
  setIsland: IslandChangedMessage
}

export type LegacyServices = {
  fetchContentServer: string
  lambdasServer: string
  updateContentServer: string
  hotScenesService: string
  exploreRealmsService: string
  poiService: string
}

export type BffServices<CallContext = any> = {
  comms: RpcClientModule<CommsServiceDefinition, CallContext>
  legacy: LegacyServices
}

export interface IRealmAdapter<CallContext = any> {
  readonly about: AboutResponse
  readonly baseUrl: string
  disconnect(error?: Error): Promise<void>
  events: Emitter<RealmConnectionEvents>
  services: BffServices<CallContext>
}
