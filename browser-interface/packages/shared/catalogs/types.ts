import type { EmoteDataADR74 } from '@dcl/schemas'

export type Catalog = PartialWearableV2[]

export type Collection = { id: string; wearables: Wearable }
export type RarityEnum = 'common' | 'uncommon' | 'rare' | 'epic' | 'mythic' | 'legendary' | 'unique'

export type Wearable = {
  id: WearableId
  type: 'wearable'
  category: string
  baseUrl: string
  baseUrlBundles: string
  description: string
  tags: string[]
  hides?: string[]
  replaces?: string[]
  rarity: RarityEnum
  representations: BodyShapeRepresentation[]
  i18n: { code: string; text: string }[]
  thumbnail: string
}

export enum UnpublishedWearableType {
  WEARABLE = 'wearable',
  EMOTE = 'emote'
}

export type UnpublishedWearable = {
  id: string // uuid
  rarity: string
  name: string
  thumbnail: string
  description: string
  type: UnpublishedWearableType
  data: {
    category: string
    tags: string[]
    hides?: string[]
    replaces?: string[]
    representations: UnpublishedBodyShapeRepresentation[]
    loop?: boolean
  }
  contents: Record<string, string> // from file name to hash
}

type UnpublishedBodyShapeRepresentation = {
  bodyShapes: string[]
  mainFile: string
  overrideHides?: string[]
  overrideReplaces?: string[]
  contents: string[]
}

export type WearableV2 = {
  id: string
  rarity: string
  i18n: { code: string; text: string }[]
  thumbnail: string
  description: string
  data: {
    category: string
    tags: string[]
    hides?: string[]
    replaces?: string[]
    representations: BodyShapeRepresentationV2[]
  }
  baseUrl: string
  baseUrlBundles: string
  menuBarIcon?: string
}

export type Emote = Omit<WearableV2, 'data'> & {
  emoteDataADR74: Omit<EmoteDataADR74, 'contents'> & { contents: KeyAndHash[] }
}

export type BodyShapeRepresentationV2 = {
  bodyShapes: string[]
  mainFile: string
  overrideHides?: string[]
  overrideReplaces?: string[]
  contents: KeyAndHash[]
}

type KeyAndHash = {
  key: string
  hash: string
}

export type PartialWearableV2 = PartialBy<Omit<WearableV2, 'baseUrlBundles'>, 'baseUrl'>
export type PartialEmote = PartialBy<Omit<Emote, 'baseUrlBundles'>, 'baseUrl'>
export type PartialItem = PartialWearableV2 | PartialEmote
type PartialBy<T, K extends keyof T> = Omit<T, K> & Partial<Pick<T, K>>

export const isPartialWearable = (partialItem: PartialItem): partialItem is PartialWearableV2 =>
  !!(partialItem as PartialWearableV2).data

export type BodyShapeRepresentation = {
  bodyShapes: string[]
  mainFile: string
  overrideHides?: string[]
  overrideReplaces?: string[]
  contents: FileAndHash[]
}

type FileAndHash = {
  file: string
  hash: string
}

export type WearableId = string
export type EmoteId = string

export type ColorString = string

export type CatalogState = {
  catalogs: {
    [key: string]: { id: string; status: 'error' | 'ok'; data?: Record<WearableId, PartialWearableV2>; error?: any }
  }
}

export type RootCatalogState = {
  catalogs: CatalogState
}

export type WearablesRequestFilters = {
  ownedByUser?: string
  wearableIds?: WearableId[]
  collectionIds?: string[]
  thirdPartyId?: string
}

export const areWearablesRequestFilters = (
  filters: WearablesRequestFilters | EmotesRequestFilters
): filters is WearablesRequestFilters => !!(filters as WearablesRequestFilters).wearableIds

export type EmotesRequestFilters = Omit<WearablesRequestFilters, 'wearableIds'> & { emoteIds?: EmoteId[] }
