import { Vector3 } from 'lib/math/Vector3'
import { Emitter } from 'mitt'
import { IslandChangedMessage } from 'shared/protocol/decentraland/kernel/comms/v3/archipelago.gen'
import { AboutResponse } from 'shared/protocol/decentraland/realm/about.gen'

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

export interface IRealmAdapter {
  readonly about: AboutResponse
  readonly baseUrl: string
  disconnect(error?: Error): Promise<void>
  sendHeartbeat(p: Vector3): void
  events: Emitter<RealmConnectionEvents>
  services: LegacyServices
}
