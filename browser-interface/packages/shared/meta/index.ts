import { storeCondition } from 'lib/redux'
import { isMetaConfigurationInitialized } from './selectors'

export async function ensureMetaConfigurationInitialized(): Promise<void> {
  await storeCondition(isMetaConfigurationInitialized)
}

export const DEFAULT_MAX_VISIBLE_PEERS = 1000
