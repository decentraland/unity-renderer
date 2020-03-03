export type Layer = {
  name: string
  usersCount: number
  maxUsers: number
  usersParcels?: [number, number][]
}

export enum ServerConnectionStatus {
  OK,
  UNREACHABLE
}

export type CatalystLayers = {
  name: string
  layers: Layer[]
}

export type Candidate = {
  domain: string
  catalystName: string
  elapsed: number
  score: number
  layer: Layer
  status: ServerConnectionStatus
}

export type LayerUserInfo = {
  userId: string
  peerId: string
  protocolVersion: number
  parcel?: [number, number]
}

export type Realm = {
  domain: string
  catalystName: string
  layer: string
}

export type DaoState = {
  initialized: boolean
  profileServer: string
  fetchContentServer: string
  fetchMetaContentServer: string
  updateContentServer: string
  commsServer: string
  realm: Realm | undefined
  candidates: Candidate[]
  contentWhitelist: Candidate[]
  addedCandidates: Candidate[]
  commsStatus: CommsStatus
}

export type RootDaoState = {
  dao: DaoState
}

export type CommsState = 'initial' | 'connecting' | 'connected' | 'error' | 'realm-full' | 'reconnection-error'

export type CommsStatus = {
  status: CommsState
  connectedPeers: number
}

export type PingResult = {
  elapsed?: number
  status?: ServerConnectionStatus
  result?: CatalystLayers
}
