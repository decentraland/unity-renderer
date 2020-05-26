import { ReadOnlyColor4 } from 'decentraland-ecs/src'
import { AuthLink } from 'dcl-crypto'
import { RarityEnum } from '../airdrops/interface'

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
  snapshots?: {
    face: string
    body: string
  }
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
  rarity: RarityEnum
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

export type ProfileState = {
  userInfo: {
    [key: string]:
      | { status: 'loading' | 'error'; data: any; hasConnectedWeb3: boolean; addedToCatalog?: boolean }
      | { status: 'ok'; data: Profile; hasConnectedWeb3: boolean; addedToCatalog?: boolean }
  }
  userInventory: {
    [key: string]: { status: 'loading' } | { status: 'error'; data: any } | { status: 'ok'; data: WearableId[] }
  }
  catalogs: {
    [key: string]: { id: string; status: 'loading' | 'error' | 'ok'; data?: Wearable[]; error?: any }
  }
}

export type RootProfileState = {
  profiles: ProfileState
}

export const INITIAL_PROFILES: ProfileState = {
  userInfo: {},
  userInventory: {},
  catalogs: {}
}

export type Timestamp = number
export type Pointer = string
export type ContentFileHash = string
export type ContentFile = {
  name: string
  content: Buffer
}
export type DeployData = {
  entityId: string
  ethAddress?: string
  signature?: string
  authChain: AuthLink[]
  files: ContentFile[]
}
export type ControllerEntity = {
  id: string
  type: string
  pointers: string[]
  timestamp: number
  content?: ControllerEntityContent[]
  metadata?: any
}
export type ControllerEntityContent = {
  file: string
  hash: string
}
export enum EntityType {
  SCENE = 'scene',
  WEARABLE = 'wearable',
  PROFILE = 'profile'
}
export type EntityId = ContentFileHash
export enum EntityField {
  CONTENT = 'content',
  POINTERS = 'pointers',
  METADATA = 'metadata'
}
export const ENTITY_FILE_NAME = 'entity.json'
export class Entity {
  constructor(
    public readonly id: EntityId,
    public readonly type: EntityType,
    public readonly pointers: Pointer[],
    public readonly timestamp: Timestamp,
    public readonly content?: Map<string, ContentFileHash>,
    public readonly metadata?: any
  ) {}
}
