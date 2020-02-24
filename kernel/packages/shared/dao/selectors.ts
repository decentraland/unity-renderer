import { RootDaoState } from './types'

export const getFetchProfileServer = (store: RootDaoState) => store.dao.profileServer
export const getUpdateProfileServer = (store: RootDaoState) => store.dao.updateContentServer

export const getFetchContentServer = (store: RootDaoState) => store.dao.fetchContentServer
export const getFetchMetaContentServer = (store: RootDaoState) => store.dao.fetchMetaContentServer

export const getCommsServer = (store: RootDaoState) => store.dao.commsServer

export const getRealm = (store: RootDaoState) => store.dao.realm

export const getLayer = (store: RootDaoState) => (store.dao.realm ? store.dao.realm.layer : '')

export const getCatalystCandidates = (store: RootDaoState) => store.dao.candidates
export const getAddedCatalystCandidates = (store: RootDaoState) => store.dao.addedCandidates

export const getAllCatalystCandidates = (store: RootDaoState) => getAddedCatalystCandidates(store).concat(getCatalystCandidates(store))

export const isRealmInitialized = (store: RootDaoState) => store.dao.initialized

export const getCatalystRealmCommsStatus = (store: RootDaoState) => store.dao.commsStatus
