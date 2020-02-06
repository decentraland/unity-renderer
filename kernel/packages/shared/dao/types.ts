export type Layer = {
  name: string
  usersCount: number
  maxUsers: number
}

export type Candidate = {
  domain: string
  elapsed: number
  layer: Layer
}

export type Realm = {
  domain: string
  layer: string
}

export type DaoState = {
  initialized: boolean
  profileServer: string
  fetchContentServer: string
  updateContentServer: string
  commsServer: string
  layer: string
}

export type RootDaoState = {
  dao: DaoState
}
