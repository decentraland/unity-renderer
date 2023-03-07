import { MetaConfiguration } from './types'
import { action } from 'typesafe-actions'

export const META_CONFIGURATION_INITIALIZED = 'Meta Configuration Initialized'
export const metaConfigurationInitialized = (config: Partial<MetaConfiguration>) =>
  action(META_CONFIGURATION_INITIALIZED, config)
export type MetaConfigurationInitialized = ReturnType<typeof metaConfigurationInitialized>

export const BEFORE_UNLOAD = 'Meta: Before Unload'
export const beforeUnloadAction = () => action(BEFORE_UNLOAD)
