import { action } from 'typesafe-actions'
import { Catalog } from './types'

// Wearables catalog

export const ADD_CATALOG = 'Add Catalog'
export const addCatalog = (name: string, catalog: Catalog) => action(ADD_CATALOG, { name, catalog })
export type AddCatalogAction = ReturnType<typeof addCatalog>
export const CATALOG_LOADED = 'Catalog Loaded'
export const catalogLoaded = (name: string) => action(CATALOG_LOADED, { name })
export type CatalogLoadedAction = ReturnType<typeof catalogLoaded>
