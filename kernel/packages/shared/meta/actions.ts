import { MetaConfiguration } from './sagas'
import { action } from 'typesafe-actions'

export const META_CONFIGURATION_INITIALIZED = 'Meta Configuration Initialized'
export const metaConfigurationInitialized = (config: MetaConfiguration) =>
  action(META_CONFIGURATION_INITIALIZED, config)
export type MetaConfigurationInitialized = ReturnType<typeof metaConfigurationInitialized>
