import { ETHEREUM_NETWORK } from 'config'
import { action } from 'typesafe-actions'
import { Candidate } from './types'

export const SET_CATALYST_CANDIDATES = 'Set Catalyst Candidates'
export const setCatalystCandidates = (candidates: Candidate[]) => action(SET_CATALYST_CANDIDATES, candidates)
export type SetCatalystCandidates = ReturnType<typeof setCatalystCandidates>

export const SELECT_NETWORK = '[DAO] Select network'
export const selectNetwork = (network: ETHEREUM_NETWORK) => action(SELECT_NETWORK, network)
export type SelectNetworkAction = ReturnType<typeof selectNetwork>

const CATALYST_REALMS_SCAN_REQUESTED = '[Request] Catalyst Realms scan'
export const catalystRealmsScanRequested = () => action(CATALYST_REALMS_SCAN_REQUESTED)
