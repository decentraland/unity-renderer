import { RarityEnum } from '../airdrops/interface'

export type Catalog = Wearable[]

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
  representations: BodyShapeRepresentation[]
  i18n: { code: string; text: string }[]
  thumbnail: string
}

export type BodyShapeRepresentation = {
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

export type CatalogState = {
  catalogs: {
    [key: string]: { id: string; status: 'error' | 'ok'; data?: Record<WearableId, Wearable>; error?: any }
  }
}

export type RootCatalogState = {
  catalogs: CatalogState
}

export type WearablesRequestFilters = {
  ownedByUser?: string
  wearableIds?: WearableId[]
  collectionIds?: string[]
}
