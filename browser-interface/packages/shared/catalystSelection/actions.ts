import { action } from 'typesafe-actions'
import { Candidate, Realm } from './types'

export const SET_CATALYST_CANDIDATES = '[CatalystSelection] Set Candidates'
export const setCatalystCandidates = (candidates: Candidate[]) => action(SET_CATALYST_CANDIDATES, candidates)
export type SetCatalystCandidates = ReturnType<typeof setCatalystCandidates>

export const CATALYST_REALMS_SCAN_REQUESTED = '[CatalystSelection] Catalyst Scan Requested'
export const catalystRealmsScanRequested = () => action(CATALYST_REALMS_SCAN_REQUESTED)
export type CatalystRealmsScanRequested = ReturnType<typeof catalystRealmsScanRequested>

export const CATALYST_REALMS_SCAN_FINISHED = '[CatalystSelection] Catalyst Scan Completed'
export const catalystRealmsScanFinished = (catalyst: Realm) => action(CATALYST_REALMS_SCAN_FINISHED, catalyst)
export type CatalystRealmsScanFinished = ReturnType<typeof catalystRealmsScanFinished>
