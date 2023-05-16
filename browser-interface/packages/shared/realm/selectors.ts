import { FETCH_CONTENT_SERVICE, UPDATE_CONTENT_SERVICE } from 'config'
import { realmToConnectionString, urlWithProtocol } from './resolver'
import type { IRealmAdapter, RootRealmState } from './types'
import { OFFLINE_REALM, OnboardingState } from './types'

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
  const url = adapter.services.updateContentServer
  return urlWithProtocol(url)
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
  return adapter.services.fetchContentServer
}

/**
 * Returns the base URL to resolve all assets by CID in the configured server
 */
export const getFetchContentUrlPrefixFromRealmAdapter = (adapter: IRealmAdapter) => {
  return getFetchContentServerFromRealmAdapter(adapter) + '/contents/'
}

export const getOnboardingState = (state: RootRealmState): OnboardingState => state.onboarding
