import { AuthIdentity } from 'dcl-crypto'

import { ETHEREUM_NETWORK } from 'config'

export type RootSessionState = {
  session: SessionState
}

export type ExplorerIdentity = AuthIdentity & {
  address: string
  hasConnectedWeb3: boolean
}

export type SessionState = {
  initialized: boolean
  userId: string | undefined
  identity: ExplorerIdentity | undefined
  network: ETHEREUM_NETWORK | undefined
}

export type StoredSession = {
  userId: string
  identity: ExplorerIdentity
}
