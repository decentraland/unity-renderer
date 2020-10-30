import { call, put, select, take, takeEvery, takeLatest } from 'redux-saga/effects'

import {
  getServerConfigurations,
  getWearablesSafeURL,
  PIN_CATALYST,
  WSS_ENABLED,
  TEST_WEARABLES_OVERRIDE
} from 'config'

import defaultLogger from 'shared/logger'
import { isInitialized } from 'shared/renderer/selectors'
import { RENDERER_INITIALIZED } from 'shared/renderer/types'
import { addCatalog, AddCatalogAction, ADD_CATALOG, catalogLoaded, CATALOG_LOADED } from './actions'
import { baseCatalogsLoaded } from './selectors'
import { Catalog, Wearable, Collection } from './types'
import { WORLD_EXPLORER } from '../../config/index'
import { getResourcesURL } from '../location'
import { UnityInterfaceContainer } from 'unity-interface/dcl'
import { RarityEnum } from '../airdrops/interface'
import { StoreContainer } from '../store/rootTypes'
import { retrieve, store } from 'shared/cache'
import { ensureRealmInitialized } from 'shared/dao/sagas'

declare const globalThis: Window & UnityInterfaceContainer & StoreContainer

/**
 * This saga handles both passports and assets required for the renderer to show the
 * users' inventory and avatar editor.
 *
 * When the renderer is initialized, it will fetch the asset catalog and submit it to the renderer.
 *
 * Whenever a passport is requested, it will fetch it and store it locally (see also: `selectors.ts`)
 *
 * If a user avatar was not found, it will create a random passport (see: `handleRandomAsSuccess`)
 *
 * Lastly, we handle save requests by submitting both to the avatar legacy server as well as to the profile server.
 *
 * It's *very* important for the renderer to never receive a passport with items that have not been loaded into the catalog.
 */
export function* catalogsSaga(): any {
  yield takeEvery(RENDERER_INITIALIZED, initialLoad)

  yield takeLatest(ADD_CATALOG, handleAddCatalog)
}

function overrideBaseUrl(wearable: Wearable) {
  if (!TEST_WEARABLES_OVERRIDE) {
    return {
      ...wearable,
      baseUrl: getWearablesSafeURL() + '/contents/',
      baseUrlBundles: PIN_CATALYST ? '' : getServerConfigurations().contentAsBundle + '/'
    }
  } else {
    return wearable ?? {}
  }
}

function overrideSwankyRarity(wearable: Wearable) {
  if ((wearable.rarity as any) === 'swanky') {
    return {
      ...wearable,
      rarity: 'rare' as RarityEnum
    }
  }
  return wearable
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
      const catalog = collections!
        .reduce((flatten, collection) => flatten.concat(collection.wearables), [] as Wearable[])
        .map(overrideBaseUrl)
        // TODO - remove once all swankies are removed from service! - moliva - 22/05/2020
        .map(overrideSwankyRarity)
      const baseAvatars = catalog.filter((_: Wearable) => _.tags && !_.tags.includes('exclusive'))
      const baseExclusive = catalog.filter((_: Wearable) => _.tags && _.tags.includes('exclusive'))
      if (!(yield select(isInitialized))) {
        yield take(RENDERER_INITIALIZED)
      }
      yield put(addCatalog('base-avatars', baseAvatars))
      yield put(addCatalog('base-exclusive', baseExclusive))
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
    yield put(addCatalog('base-avatars', baseCatalog))
    yield put(addCatalog('base-exclusive', []))
  }
}

export function* handleAddCatalog(action: AddCatalogAction): any {
  // TODO (eordano, 16/Sep/2019): Validate correct schema
  if (!action.payload.catalog) {
    return
  }
  if (!(yield select(isInitialized))) {
    yield take(RENDERER_INITIALIZED)
  }
  yield call(sendWearablesCatalog, action.payload.catalog)
  yield put(catalogLoaded(action.payload.name))
}

async function headCatalog(url: string) {
  const request = await fetch(url, { method: 'HEAD' })
  if (!request.ok) {
    throw new Error('Catalog not found')
  }
  return request.headers.get('etag')
}

export async function fetchCatalog(url: string) {
  const request = await fetch(url)
  if (!request.ok) {
    throw new Error('Catalog not found')
  }
  const etag = request.headers.get('etag')
  return [await request.json(), etag]
}

export function sendWearablesCatalog(catalog: Catalog) {
  globalThis.unityInterface.AddWearablesToCatalog(catalog)
}

export function* ensureBaseCatalogs() {
  while (!(yield select(baseCatalogsLoaded))) {
    yield take(CATALOG_LOADED)
  }
}
