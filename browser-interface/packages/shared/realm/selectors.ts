import { FETCH_CONTENT_SERVICE, UPDATE_CONTENT_SERVICE } from 'config'
import { select, take } from 'redux-saga/effects'
import { store } from 'shared/store/isolatedStore'
import { SET_REALM_ADAPTER } from './actions'
import { realmToConnectionString, urlWithProtocol } from './resolver'
import { IRealmAdapter, RootRealmState, OFFLINE_REALM } from './types'

export const getRealmAdapter = (state: RootRealmState): IRealmAdapter | undefined => state.realm.realmAdapter
export const getRealmConnectionString = (state: RootRealmState): string =>
  state.realm.realmAdapter ? realmToConnectionString(state.realm.realmAdapter) : OFFLINE_REALM

export function* waitForRealmAdapter() {
  while (true) {
    const realmAdapter: IRealmAdapter | undefined = yield select(getRealmAdapter)
    if (realmAdapter) return realmAdapter
    yield take(SET_REALM_ADAPTER)
  }
}

export async function ensureRealmAdapterPromise(): Promise<IRealmAdapter> {
  const bff = getRealmAdapter(store.getState())
  if (bff) {
    return bff
  }

  return new Promise((resolve) => {
    const unsubscribe = store.subscribe(() => {
      const bff = getRealmAdapter(store.getState())
      if (bff) {
        unsubscribe()
        return resolve(bff)
      }
    })
  })
}

export const getProfilesContentServerFromRealmAdapter = (adapter: IRealmAdapter) => {
  if (UPDATE_CONTENT_SERVICE) {
    return urlWithProtocol(UPDATE_CONTENT_SERVICE)
  }
  const url = adapter.services.legacy.updateContentServer
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
  return adapter.services.legacy.fetchContentServer
}

/**
 * Returns the base URL to resolve all assets by CID in the configured server
 */
export const getFetchContentUrlPrefixFromRealmAdapter = (adapter: IRealmAdapter) => {
  return getFetchContentServerFromRealmAdapter(adapter) + '/contents/'
}
