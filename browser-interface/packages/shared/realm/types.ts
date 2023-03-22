import { RpcClientModule } from '@dcl/rpc/dist/codegen'
import { Emitter } from 'mitt'
import { CommsServiceDefinition } from 'protocol-gen/decentraland/bff/comms_service.gen'
import { AboutResponse } from 'protocol-gen/decentraland/bff/http_endpoints.gen'
import { IslandChangedMessage } from 'protocol-gen/decentraland/kernel/comms/v3/archipelago.gen'

export const OFFLINE_REALM = 'offline'

export type RealmState = {
  realmAdapter: IRealmAdapter | undefined
  previousAdapter: IRealmAdapter | undefined
}

export type OnboardingState = {
  isInOnboarding: boolean
  onboardingRealm: string | undefined
}

export type RootRealmState = {
  realm: RealmState
  onboarding: OnboardingState
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
