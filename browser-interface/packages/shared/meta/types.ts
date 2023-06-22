import { RenderProfile } from 'shared/types'
import { FeatureFlagVariant } from '@dcl/feature-flags'

export type MetaConfiguration = {
  explorer: {
    minBuildNumber: number
    assetBundlesFetchUrl: string
  }
  servers: {
    added: string[]
    denied: string[]
    contentWhitelist: string[]
    catalystsNodesEndpoint?: string
  }
  synapseUrl: string
  socialServerUrl: string
  world: WorldConfig
  minCatalystVersion?: string
  featureFlagsV2?: FeatureFlag
  bannedUsers?: BannedUsers
}

export type FeatureFlagsName =
  | 'quests' // quests feature
  | 'retry_matrix_login' // retry matrix reconnection
  | 'parcel-denylist' // denylist of specific parcels using variants
  | 'matrix_disabled' // disable matrix integration entirely
  | 'matrix_presence_disabled' // disable matrix presence feature
  | 'matrix_channels_enabled' // enables matrix channels feature
  | 'max_joined_channels' // the max amount of joined channels allowed per user
  | 'users_allowed_to_create_channels' // users who are allowed to create channels
  | 'new_friend_requests' // enables the new friends request flow
  | 'friend_request_anti_spam_config' // json with anti-spam config values
  | 'avatar_lods'
  | 'asset_bundles'
  | 'explorev2'
  | 'unsafe-request'
  | 'pick_realm_algorithm_config'
  | 'banned_users'
  | 'max_visible_peers'
  | 'initial_portable_experiences'
  | 'web_cap_fps' // caps the web client FPS
  | 'disabled-catalyst'
  | 'livekit-voicechat'
  | 'gif-web'
  | 'ping_enabled'
  | 'use-synapse-server'
  | 'use-social-server-friendships' // get friendships from social service v1 API
  | 'new_tutorial_variant'
  | 'enable_legacy_comms_v2'
  | 'ab-new-cdn' // enables the new CDN for asset bundles along with the new loader
  | 'decoupled_loading_screen'
  | 'seamless_login_variant'
  | 'my_account'


export type BannedUsers = Record<string, Ban[]>

export type Ban = {
  type: 'VOICE_CHAT_AND_CHAT' // For now we only handle one ban type
  expiration: number // Timestamp
}

export interface POI {
  x: number
  y: number
}

export type WorldConfig = {
  renderProfile?: RenderProfile
  enableNewTutorialCamera?: boolean
  pois?: Array<POI>
}

export type MetaState = {
  initialized: boolean
  config: Partial<MetaConfiguration>
}

export type RootMetaState = {
  meta: MetaState
}

export type FeatureFlag = {
  flags: Partial<Record<FeatureFlagsName, boolean>>
  variants: Partial<Record<FeatureFlagsName, FeatureFlagVariant>>
}
