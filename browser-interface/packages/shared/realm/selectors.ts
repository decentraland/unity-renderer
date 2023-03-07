import { FETCH_CONTENT_SERVICE, HOTSCENES_SERVICE, POI_SERVICE, UPDATE_CONTENT_SERVICE } from 'config'
import { realmToConnectionString, urlWithProtocol } from './resolver'
import type { IRealmAdapter, RootRealmState } from './types'
import { OFFLINE_REALM } from './types'

export const isWorldLoaderActive = (realmAdapter: IRealmAdapter) =>
  !!realmAdapter?.about.configurations?.scenesUrn?.length ||
  realmAdapter?.about.configurations?.cityLoaderContentServer === ''

export function isPreviousAdapterWorld(state: RootRealmState) {
  return state.realm.previousAdapter && isWorldLoaderActive(state.realm.previousAdapter)
}

export const getRealmAdapter = (state: RootRealmState): IRealmAdapter | undefined => state.realm.realmAdapter
export const getRealmConnectionString = (state: RootRealmState): string =>
  state.realm.realmAdapter ? realmToConnectionString(state.realm.realmAdapter) : OFFLINE_REALM

export const getProfilesContentServerFromRealmAdapter = (adapter: IRealmAdapter) => {
  if (UPDATE_CONTENT_SERVICE) {
    return urlWithProtocol(UPDATE_CONTENT_SERVICE)
  }
  const url = adapter.services.legacy.updateContentServer
  return urlWithProtocol(url)
}

export const getHotScenesService = (state: RootRealmState) => {
  if (HOTSCENES_SERVICE) {
    return HOTSCENES_SERVICE
  }
  return urlWithProtocol(state.realm.realmAdapter!.services.legacy.hotScenesService)
}

export const getExploreRealmsService = (state: RootRealmState) =>
  state.realm.realmAdapter!.services.legacy.exploreRealmsService
export const getPOIService = (state: RootRealmState) => {
  if (POI_SERVICE) {
    return POI_SERVICE
  }
  return urlWithProtocol(state.realm.realmAdapter!.services.legacy.poiService)
}

/**
 * Returns the fetch content server configured by this BFF.
 *
 * If it is overwritten by url params then that value is returned.
 */
export const getFetchContentServerFromRealmAdapter = (adapter: IRealmAdapter) => {
  if (FETCH_CONTENT_SERVICE) {
    return urlWithProtocol(FETCH_CONTENT_SERVICE)
  }
  return adapter.services.legacy.fetchContentServer
}

/**
 * Returns the base URL to resolve all assets by CID in the configured server
 */
export const getFetchContentUrlPrefixFromRealmAdapter = (adapter: IRealmAdapter) => {
  return getFetchContentServerFromRealmAdapter(adapter) + '/contents/'
}
