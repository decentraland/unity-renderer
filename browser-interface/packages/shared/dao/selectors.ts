import { RootDaoState } from './types'
import { HOTSCENES_SERVICE, POI_SERVICE } from 'config'
import { urlWithProtocol } from 'shared/realm/resolver'
import { RootRealmState } from 'shared/realm/types'

export const getCatalystCandidates = (store: RootDaoState) => store.dao.candidates
export const getCatalystCandidatesReceived = (store: RootDaoState) => store.dao.catalystCandidatesReceived

export const getAllCatalystCandidates = (store: RootDaoState) => getCatalystCandidates(store).filter((it) => !!it)

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

export const getSelectedNetwork = (store: RootDaoState) => {
  if (store.dao.network) {
    return store.dao.network
  }
  throw new Error('Missing network')
}
