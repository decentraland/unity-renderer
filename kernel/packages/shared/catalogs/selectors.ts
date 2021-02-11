import { RootCatalogState, Wearable, WearableId } from './types'

export const getPlatformCatalog = (store: RootCatalogState): Record<WearableId, Wearable> | null =>
  store.catalogs &&
  store.catalogs.catalogs &&
  store.catalogs.catalogs['base-avatars'] &&
  store.catalogs.catalogs['base-avatars'].status === 'ok'
    ? store.catalogs.catalogs['base-avatars'].data!
    : null

export const getExclusiveCatalog = (store: RootCatalogState): Record<WearableId, Wearable> | null =>
  store.catalogs.catalogs &&
  store.catalogs.catalogs['base-exclusive'] &&
  store.catalogs.catalogs['base-exclusive'].status === 'ok'
    ? store.catalogs.catalogs['base-exclusive'].data!
    : null

export const baseCatalogsLoaded = (store: RootCatalogState) =>
  store.catalogs &&
  store.catalogs.catalogs &&
  store.catalogs.catalogs['base-avatars'] &&
  store.catalogs.catalogs['base-avatars'].status === 'ok' &&
  store.catalogs.catalogs['base-exclusive'] &&
  store.catalogs.catalogs['base-exclusive'].status === 'ok'
