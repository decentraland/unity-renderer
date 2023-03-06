import type { BannedUsers, FeatureFlag, FeatureFlagsName, RootMetaState, WorldConfig } from './types'
import { AlgorithmChainConfig } from 'shared/dao/pick-realm-algorithm/types'
import { BYPASS_CONTENT_ALLOWLIST, SOCIAL_SERVER_URL } from 'config'
import { urlWithProtocol } from 'shared/realm/resolver'
import { DEFAULT_MAX_VISIBLE_PEERS } from '.'
import { QS_MAX_VISIBLE_PEERS } from 'config'
import { trackEvent } from 'shared/analytics/trackEvent'

export const getAddedServers = (store: RootMetaState): string[] => {
  const { config } = store.meta

  if (!config || !config.servers || !config.servers.added) {
    return []
  }

  return config.servers.added
}

export const getContentWhitelist = (store: RootMetaState): string[] => {
  const { config } = store.meta

  if (!config || !config.servers || !config.servers.contentWhitelist) {
    return []
  }

  return config.servers.contentWhitelist
}

export const getMinCatalystVersion = (store: RootMetaState): string | undefined => {
  const { config } = store.meta

  return config.minCatalystVersion
}

export const isMetaConfigurationInitialized = (store: RootMetaState): boolean => store.meta.initialized

export const getWorldConfig = (store: RootMetaState): WorldConfig => store.meta.config.world as WorldConfig

export const getBannedUsers = (store: RootMetaState): BannedUsers =>
  (getFeatureFlagVariantValue(store, 'banned_users') as BannedUsers) ?? {}

export const getPickRealmsAlgorithmConfig = (store: RootMetaState): AlgorithmChainConfig | undefined =>
  getFeatureFlagVariantValue(store, 'pick_realm_algorithm_config') as AlgorithmChainConfig | undefined

export const getDisabledCatalystConfig = (store: RootMetaState): string[] => {
  const disabledCatalysts = getFeatureFlagVariantValue<string[]>(store, 'disabled-catalyst') || []
  return disabledCatalysts.filter((catalyst) => {
    try {
      return !!new URL(catalyst)
    } catch (e) {
      trackEvent('invalid_denied_catalyst_url', { url: catalyst })
      return false
    }
  })
}

export const isLiveKitVoiceChatFeatureFlag = (store: RootMetaState): boolean =>
  getFeatureFlagEnabled(store, 'livekit-voicechat') as boolean

// Enable the gif processor on the web instead of processing it in Unity (just for WebGL build)
export const isGifWebSupported = (store: RootMetaState): boolean => getFeatureFlagEnabled(store, 'gif-web') as boolean

export function getMaxVisiblePeers(store: RootMetaState): number {
  return (
    QS_MAX_VISIBLE_PEERS ||
    +(getFeatureFlagVariantValue(store, 'max_visible_peers') as string) ||
    DEFAULT_MAX_VISIBLE_PEERS
  )
}

/**
 * Returns the variant content of a feature flag
 */
export function getFeatureFlagVariantValue<T = unknown>(
  store: RootMetaState,
  featureName: FeatureFlagsName
): T | undefined {
  const ff = getFeatureFlags(store)
  const variant = ff.variants[featureName]?.payload
  if (variant) {
    try {
      if (variant.type === 'json') return JSON.parse(variant.value) as any as T
      if (variant.type === 'csv') return (variant.value ?? '').split(',') as any as T
      return variant.value as any as T
    } catch (e) {
      console.warn(`Couldn't parse value for ${featureName} from variants.`)
      return undefined
    }
  }
  return undefined
}

/**
 * Returns the feature flag value
 */
export function getFeatureFlagEnabled(store: RootMetaState, featureName: FeatureFlagsName): boolean {
  const ff = getFeatureFlags(store)
  if (ff.flags[featureName]) {
    return ff.flags[featureName] || false
  }
  return false
}

/**
 * Returns the feature flag variant name
 */
export function getFeatureFlagVariantName(store: RootMetaState, featureName: FeatureFlagsName): string {
  const ff = getFeatureFlags(store)
  if (getFeatureFlagEnabled(store, featureName)) {
    const variant = ff.variants[featureName]
    if (variant && variant.enabled) {
      return variant.name
    }
    return 'undefined'
  }
  return 'undefined'
}

export function getFeatureFlags(store: RootMetaState): FeatureFlag {
  return store.meta.config.featureFlagsV2 || { flags: {}, variants: {} }
}

export const getSynapseUrl = (store: RootMetaState): string => {
  if (getFeatureFlagEnabled(store, 'use-synapse-server')) {
    return store.meta.config.synapseUrl ?? 'https://synapse.decentraland.zone'
  }

  const defaultSocialServerUrl = store.meta.config.socialServerUrl ?? 'https://social.decentraland.zone'

  return SOCIAL_SERVER_URL ?? defaultSocialServerUrl
}

export const getCatalystNodesEndpoint = (store: RootMetaState): string | undefined =>
  store.meta.config.servers?.catalystsNodesEndpoint

/**
 * Filters out content server hostnames from the allowed list of the MetaState.
 * This is necessary to protect the IP and domains in which DCL is served.
 */
export function getAllowedContentServer(meta: RootMetaState, givenServer: string): string {
  // if a catalyst is pinned => avoid any override
  if (BYPASS_CONTENT_ALLOWLIST) {
    return givenServer
  }

  const contentWhitelist = getContentWhitelist(meta)

  // if current realm is in whitelist => return current state
  if (givenServer && contentWhitelist.some((allowedCandidate) => allowedCandidate === givenServer)) {
    return urlWithProtocol(givenServer)
  }

  if (contentWhitelist.length) {
    return urlWithProtocol(contentWhitelist[0] + '/content')
  }

  return urlWithProtocol(givenServer)
}
