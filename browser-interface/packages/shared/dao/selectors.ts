import { Candidate, RootDaoState } from './types'
import { HOTSCENES_SERVICE, POI_SERVICE } from 'config'
import { urlWithProtocol } from 'shared/realm/resolver'
import { RootRealmState } from 'shared/realm/types'

export const getCatalystCandidates = (store: RootDaoState): Candidate[] => [...store.dao.candidates]
export const getCatalystCandidatesReceived = (store: RootDaoState) => store.dao.catalystCandidatesReceived

export const getLastConnectedCandidates = (store: RootDaoState) => store.dao.lastConnectedCandidates

export const getHotScenesService = (state: RootRealmState) => {
  if (HOTSCENES_SERVICE) {
    return HOTSCENES_SERVICE
  }
  return urlWithProtocol(state.realm.realmAdapter!.services.hotScenesService)
}

export const getContentService = (state: RootRealmState) => {
  return urlWithProtocol(state.realm.realmAdapter!.services.fetchContentServer)
}

export const getExploreRealmsService = (state: RootRealmState) =>
  state.realm.realmAdapter!.services.exploreRealmsService
export const getPOIService = (state: RootRealmState) => {
  if (POI_SERVICE) {
    return POI_SERVICE
  }
  return urlWithProtocol(state.realm.realmAdapter!.services.poiService)
}

export const getSelectedNetwork = (store: RootDaoState) => {
  if (store.dao.network) {
    return store.dao.network
  }
  throw new Error('Missing network')
}
