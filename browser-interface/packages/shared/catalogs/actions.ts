import { action } from 'typesafe-actions'
import { Catalog, Emote, EmotesRequestFilters, WearablesRequestFilters, WearableV2 } from './types'

export const CATALOG_LOADED = 'Catalog Loaded'
export const catalogLoaded = (name: string, catalog: Catalog) => action(CATALOG_LOADED, { name, catalog })
export type CatalogLoadedAction = ReturnType<typeof catalogLoaded>

export const WEARABLES_REQUEST = '[Request] Wearable fetch'
export const wearablesRequest = (filters: WearablesRequestFilters, context?: string) =>
  action(WEARABLES_REQUEST, { filters, context })
export type WearablesRequest = ReturnType<typeof wearablesRequest>

export const WEARABLES_SUCCESS = '[Success] Wearable fetch'
export const wearablesSuccess = (wearables: WearableV2[], context: string | undefined) =>
  action(WEARABLES_SUCCESS, { wearables, context })
export type WearablesSuccess = ReturnType<typeof wearablesSuccess>

export const WEARABLES_FAILURE = '[Failure] Wearable fetch'
export const wearablesFailure = (context: string | undefined, error: string) =>
  action(WEARABLES_FAILURE, { context, error })
export type WearablesFailure = ReturnType<typeof wearablesFailure>

export const EMOTES_REQUEST = '[Request] Emote fetch'
export const emotesRequest = (filters: EmotesRequestFilters, context?: string) =>
  action(EMOTES_REQUEST, { filters, context })
export type EmotesRequest = ReturnType<typeof emotesRequest>

export const EMOTES_SUCCESS = '[Success] Emote fetch'
export const emotesSuccess = (emotes: Emote[], context: string | undefined) =>
  action(EMOTES_SUCCESS, { emotes, context })
export type EmotesSuccess = ReturnType<typeof emotesSuccess>

export const EMOTES_FAILURE = '[Failure] Emote fetch'
export const emotesFailure = (context: string | undefined, error: string) => action(EMOTES_FAILURE, { context, error })
export type EmotesFailure = ReturnType<typeof emotesFailure>
