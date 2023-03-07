import { ETHEREUM_NETWORK } from 'config'
import { action } from 'typesafe-actions'

export const SELECT_NETWORK = '[CatalystSelection] Web3 Network Selected'
export const selectNetwork = (network: ETHEREUM_NETWORK) => action(SELECT_NETWORK, network)
export type SelectNetworkAction = ReturnType<typeof selectNetwork>
