import { AuthIdentity } from 'dcl-crypto'
import { ProviderType } from 'decentraland-connect'

import { ETHEREUM_NETWORK } from 'config'
import { Profile } from '../profiles/types'

export type RootSessionState = {
  session: SessionState
}

export type ExplorerIdentity = AuthIdentity & {
  address: string // contains the lowercased address that will be used for the userId
  rawAddress: string // contains the real ethereum address of the current user
  provider?: ProviderType | null
  hasConnectedWeb3: boolean
}

export type SignUpData = {
  stage: string
  profile: Partial<Profile>
  userId?: string
  identity?: ExplorerIdentity
}

export enum LoginStage {
  LOADING = 'loading',
  SIGN_IN = 'signIn',
  SIGN_UP = 'signUp',
  CONNECT_ADVICE = 'connect_advice',
  SIGN_ADVICE = 'sign_advice',
  COMPLETED = 'completed'
}

export enum SignUpStage {
  AVATAR = 'avatar',
  PASSPORT = 'passport',
  TERMS = 'terms'
}

export type SessionState = {
  initialized: boolean
  userId: string | undefined
  identity: ExplorerIdentity | undefined
  network: ETHEREUM_NETWORK | undefined
  loginStage: LoginStage | undefined
  tos: boolean
  showWalletPrompt: boolean
  currentProvider: ProviderType | null
  isSignUp?: boolean
  signing: boolean
  signup: SignUpData
}

export type StoredSession = {
  userId: string
  identity: ExplorerIdentity
}
