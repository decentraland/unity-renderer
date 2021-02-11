import { action } from 'typesafe-actions'
import { Catalog, Wearable, WearablesRequestFilters } from './types'

export const CATALOG_LOADED = 'Catalog Loaded'
export const catalogLoaded = (name: string, catalog: Catalog) => action(CATALOG_LOADED, { name, catalog })
export type CatalogLoadedAction = ReturnType<typeof catalogLoaded>

export const WEARABLES_REQUEST = '[Request] Wearable fetch'
export const wearablesRequest = (filters: WearablesRequestFilters, context?: string) =>
  action(WEARABLES_REQUEST, { filters, context })
export type WearablesRequest = ReturnType<typeof wearablesRequest>

export const WEARABLES_SUCCESS = '[Success] Wearable fetch'
export const wearablesSuccess = (wearables: Wearable[], context: string | undefined) =>
  action(WEARABLES_SUCCESS, { wearables, context })
export type WearablesSuccess = ReturnType<typeof wearablesSuccess>

export const WEARABLES_FAILURE = '[Failure] Wearable fetch'
export const wearablesFailure = (context: string | undefined, error: string) =>
  action(WEARABLES_FAILURE, { context, error })
export type WearablesFailure = ReturnType<typeof wearablesFailure>
