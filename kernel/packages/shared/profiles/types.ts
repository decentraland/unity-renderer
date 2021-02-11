import { WearableId } from 'shared/catalogs/types'

export interface Profile {
  userId: string
  name: string
  hasClaimedName: boolean
  description: string
  email: string
  avatar: Avatar
  ethAddress: string
  blocked?: string[]
  muted?: string[]
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

export type ColorString = string

export type ProfileStatus = 'ok' | 'error' | 'loading'

export type ProfileUserInfo =
  | { status: 'loading' | 'error'; data: any; hasConnectedWeb3: boolean; addedToCatalog?: boolean }
  | { status: 'ok'; data: Profile; hasConnectedWeb3: boolean; addedToCatalog?: boolean }

export type ProfileState = {
  userInfo: {
    [key: string]: ProfileUserInfo
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
