import { ReadOnlyColor4 } from 'decentraland-ecs/src'
import { WearableId } from 'shared/catalogs/types'

export interface Profile {
  userId: string
  name: string
  hasClaimedName: boolean
  description: string
  email: string
  avatar: Avatar
  ethAddress: string
  inventory: WearableId[]
  blocked?: string[]
  muted?: string[]
  snapshots?: Snapshots
  version: number
  tutorialStep: number
  interests?: string[]
  unclaimedName: string
}

export interface Avatar {
  bodyShape: WearableId
  skinColor: ColorString
  hairColor: ColorString
  eyeColor: ColorString
  wearables: WearableId[]
  snapshots: Snapshots
}

export type Snapshots = {
  face: string
  face256: string
  face128: string
  body: string
}

export interface ProfileForRenderer {
  userId: string
  name: string
  description: string
  email: string
  avatar: AvatarForRenderer
  ethAddress: string
  inventory: WearableId[]
  snapshots: {
    face: string
    body: string
  }
  version: number
}

export interface AvatarForRenderer {
  bodyShape: WearableId
  skinColor: ReadOnlyColor4
  hairColor: ReadOnlyColor4
  eyeColor: ReadOnlyColor4
  wearables: WearableId[]
}

export type FileAndHash = {
  file: string
  hash: string
}

export type ColorString = string

export type ProfileStatus = 'ok' | 'error' | 'loading'

export type ProfileUserInfo =
  | { status: 'loading' | 'error'; data: any; hasConnectedWeb3: boolean; addedToCatalog?: boolean }
  | { status: 'ok'; data: Profile; hasConnectedWeb3: boolean; addedToCatalog?: boolean }

export type ProfileState = {
  userInfo: {
    [key: string]: ProfileUserInfo
  }
  userInventory: {
    [key: string]: { status: 'loading' } | { status: 'error'; data: any } | { status: 'ok'; data: WearableId[] }
  }
}

export type RootProfileState = {
  profiles: ProfileState
}

export type ContentFile = {
  name: string
  content: Buffer
}

export enum ProfileType {
  LOCAL,
  DEPLOYED
}
