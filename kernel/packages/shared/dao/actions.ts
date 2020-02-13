import { action } from 'typesafe-actions'
import { Realm, Candidate, CommsStatus } from './types'

export const WEB3_INITIALIZED = 'Web3 initialized'

export const UPDATE_CATALYST_REALM = 'Update Catalyst Realm'

export const web3initialized = () => action(WEB3_INITIALIZED)
export type Web3Initialized = ReturnType<typeof web3initialized>

export const SET_CATALYST_CANDIDATES = 'Set Catalyst Candidates'
export const setCatalystCandidates = (candidates: Candidate[]) => action(SET_CATALYST_CANDIDATES, candidates)
export type SetCatalystCandidates = ReturnType<typeof setCatalystCandidates>

export const INIT_CATALYST_REALM = 'Init Catalyst realm'
export const initCatalystRealm = (realm: Realm) => action(INIT_CATALYST_REALM, realm)
export type initCatalystRealm = ReturnType<typeof initCatalystRealm>

export const SET_CATALYST_REALM = 'Set Catalyst realm'
export const setCatalystRealm = (realm: Realm) => action(SET_CATALYST_REALM, realm)
export type SetCatalystRealm = ReturnType<typeof setCatalystRealm>

export const CATALYST_REALM_INITIALIZED = 'Catalyst realm initialized'
export const catalystRealmInitialized = () => action(CATALYST_REALM_INITIALIZED)
export type CatalystNodeInitialized = ReturnType<typeof catalystRealmInitialized>

export const SET_CATALYST_REALM_COMMS_STATUS = 'Set Catalyst Realm Comms Status'
export const setCatalystRealmCommsStatus = (status: CommsStatus) => action(SET_CATALYST_REALM_COMMS_STATUS, status)
export type SetCatalystRealmCommsStatus = ReturnType<typeof setCatalystRealmCommsStatus>
