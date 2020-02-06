import { action } from 'typesafe-actions'
import { Realm } from './types'

export const WEB3_INITIALIZED = 'Web3 initialized'

export const web3initialized = () => action(WEB3_INITIALIZED)
export type Web3Initialized = ReturnType<typeof web3initialized>

export const SET_CATALYST_REALM = 'Set Catalyst realm'
export const setCatalystRealm = (realm: Realm) => action(SET_CATALYST_REALM, realm)
export type SetCatalystRealm = ReturnType<typeof setCatalystRealm>

export const CATALYST_REALM_INITIALIZED = 'Catalyst realm initialized'
export const catalystRealmInitialized = () => action(CATALYST_REALM_INITIALIZED)
export type CatalystNodeInitialized = ReturnType<typeof catalystRealmInitialized>
