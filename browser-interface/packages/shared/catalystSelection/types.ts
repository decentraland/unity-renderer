import type { ETHEREUM_NETWORK } from 'config'
import { AboutResponse } from '@dcl/protocol/out-ts/decentraland/bff/http_endpoints.gen'

export enum ServerConnectionStatus {
  OK,
  UNREACHABLE
}

type BaseCandidate = {
  // connectionString: string
  protocol: string
  domain: string
  catalystName: string
  elapsed: number
  status: ServerConnectionStatus
  lastConnectionAttempt?: number
}

export type Candidate = {
  usersCount: number
  usersParcels?: Parcel[]
  maxUsers?: number
} & BaseCandidate

export type Parcel = [number, number]

export type Realm = {
  protocol: string
  hostname: string
  serverName: string
}

export type CatalystSelectionState = {
  network: ETHEREUM_NETWORK | null
  candidates: Candidate[]
  catalystCandidatesReceived: boolean
  currentCatalyst: Realm | null
}

export type RootCatalystSelectionState = {
  catalystSelection: CatalystSelectionState
}

export type PingResult = {
  elapsed?: number
  status?: ServerConnectionStatus
  result?: AboutResponse
}

export type AskResult = {
  httpStatus?: number
  elapsed?: number
  status?: ServerConnectionStatus
  result?: any
}
