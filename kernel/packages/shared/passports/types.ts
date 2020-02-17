import { ReadOnlyColor4 } from 'decentraland-ecs/src'

export type Catalog = Wearable[]

export interface Profile {
  userId: string
  name: string
  hasClaimedName: boolean
  description: string
  email: string
  avatar: Avatar
  ethAddress: string | undefined
  inventory: WearableId[]
  blocked: string[]
  version: number
  tutorialStep: number
}

export interface Avatar {
  bodyShape: WearableId
  skinColor: ColorString
  hairColor: ColorString
  eyeColor: ColorString
  wearables: WearableId[]
  snapshots: {
    face: string
    body: string
  }
}

export interface ProfileForRenderer {
  userId: string
  name: string
  description: string
  email: string
  avatar: AvatarForRenderer
  ethAddress: string | undefined
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

export type Collection = { id: string; wearables: Wearable }

export type Wearable = {
  id: WearableId
  type: 'wearable'
  category: string
  baseUrl: string
  baseUrlBundles: string
  tags: string[]
  hides?: string[]
  replaces?: string[]
  representations: BodyShapeRespresentation[]
}

export type BodyShapeRespresentation = {
  bodyShapes: string[]
  mainFile: string
  overrideHides?: string[]
  overrideReplaces?: string[]
  contents: FileAndHash[]
}

export type FileAndHash = {
  file: string
  hash: string
}

export type WearableId = string

export type ColorString = string

export type PassportState = {
  profileServer: string
  userInfo: {
    [key: string]: { status: 'loading' | 'error'; data: any } | { status: 'ok'; data: Profile }
  }
  userInventory: {
    [key: string]: { status: 'loading' } | { status: 'error'; data: any } | { status: 'ok'; data: WearableId[] }
  }
  catalogs: {
    [key: string]: { id: string; status: 'loading' | 'error' | 'ok'; data?: Wearable[]; error?: any }
  }
}

export type RootPassportState = {
  passports: PassportState
}

export const INITIAL_PASSPORTS: PassportState = {
  profileServer: '',
  userInfo: {},
  userInventory: {},
  catalogs: {}
}
