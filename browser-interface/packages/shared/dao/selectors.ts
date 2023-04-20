import { RootDaoState } from './types'
import { HOTSCENES_SERVICE, POI_SERVICE } from 'config'
import { urlWithProtocol } from 'shared/realm/resolver'
import { RootRealmState } from 'shared/realm/types'
import { getFeatureFlagEnabled } from 'shared/meta/selectors'
import { RootMetaState } from 'shared/meta/types'

export const getCatalystCandidates = (store: RootDaoState) => store.dao.candidates
export const getCatalystCandidatesReceived = (store: RootDaoState) => store.dao.catalystCandidatesReceived

export const getAllCatalystCandidates = (store: RootDaoState & RootMetaState) => {
  const candidates = getCatalystCandidates(store).filter((it) => !!it)
  const enableLegacyComms: boolean = getFeatureFlagEnabled(store, 'enable_legacy_comms_v2')

  return candidates.filter(($) => {
    if ($.lastConnectionAttempt && Date.now() - $.lastConnectionAttempt < 60 * 1000) {
      return false
    }
    if ($.protocol === 'v3') return true
    return enableLegacyComms
  })
}

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
