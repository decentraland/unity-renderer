import type { ETHEREUM_NETWORK } from 'config'
import { AboutResponse } from '@dcl/protocol/out-ts/decentraland/bff/http_endpoints.gen'

export enum ServerConnectionStatus {
  OK,
  UNREACHABLE
}

export type CatalystStatus = {
  name: string
  version: string
  usersCount?: number
  maxUsers?: number
  usersParcels?: Parcel[]
}

type BaseCandidate = {
  // connectionString: string
  protocol: string
  domain: string
  catalystName: string
  elapsed: number
  status: ServerConnectionStatus
}

export type Candidate = {
  usersCount: number
  usersParcels?: Parcel[]
  maxUsers?: number
} & BaseCandidate

export type Parcel = [number, number]

export type LayerUserInfo = {
  userId: string
  peerId: string
  protocolVersion: number
  parcel?: Parcel
}

export type Realm = {
  protocol: string
  hostname: string
  serverName: string
}

export type DaoState = {
  network: ETHEREUM_NETWORK | null
  candidates: Candidate[]
  catalystCandidatesReceived: boolean
}

export type RootDaoState = {
  dao: DaoState
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
