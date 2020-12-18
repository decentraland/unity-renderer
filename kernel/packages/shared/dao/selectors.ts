import { RootDaoState } from './types'
import { getServerConfigurations } from 'config'

export const getFetchProfileServer = (store: RootDaoState) => store.dao.profileServer
export const getUpdateProfileServer = (store: RootDaoState) => store.dao.updateContentServer

export const getFetchContentServer = (store: RootDaoState) => store.dao.fetchContentServer
export const getFetchMetaContentServer = (store: RootDaoState) => store.dao.fetchMetaContentServer
export const getFetchMetaContentService = (store: RootDaoState) =>
  store.dao.fetchMetaContentServer + '/lambdas/contentv2'
export const getResizeService = (store: RootDaoState) => store.dao.resizeService

export const getCommsServer = (store: RootDaoState) => store.dao.commsServer

export const getRealm = (store: RootDaoState) => store.dao.realm

export const getLayer = (store: RootDaoState) => (store.dao.realm ? store.dao.realm.layer : '')

export const getCatalystCandidates = (store: RootDaoState) => store.dao.candidates
export const getAddedCatalystCandidates = (store: RootDaoState) => store.dao.addedCandidates

export const getAllCatalystCandidates = (store: RootDaoState) =>
  getAddedCatalystCandidates(store)
    .concat(getCatalystCandidates(store))
    .filter((it) => !!it)

export const isRealmInitialized = (store: RootDaoState) => store.dao.initialized
export const areCandidatesFetched = (store: RootDaoState) => store.dao.candidatesFetched

export const getCatalystRealmCommsStatus = (store: RootDaoState) => store.dao.commsStatus

export const isResizeServiceUrl = (store: RootDaoState, url: string | undefined) =>
  url?.startsWith(getResizeService(store)) || url?.startsWith(getServerConfigurations().fallbackResizeServiceUrl)

export const getHotScenesService = (store: RootDaoState) => store.dao.hotScenesService
export const getExploreRealmsService = (store: RootDaoState) => store.dao.exploreRealmsService
