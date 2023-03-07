import { RootCatalystSelectionState as RootCatalystSelectionState } from './types'
import { getFeatureFlagEnabled } from 'shared/meta/selectors'
import { RootMetaState } from 'shared/meta/types'

export const getCatalystCandidates = (store: RootCatalystSelectionState) => store.catalystSelection.candidates
export const getCatalystCandidatesReceived = (store: RootCatalystSelectionState) =>
  store.catalystSelection.catalystCandidatesReceived

export const getAllCatalystCandidates = (store: RootCatalystSelectionState & RootMetaState) => {
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

export const getSelectedNetwork = (store: RootCatalystSelectionState) => {
  if (store.catalystSelection.network) {
    return store.catalystSelection.network
  }
  throw new Error('Missing network')
}
