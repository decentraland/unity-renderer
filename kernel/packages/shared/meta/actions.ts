import { MetaConfiguration } from './types'
import { action } from 'typesafe-actions'

export const META_CONFIGURATION_INITIALIZED = 'Meta Configuration Initialized'
export const metaConfigurationInitialized = (config: Partial<MetaConfiguration>) =>
  action(META_CONFIGURATION_INITIALIZED, config)
export type MetaConfigurationInitialized = ReturnType<typeof metaConfigurationInitialized>
export const META_UPDATE_MESSAGE_OF_THE_DAY = '[UPDATE] update message of the day'
export const metaUpdateMessageOfTheDay = (values: any) => action(META_UPDATE_MESSAGE_OF_THE_DAY, values)
