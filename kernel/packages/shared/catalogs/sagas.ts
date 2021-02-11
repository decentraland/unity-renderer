import { call, put, select, take, takeEvery } from 'redux-saga/effects'

import {
  getServerConfigurations,
  getWearablesSafeURL,
  PIN_CATALYST,
  WSS_ENABLED,
  TEST_WEARABLES_OVERRIDE,
  ALL_WEARABLES
} from 'config'

import defaultLogger from 'shared/logger'
import { RENDERER_INITIALIZED } from 'shared/renderer/types'
import {
  catalogLoaded,
  CATALOG_LOADED,
  WearablesFailure,
  wearablesFailure,
  WearablesRequest,
  WearablesSuccess,
  wearablesSuccess,
  WEARABLES_FAILURE,
  WEARABLES_REQUEST,
  WEARABLES_SUCCESS
} from './actions'
import { baseCatalogsLoaded, getExclusiveCatalog, getPlatformCatalog } from './selectors'
import { Catalog, Wearable, Collection, WearableId, WearablesRequestFilters } from './types'
import { WORLD_EXPLORER } from '../../config/index'
import { getResourcesURL } from '../location'
import { UnityInterfaceContainer } from 'unity-interface/dcl'
import { StoreContainer } from '../store/rootTypes'
import { retrieve, store } from 'shared/cache'
import { ensureRealmInitialized } from 'shared/dao/sagas'
import { ensureRenderer } from 'shared/renderer/sagas'

declare const globalThis: Window & UnityInterfaceContainer & StoreContainer
export const WRONG_FILTERS_ERROR =
  'You must set one and only one filter for V1. Also, the only collection name allowed is base-avatars'

/**
 * This saga handles wearable definition fetching.
 *
 * When the renderer detects a new wearable, but it doesn't know its definition, then it will create a catalog request.
 *
 * This request will include the ids of the unknown wearables. We will then find the appropriate definition, and return it to the renderer.
 *
 */
export function* catalogsSaga(): any {
  yield takeEvery(RENDERER_INITIALIZED, initialLoad)

  yield takeEvery(WEARABLES_REQUEST, handleWearablesRequest)
  yield takeEvery(WEARABLES_SUCCESS, handleWearablesSuccess)
  yield takeEvery(WEARABLES_FAILURE, handleWearablesFailure)
}

function overrideBaseUrl(wearable: Wearable) {
  if (!TEST_WEARABLES_OVERRIDE) {
    return {
      ...wearable,
      baseUrl: getWearablesSafeURL() + '/contents/',
      baseUrlBundles: PIN_CATALYST ? '' : getServerConfigurations().contentAsBundle + '/'
    }
  } else {
    return wearable
  }
}

function* initialLoad() {
  yield call(ensureRealmInitialized)

  if (WORLD_EXPLORER) {
    try {
      const catalogUrl = getServerConfigurations().avatar.catalog

      let collections: Collection[] | undefined
      if (globalThis.location.search.match(/TEST_WEARABLES/)) {
        collections = [{ id: 'all', wearables: (yield call(fetchCatalog, catalogUrl))[0] }]
      } else {
        const cached = yield retrieve('catalog')

        if (cached) {
          const version = yield headCatalog(catalogUrl)
          if (cached.version === version) {
            collections = cached.data
          }
        }

        if (!collections) {
          const response = yield call(fetchCatalog, catalogUrl)
          collections = response[0]

          const version = response[1]
          if (version) {
            yield store('catalog', { version, data: response[0] })
          }
        }
      }
      const catalog: Wearable[] = collections!
        .reduce((flatten, collection) => flatten.concat(collection.wearables), [] as Wearable[])
        .filter((wearable) => !!wearable)
        .map(overrideBaseUrl)
      const baseAvatars = catalog.filter((_: Wearable) => _.tags && !_.tags.includes('exclusive'))
      const baseExclusive = catalog.filter((_: Wearable) => _.tags && _.tags.includes('exclusive'))
      yield put(catalogLoaded('base-avatars', baseAvatars))
      yield put(catalogLoaded('base-exclusive', baseExclusive))
    } catch (error) {
      defaultLogger.error('[FATAL]: Could not load catalog!', error)
    }
  } else {
    let baseCatalog = []
    try {
      const catalogPath = '/default-profile/basecatalog.json'
      const response = yield fetch(getResourcesURL() + catalogPath)
      baseCatalog = yield response.json()

      if (WSS_ENABLED) {
        for (let item of baseCatalog) {
          item.baseUrl = `http://localhost:8000${item.baseUrl}`
        }
      }
    } catch (e) {
      defaultLogger.warn(`Could not load base catalog`)
    }
    yield put(catalogLoaded('base-avatars', baseCatalog))
    yield put(catalogLoaded('base-exclusive', []))
  }
}

export function* handleWearablesRequest(action: WearablesRequest) {
  const { filters, context } = action.payload

  const valid = areFiltersValid(filters)
  if (valid) {
    try {
      yield call(ensureBaseCatalogs)

      const platformCatalog = yield select(getPlatformCatalog)
      const exclusiveCatalog = yield select(getExclusiveCatalog)

      let response: Wearable[]
      if (filters.wearableIds) {
        // Filtering by ids
        response = filters.wearableIds
          .map((wearableId) =>
            wearableId.includes(`base-avatars`) ? platformCatalog[wearableId] : exclusiveCatalog[wearableId]
          )
          .filter((wearable) => !!wearable)
      } else if (filters.ownedByUser) {
        // Only owned wearables
        if (ALL_WEARABLES) {
          response = Object.values(exclusiveCatalog)
        } else {
          const inventoryItemIds: WearableId[] = yield call(fetchInventoryItemsByAddress, filters.ownedByUser)
          response = inventoryItemIds.map((id) => exclusiveCatalog[id]).filter((wearable) => !!wearable)
        }
      } else if (filters.collectionIds) {
        // We assume that the only collection name used is base-avatars
        response = Object.values(platformCatalog)
      } else {
        throw new Error('Unknown filter')
      }
      yield put(wearablesSuccess(response, context))
    } catch (error) {
      yield put(wearablesFailure(context, error.message))
    }
  } else {
    yield put(wearablesFailure(context, WRONG_FILTERS_ERROR))
  }
}

export function* handleWearablesSuccess(action: WearablesSuccess) {
  const { wearables, context } = action.payload

  yield call(ensureRenderer)
  yield call(sendWearablesCatalog, wearables, context)
}

export function* handleWearablesFailure(action: WearablesFailure) {
  const { context, error } = action.payload

  defaultLogger.error(`Failed to fetch wearables for context '${context}'`, error)

  yield call(ensureRenderer)
  yield call(informRequestFailure, error, context)
}

function areFiltersValid(filters: WearablesRequestFilters) {
  let filtersSet = 0
  let ok = true
  if (filters.collectionIds) {
    filtersSet += 1
    if (filters.collectionIds.some((name) => name !== 'base-avatars')) {
      ok = false
    }
  }

  if (filters.ownedByUser) {
    filtersSet += 1
  }

  if (filters.wearableIds) {
    filtersSet += 1
  }

  return filtersSet === 1 && ok
}

async function headCatalog(url: string) {
  const request = await fetch(url, { method: 'HEAD' })
  if (!request.ok) {
    throw new Error('Catalog not found')
  }
  return request.headers.get('etag')
}

async function fetchCatalog(url: string) {
  const request = await fetch(url)
  if (!request.ok) {
    throw new Error('Catalog not found')
  }
  const etag = request.headers.get('etag')
  return [await request.json(), etag]
}

export function informRequestFailure(error: string, context: string | undefined) {
  globalThis.unityInterface.WearablesRequestFailed(error, context)
}

export function sendWearablesCatalog(catalog: Catalog, context: string | undefined) {
  globalThis.unityInterface.AddWearablesToCatalog(catalog, context)
}

export function* ensureBaseCatalogs() {
  while (!(yield select(baseCatalogsLoaded))) {
    yield take(CATALOG_LOADED)
  }
}

export async function fetchInventoryItemsByAddress(address: string): Promise<WearableId[]> {
  if (!WORLD_EXPLORER) {
    return []
  }
  const result = await fetch(`${getServerConfigurations().wearablesApi}/addresses/${address}/wearables?fields=id`)
  if (!result.ok) {
    throw new Error('Unable to fetch inventory for address ' + address)
  }
  const inventory: { id: string }[] = await result.json()

  return inventory.map((wearable) => wearable.id)
}
