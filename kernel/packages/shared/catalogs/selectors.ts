import { RootCatalogState, Wearable } from './types'

export const getPlatformCatalog = (store: RootCatalogState): Wearable[] | null =>
  store.catalogs &&
  store.catalogs.catalogs &&
  store.catalogs.catalogs['base-avatars'] &&
  store.catalogs.catalogs['base-avatars'].status === 'ok'
    ? (store.catalogs.catalogs['base-avatars'].data as Wearable[])
    : null

export const getExclusiveCatalog = (store: RootCatalogState): Wearable[] | null =>
  store.catalogs.catalogs &&
  store.catalogs.catalogs['base-exclusive'] &&
  store.catalogs.catalogs['base-exclusive'].status === 'ok'
    ? (store.catalogs.catalogs['base-exclusive'].data as Wearable[])
    : null

export const baseCatalogsLoaded = (store: RootCatalogState) =>
  store.catalogs &&
  store.catalogs.catalogs &&
  store.catalogs.catalogs['base-avatars'] &&
  store.catalogs.catalogs['base-avatars'].status === 'ok' &&
  store.catalogs.catalogs['base-exclusive'] &&
  store.catalogs.catalogs['base-exclusive'].status === 'ok'
