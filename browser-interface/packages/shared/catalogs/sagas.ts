import { apply, call, put, select, takeEvery } from 'redux-saga/effects'

import {
  WITH_FIXED_COLLECTIONS,
  getAssetBundlesBaseUrl,
  getTLD,
  PREVIEW,
  DEBUG,
  ETHEREUM_NETWORK,
  BUILDER_SERVER_URL,
  rootURLPreviewMode,
  WITH_FIXED_ITEMS
} from 'config'

import { authorizeBuilderHeaders } from 'lib/decentraland/authentication/authorizeBuilderHeaders'
import defaultLogger from 'lib/logger'
import {
  EmotesRequest,
  emotesFailure,
  EmotesFailure,
  emotesSuccess,
  EmotesSuccess,
  EMOTES_FAILURE,
  EMOTES_REQUEST,
  EMOTES_SUCCESS,
  WearablesFailure,
  wearablesFailure,
  WearablesRequest,
  WearablesSuccess,
  wearablesSuccess,
  WEARABLES_FAILURE,
  WEARABLES_REQUEST,
  WEARABLES_SUCCESS
} from './actions'
import {
  WearablesRequestFilters,
  WearableV2,
  BodyShapeRepresentationV2,
  PartialWearableV2,
  UnpublishedWearable,
  Emote,
  EmotesRequestFilters,
  PartialItem,
  UnpublishedWearableType,
  areWearablesRequestFilters,
  isPartialWearable
} from './types'
import { waitForRendererInstance } from 'shared/renderer/sagas-helper'
import { CatalystClient } from 'dcl-catalyst-client/dist/CatalystClient'
import { getSelectedNetwork } from 'shared/dao/selectors'
import { getCurrentIdentity } from 'shared/session/selectors'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import { ExplorerIdentity } from 'shared/session/types'
import { trackEvent } from 'shared/analytics/trackEvent'
import { IRealmAdapter } from 'shared/realm/types'
import { getFetchContentServerFromRealmAdapter, getFetchContentUrlPrefixFromRealmAdapter } from 'shared/realm/selectors'
import { ErrorContext, BringDownClientAndReportFatalError } from 'shared/loading/ReportFatalError'
import { OwnedItemsWithDefinition } from 'dcl-catalyst-client/dist/LambdasAPI'
import { waitForRealm } from 'shared/realm/waitForRealmAdapter'

const BASE_AVATARS_COLLECTION_ID = 'urn:decentraland:off-chain:base-avatars'
const WRONG_FILTERS_ERROR = `You must set one and only one filter for V1. Also, the only collection id allowed is '${BASE_AVATARS_COLLECTION_ID}'`

const BASE_BUILDER_DOWNLOAD_URL = `${BUILDER_SERVER_URL}/storage/contents`

/**
 * This saga handles wearable definition fetching.
 *
 * When the renderer detects a new wearable, but it doesn't know its definition, then it will create a catalog request.
 *
 * This request will include the ids of the unknown wearables. We will then find the appropriate definition, and return it to the renderer.
 *
 */
export function* catalogsSaga(): any {
  yield takeEvery([WEARABLES_REQUEST, EMOTES_REQUEST], handleItemRequest)
  yield takeEvery([WEARABLES_SUCCESS, EMOTES_SUCCESS], handleItemsRequestSuccess)
  yield takeEvery([WEARABLES_FAILURE, EMOTES_FAILURE], handleItemsRequestFailure)
}

function* handleItemRequest(action: EmotesRequest | WearablesRequest) {
  const { filters, context } = action.payload

  const valid = areFiltersValid(filters)
  const isRequestingEmotes = action.type === EMOTES_REQUEST
  const failureAction = isRequestingEmotes ? emotesFailure : wearablesFailure
  if (valid) {
    const realmAdapter: IRealmAdapter = yield call(waitForRealm)
    const contentBaseUrl: string = getFetchContentUrlPrefixFromRealmAdapter(realmAdapter)

    try {
      const response: PartialItem[] = yield call(fetchItemsFromCatalyst, action, filters)
      const net: ETHEREUM_NETWORK = yield select(getSelectedNetwork)
      const assetBundlesBaseUrl: string = getAssetBundlesBaseUrl(net) + '/'

      const v2Items: (WearableV2 | Emote)[] = response.map((item) => ({
        ...item,
        baseUrl: item.baseUrl ?? contentBaseUrl,
        baseUrlBundles: assetBundlesBaseUrl
      }))

      yield put(
        isRequestingEmotes
          ? emotesSuccess(v2Items as Emote[], context)
          : wearablesSuccess(v2Items as WearableV2[], context)
      )
    } catch (error: any) {
      yield put(failureAction(context, error.message))
    }
  } else {
    yield put(failureAction(context, WRONG_FILTERS_ERROR))
  }
}

function* fetchItemsFromCatalyst(
  action: EmotesRequest | WearablesRequest,
  filters: EmotesRequestFilters | WearablesRequestFilters
) {
  const realmAdapter: IRealmAdapter = yield call(waitForRealm)
  const contentBaseUrl: string = getFetchContentServerFromRealmAdapter(realmAdapter)
  // TODO: stop using CatalystClient and move endpoints to BFF
  const catalystUrl: string = contentBaseUrl.replace(/\/content\/?.*$/, '')
  const identity: ExplorerIdentity = yield select(getCurrentIdentity)
  const client: CatalystClient = new CatalystClient({ catalystUrl })
  const network: ETHEREUM_NETWORK = yield select(getSelectedNetwork)
  const COLLECTIONS_OR_ITEMS_ALLOWED =
    PREVIEW || ((DEBUG || getTLD() !== 'org') && network !== ETHEREUM_NETWORK.MAINNET)
  const isRequestingEmotes = action.type === EMOTES_REQUEST
  const catalystFetchFn = isRequestingEmotes ? client.fetchEmotes : client.fetchWearables
  const result: PartialItem[] = []
  if (filters.ownedByUser) {
    if (WITH_FIXED_ITEMS && COLLECTIONS_OR_ITEMS_ALLOWED) {
      const splittedItemIds = WITH_FIXED_ITEMS.split(',')
      const itemUuids = splittedItemIds.filter((id) => !id.startsWith('urn'))
      const itemURNs = splittedItemIds.filter((id) => id.startsWith('urn'))

      if (identity) {
        if (itemUuids.length > 0) {
          const v2Items: PartialItem[] = yield call(fetchItemsByIdFromBuilder, action, itemUuids, identity)
          result.push(...v2Items)
        }

        if (itemURNs.length > 0) {
          const zoneItems: PartialItem[] = isRequestingEmotes
            ? yield client.fetchEmotes({ emoteIds: itemURNs })
            : yield client.fetchWearables({ wearableIds: itemURNs })

          result.push(...zoneItems)
        }
      }
    } else if (WITH_FIXED_COLLECTIONS && COLLECTIONS_OR_ITEMS_ALLOWED) {
      // The WITH_FIXED_COLLECTIONS config can only be used in zone. However, we want to be able to use prod collections for testing.
      // That's why we are also querying a prod catalyst for the given collections
      const collectionIds: string[] = WITH_FIXED_COLLECTIONS.split(',')

      // Fetch published collections
      const urnCollections = collectionIds.filter((collectionId) => collectionId.startsWith('urn'))
      if (urnCollections.length > 0) {
        const zoneItems: PartialItem[] = yield apply(client, catalystFetchFn, [{ collectionIds: urnCollections }])
        result.push(...zoneItems)
      }

      // Fetch unpublished collections from builder server
      const uuidCollections = collectionIds.filter((collectionId) => !collectionId.startsWith('urn'))
      if (uuidCollections.length > 0 && identity) {
        const v2Items: PartialItem[] = yield call(
          fetchItemsByCollectionFromBuilder,
          action,
          uuidCollections,
          filters,
          identity
        )
        result.push(...v2Items)
      }
    } else {
      let ownedItems: OwnedItemsWithDefinition[]
      if (filters.thirdPartyId) {
        ownedItems = yield call(fetchOwnedThirdPartyWearables, filters.ownedByUser, filters.thirdPartyId, client)
      } else {
        ownedItems = yield call(
          isRequestingEmotes ? fetchOwnedEmotes : fetchOwnedWearables,
          filters.ownedByUser,
          client
        )
      }

      for (const { amount, definition } of ownedItems) {
        if (definition) {
          for (let i = 0; i < amount; i++) {
            result.push(definition)
          }
        }
      }
    }
  } else {
    const items: PartialItem[] = yield call(
      action.type === EMOTES_REQUEST ? fetchEmotesByFilters : fetchWearablesByFilters,
      filters,
      client
    )
    result.push(...items)

    if (WITH_FIXED_ITEMS && COLLECTIONS_OR_ITEMS_ALLOWED) {
      const splittedItemIds = WITH_FIXED_ITEMS.split(',')
      const itemUuids = splittedItemIds.filter((id) => !id.startsWith('urn'))
      const itemURNs = splittedItemIds.filter((id) => id.startsWith('urn'))

      if (identity) {
        if (itemUuids.length > 0) {
          const v2Items: PartialItem[] = yield call(fetchItemsByIdFromBuilder, action, itemUuids, identity)
          result.push(...v2Items)
        }

        if (itemURNs.length > 0) {
          const zoneItems: PartialItem[] = yield apply(client, catalystFetchFn, [{ wearableIds: itemURNs }])
          result.push(...zoneItems)
        }
      }
    } else if (WITH_FIXED_COLLECTIONS && COLLECTIONS_OR_ITEMS_ALLOWED) {
      const uuidCollections = WITH_FIXED_COLLECTIONS.split(',').filter(
        (collectionId) => !collectionId.startsWith('urn')
      )
      if (uuidCollections.length > 0 && identity) {
        const v2Items: PartialItem[] = yield call(
          fetchItemsByCollectionFromBuilder,
          action,
          uuidCollections,
          filters,
          identity
        )
        result.push(...v2Items)
      }
    }
  }

  if (PREVIEW) {
    const v2Wearables: PartialWearableV2[] = yield call(fetchWearablesByCollectionFromPreviewMode, filters)
    result.push(...v2Wearables)
  }

  return result
    .map((item) => {
      try {
        return mapCatalystItemIntoV2(item)
      } catch (err) {
        trackEvent('fetchWearablesFromCatalyst_failed', { wearableId: item.id })
        defaultLogger.log(`There was an error with item ${item.id}.`, err)
        return undefined
      }
    })
    .filter((item) => !!item)
}

function fetchOwnedThirdPartyWearables(ethAddress: string, thirdPartyId: string, client: CatalystClient) {
  return client.fetchOwnedThirdPartyWearables(ethAddress, thirdPartyId, true)
}

function fetchOwnedWearables(ethAddress: string, client: CatalystClient) {
  return client.fetchOwnedWearables(ethAddress, true)
}

function fetchOwnedEmotes(ethAddress: string, client: CatalystClient) {
  return client.fetchOwnedEmotes(ethAddress, true)
}

async function fetchWearablesByFilters(filters: WearablesRequestFilters, client: CatalystClient) {
  return client.fetchWearables(filters)
}

async function fetchEmotesByFilters(filters: EmotesRequestFilters, client: CatalystClient) {
  return client.fetchEmotes(filters)
}

/**
 * Fetches a single item from the Builder Server. Retrieves both wearables and emotes
 */
async function fetchItemsByIdFromBuilder(
  action: EmotesRequest | WearablesRequest,
  uuidsItems: string[],
  identity: ExplorerIdentity
): Promise<WearableV2[]> {
  return Promise.all(
    uuidsItems.map(async (uuid) => {
      const path = `items/${uuid}`
      const headers = authorizeBuilderHeaders(identity, 'get', `/${path}`)
      const itemRequest = await fetch(`${BUILDER_SERVER_URL}/${path}`, {
        headers
      })
      const itemResponse = (await itemRequest.json()) as { data: UnpublishedWearable; ok: boolean; error?: string }

      if (!itemResponse.ok) {
        const err = new Error('Cannot load items from Builder')
        BringDownClientAndReportFatalError(err, ErrorContext.KERNEL_SAGA)
        throw err
      }

      return mapUnpublishedItemIntoCatalystItem(action, itemResponse.data) as WearableV2
    })
  )
}

async function fetchItemsByCollectionFromBuilder(
  action: EmotesRequest | WearablesRequest,
  uuidCollections: string[],
  filters: WearablesRequestFilters | EmotesRequestFilters | undefined,
  identity: ExplorerIdentity
) {
  const isRequestingEmotes = action.type === EMOTES_REQUEST
  const result: PartialItem[] = []
  for (const collectionUuid of uuidCollections) {
    if (filters?.collectionIds && !filters.collectionIds.includes(collectionUuid)) {
      continue
    }

    const path = `collections/${collectionUuid}/items`
    const headers = authorizeBuilderHeaders(identity, 'get', `/${path}`)
    const collectionRequest = await fetch(`${BUILDER_SERVER_URL}/${path}`, {
      headers
    })
    const collection = (await collectionRequest.json()) as { data: UnpublishedWearable[] }
    const items = collection.data
      .filter((item) =>
        isRequestingEmotes
          ? item.type === UnpublishedWearableType.EMOTE
          : item.type === UnpublishedWearableType.WEARABLE
      )
      .map((item) => mapUnpublishedItemIntoCatalystItem(action, item))
    result.push(...items)
  }
  if (filters && areWearablesRequestFilters(filters) && filters.wearableIds) {
    return result.filter((item) => filters.wearableIds!.includes(item.id))
  } else if (filters && !areWearablesRequestFilters(filters) && filters.emoteIds) {
    return result.filter((item) => filters.emoteIds!.includes(item.id))
  }
  return result
}

async function fetchWearablesByCollectionFromPreviewMode(filters: WearablesRequestFilters | undefined) {
  const result: WearableV2[] = []
  try {
    const url = `${rootURLPreviewMode()}/preview-wearables`
    const collection: { data: any[] } = await (await fetch(url)).json()
    result.push(...collection.data)

    if (filters?.wearableIds) {
      return result.filter((w) => filters.wearableIds!.includes(w.id))
    }
  } catch (err) {
    defaultLogger.error(`Couldn't get the preview wearables. Check wearables folder.`, err)
  }
  return result
}

/**
 * We are now mapping wearables that were fetched from the builder server into the same format that is returned by the catalysts
 */
function mapUnpublishedItemIntoCatalystItem(action: EmotesRequest | WearablesRequest, item: UnpublishedWearable): any {
  const { id, rarity, name, thumbnail, description, data, contents: contentToHash } = item
  const baseItem = {
    id,
    rarity,
    i18n: [{ code: 'en', text: name }],
    thumbnail: `${BASE_BUILDER_DOWNLOAD_URL}/${contentToHash[thumbnail]}${DEBUG ? '?ts=' + Date.now() : ''}`,
    description
  }

  const representations = data.representations.map(({ contents, ...other }) => ({
    ...other,
    contents: contents.map((key) => ({
      key,
      url: `${BASE_BUILDER_DOWNLOAD_URL}/${contentToHash[key]}`
    }))
  }))

  if (action.type === WEARABLES_REQUEST) {
    return {
      ...baseItem,
      data: {
        ...data,
        representations
      }
    }
  } else {
    return {
      ...baseItem,
      emoteDataADR74: { ...data, representations }
    }
  }
}

function mapCatalystRepresentationIntoV2(representation: any): BodyShapeRepresentationV2 {
  const { contents, ...other } = representation

  const newContents = contents.map(({ key, url }: { key: string; url: string }) => ({
    key,
    hash: url.substring(url.lastIndexOf('/') + 1)
  }))
  return {
    ...other,
    contents: newContents
  }
}

function mapCatalystItemIntoV2(v2Item: PartialItem): PartialItem {
  const { id, rarity, i18n, thumbnail, description } = v2Item
  let data
  if (isPartialWearable(v2Item)) {
    data = v2Item.data
  } else {
    data = v2Item.emoteDataADR74
  }
  const { representations } = data
  const newRepresentations: BodyShapeRepresentationV2[] = representations.map(mapCatalystRepresentationIntoV2)
  const index = thumbnail.lastIndexOf('/')
  const newThumbnail = thumbnail.substring(index + 1)
  const baseUrl = thumbnail.substring(0, index + 1)

  return {
    id,
    rarity,
    i18n,
    thumbnail: newThumbnail,
    description,
    data: {
      ...data,
      representations: newRepresentations
    },
    baseUrl
  }
}

function* handleItemsRequestSuccess(action: WearablesSuccess | EmotesSuccess) {
  const { context } = action.payload

  yield call(waitForRendererInstance)
  if (action.type === EMOTES_SUCCESS) {
    yield call(sendEmotesCatalog, action.payload.emotes, context)
  } else {
    yield call(sendWearablesCatalog, action.payload.wearables, context)
  }
}

function* handleItemsRequestFailure(action: WearablesFailure | EmotesFailure) {
  const { context, error } = action.payload

  defaultLogger.error(
    `Failed to fetch ${action.type === WEARABLES_FAILURE ? 'wearables' : 'emotes'} for context '${context}'`,
    error
  )

  yield call(waitForRendererInstance)
  yield call(informRequestFailure, error, context)
}

function areFiltersValid(filters: WearablesRequestFilters | EmotesRequestFilters) {
  let filtersSet = 0
  let ok = true
  if (filters.collectionIds) {
    filtersSet += 1
    if (filters.collectionIds.some((id) => id !== BASE_AVATARS_COLLECTION_ID)) {
      ok = false
    }
  }

  if (filters.ownedByUser) {
    filtersSet += 1
  }

  if (areWearablesRequestFilters(filters) && filters.wearableIds) {
    filtersSet += 1
  }

  if (!areWearablesRequestFilters(filters) && filters.emoteIds) {
    filtersSet += 1
  }

  if (filters.thirdPartyId && !filters.ownedByUser) {
    ok = false
  }

  return filtersSet === 1 && ok
}

function informRequestFailure(error: string, context: string | undefined) {
  getUnityInstance().WearablesRequestFailed(error, context)
}

function sendWearablesCatalog(wearables: WearableV2[], context: string | undefined) {
  getUnityInstance().AddWearablesToCatalog(wearables, context)
}

function sendEmotesCatalog(emotes: Emote[], context: string | undefined) {
  getUnityInstance().AddEmotesToCatalog(emotes, context)
}
