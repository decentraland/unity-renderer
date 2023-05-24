import { AuthIdentity } from '@dcl/crypto'

import { ETHEREUM_NETWORK } from 'config'
import { IEthereumProvider, LoginState } from '@dcl/kernel-interface'

export type RootSessionState = {
  session: SessionState
}

export type ExplorerIdentity = AuthIdentity & {
  address: string // contains the lowercased address that will be used for the userId
  rawAddress: string // contains the real ethereum address of the current user
  hasConnectedWeb3: boolean
}

export type SessionState = {
  identity: ExplorerIdentity | undefined
  network: ETHEREUM_NETWORK | undefined
  loginState: LoginState | undefined
  isSignUp?: boolean
  isGuestLogin?: boolean
  provider?: IEthereumProvider
}

export type StoredSession = {
  identity: ExplorerIdentity
  isGuest?: boolean
}
