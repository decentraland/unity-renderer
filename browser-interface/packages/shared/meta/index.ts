import { storeCondition } from 'lib/redux'
import { isMetaConfigurationInitialized } from './selectors'

/**
 * MetaConfiguration is the combination of three main aspects of the environment in which we are running:
 * - which Ethereum network are we connected to
 * - what is the current global explorer configuration from https://config.decentraland.${tld}/explorer.json
 * - what feature flags are currently enabled
 */
export async function ensureMetaConfigurationInitialized(): Promise<void> {
  await storeCondition(isMetaConfigurationInitialized)
}

export const DEFAULT_MAX_VISIBLE_PEERS = 1000
