export type Layer = {
  name: string
  usersCount: number
  maxUsers: number
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
  updateContentServer: string
  commsServer: string
  layer: string
  realm: Realm | undefined
  candidates: Candidate[]
  commsStatus: CommsStatus
}

export type RootDaoState = {
  dao: DaoState
}

export type CommsState = 'initial' | 'connecting' | 'connected' | 'error'

export type CommsStatus = {
  status: CommsState
  connectedPeers: number
}
