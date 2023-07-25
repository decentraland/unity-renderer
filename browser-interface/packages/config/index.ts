import * as contractInfo from '@dcl/urn-resolver/dist/contracts'
import { getFeatureFlagEnabled } from 'shared/meta/selectors'
import { now } from 'lib/javascript/now'
import { store } from 'shared/store/isolatedStore'

/**
 * Estimated avatar height
 */
export const playerHeight = 1.6

// Entry points
export const PREVIEW: boolean = !!(globalThis as any).preview
export const WORLD_EXPLORER = !PREVIEW

export const RENDERER_WS = location.search.includes('ws')

// Development
const ENV_OVERRIDE = location.search.includes('ENV')
export const GIF_WORKERS = location.search.includes('GIF_WORKERS')

const qs = new URLSearchParams(location.search)

function ensureQueryStringUrl(value: string | null): string | null {
  if (!value) return null
  if (typeof value === 'string') return addHttpsIfNoProtocolIsSet(value)
  return addHttpsIfNoProtocolIsSet(value[0])
}
function ensureSingleString(value: string | string[] | null): string | null {
  if (!value) return null
  if (typeof value === 'string') return value
  return value[0]
}

// Comms
export const COMMS_PROFILE_TIMEOUT = 1500
export const FETCH_REMOTE_PROFILE_RETRIES = 3
export const MAXIMUM_NETWORK_MSG_LENGTH = 65000

export const UPDATE_CONTENT_SERVICE = ensureQueryStringUrl(qs.get('UPDATE_CONTENT_SERVICE'))
export const FETCH_CONTENT_SERVICE = ensureQueryStringUrl(qs.get('FETCH_CONTENT_SERVICE'))
export const HOTSCENES_SERVICE = ensureSingleString(qs.get('HOTSCENES_SERVICE'))
export const POI_SERVICE = ensureSingleString(qs.get('POI_SERVICE'))

export const TRACE_RENDERER = ensureSingleString(qs.get('TRACE_RENDERER'))

export const DEBUG = location.search.includes('DEBUG_MODE') || !!(globalThis as any).mocha || PREVIEW
export const DEBUG_ANALYTICS = location.search.includes('DEBUG_ANALYTICS')
export const DEBUG_REDUX = location.search.includes('DEBUG_REDUX')
export const DEBUG_REDUX_SAGAS = location.search.includes('DEBUG_REDUX_SAGAS')
export const DEBUG_SCENE_LOG = DEBUG || location.search.includes('DEBUG_SCENE_LOG')
export const DEBUG_KERNEL_LOG = !PREVIEW || location.search.includes('DEBUG_KERNEL_LOG')
export const DEBUG_WS_MESSAGES = location.search.includes('DEBUG_WS_MESSAGES')
export const DEBUG_VOICE_CHAT = location.search.includes('DEBUG_VOICE_CHAT')

export const PIPE_SCENE_CONSOLE = location.search.includes('PIPE_SCENE_CONSOLE')

export const RESET_TUTORIAL = location.search.includes('RESET_TUTORIAL')

export const CATALYSTS_FROM_DAO_CONTRACT = location.search.includes('CATALYSTS_FROM_DAO_CONTRACT')
export const ENGINE_DEBUG_PANEL = location.search.includes('ENGINE_DEBUG_PANEL')
export const SCENE_DEBUG_PANEL = location.search.includes('SCENE_DEBUG_PANEL') && !ENGINE_DEBUG_PANEL
export const SHOW_FPS_COUNTER = location.search.includes('SHOW_FPS_COUNTER') || location.search.includes('DEBUG_MODE')
export const HAS_INITIAL_POSITION_MARK = location.search.includes('position')
export const HAS_INITIAL_REALM_MARK = location.search.includes('realm')
export const WSS_ENABLED = !!ensureSingleString(qs.get('ws'))
export const FORCE_SEND_MESSAGE = location.search.includes('FORCE_SEND_MESSAGE')
export const ALLOW_SWIFT_SHADER = location.search.includes('ALLOW_SWIFT_SHADER')

const ASSET_BUNDLES_DOMAIN = ensureSingleString(qs.get('ASSET_BUNDLES_DOMAIN'))
export const SOCIAL_SERVER_URL = ensureSingleString(qs.get('SOCIAL_SERVER_URL'))

export const QS_MAX_VISIBLE_PEERS =
  typeof qs.get('MAX_VISIBLE_PEERS') === 'string' ? parseInt(qs.get('MAX_VISIBLE_PEERS')!, 10) : undefined

export const BUILDER_SERVER_URL =
  ensureSingleString(qs.get('BUILDER_SERVER_URL')) ?? 'https://builder-api.decentraland.org/v1'

/**
 * Get the root URL and ensure not to end with slash
 * @returns Root URL with pathname where the index.html is served.
 */
export const rootURLPreviewMode = () => {
  if (typeof qs.get('CATALYST') === 'string' && qs.get('CATALYST')?.length !== 0) {
    return addHttpsIfNoProtocolIsSet(qs.get('CATALYST')!)
  }
  return `${location.origin}${location.pathname}`.replace(/\/$/, '')
}

export const PIN_CATALYST = PREVIEW
  ? rootURLPreviewMode()
  : typeof qs.get('CATALYST') === 'string'
  ? addHttpsIfNoProtocolIsSet(qs.get('CATALYST')!)
  : undefined

export const BYPASS_CONTENT_ALLOWLIST = qs.has('BYPASS_CONTENT_ALLOWLIST')
  ? qs.get('BYPASS_CONTENT_ALLOWLIST') === 'true'
  : PIN_CATALYST || globalThis.location.hostname !== 'play.decentraland.org'

const META_CONFIG_URL = ensureSingleString(qs.get('META_CONFIG_URL'))

export const CHANNEL_TO_JOIN_CONFIG_URL = ensureSingleString(qs.get('CHANNEL'))

export const commConfigurations = {
  commRadius: 4,
  peerTtlMs: 60000,
  voiceChatUseHRTF: location.search.includes('VOICE_CHAT_USE_HRTF')
}

// take address from http://contracts.decentraland.org/addresses.json

export enum ETHEREUM_NETWORK {
  MAINNET = 'mainnet',
  SEPOLIA = 'sepolia'
}

const knownTLDs = ['zone', 'org', 'today']

// return one of org zone today
export function getTLD() {
  if (ENV_OVERRIDE) {
    return location.search.match(/ENV=(\w+)/)![1]
  }
  const previsionalTld = location.hostname.match(/(\w+)$/)![0]
  if (knownTLDs.includes(previsionalTld)) return previsionalTld
  return 'org'
}

export const WITH_FIXED_ITEMS = (qs.get('WITH_ITEMS') && ensureSingleString(qs.get('WITH_ITEMS'))) || ''
export const WITH_FIXED_COLLECTIONS =
  (qs.get('WITH_COLLECTIONS') && ensureSingleString(qs.get('WITH_COLLECTIONS'))) || ''
export const ENABLE_EMPTY_SCENES = !location.search.includes('DISABLE_EMPTY_SCENES')

export function getAssetBundlesBaseUrl(network: ETHEREUM_NETWORK): string {
  const state = store.getState()

  if (getFeatureFlagEnabled(state, 'ab-new-cdn')) {
    // IMPORTANT: The new ab-cdn supports versioning, so the global config is now
    //            ignored.
    // TODO: this will be customizable per scene/world/wearable. for now it only
    //       has one possible value
    return ASSET_BUNDLES_DOMAIN || getNewDefaultAssetBundlesBaseUrl(network)
  }

  return (
    ASSET_BUNDLES_DOMAIN || state.meta.config.explorer?.assetBundlesFetchUrl || getDefaultAssetBundlesBaseUrl(network)
  )
}

function getDefaultAssetBundlesBaseUrl(network: ETHEREUM_NETWORK): string {
  const tld = network === ETHEREUM_NETWORK.MAINNET ? 'org' : 'zone'
  return `https://content-assets-as-bundle.decentraland.${tld}`
}

function getNewDefaultAssetBundlesBaseUrl(network: ETHEREUM_NETWORK): string {
  const tld = network === ETHEREUM_NETWORK.MAINNET ? 'org' : 'zone'
  return `https://ab-cdn.decentraland.${tld}`
}

export function getAvatarTextureAPIBaseUrl(network: ETHEREUM_NETWORK): string {
  const tld = network === ETHEREUM_NETWORK.MAINNET ? 'org' : 'zone'
  // TODO!: Change this to point to social once the rollout is complete
  return `https://synapse.decentraland.${tld}/profile-pictures/`
}

export function getServerConfigurations(network: ETHEREUM_NETWORK) {
  const tld = network === ETHEREUM_NETWORK.MAINNET ? 'org' : 'zone'

  const metaConfigBaseUrl = META_CONFIG_URL || `https://config.decentraland.${tld}/explorer.json`

  const questsUrl =
    ensureSingleString(qs.get('QUESTS_SERVER_URL')) ?? `https://quests-api.decentraland.${network ? 'org' : 'io'}`

  return {
    explorerConfiguration: `${metaConfigBaseUrl}?t=${now()}`,
    questsUrl
  }
}

function assertValue<T>(val: T | undefined | null): T {
  if (!val) throw new Error('Value is missing')
  return val
}

export namespace ethereumConfigurations {
  export const mainnet = {
    wss: 'wss://rpc.decentraland.org/mainnet',
    http: 'https://rpc.decentraland.org/mainnet',
    etherscan: 'https://etherscan.io',
    names: 'https://api.thegraph.com/subgraphs/name/decentraland/marketplace',

    // contracts
    LANDProxy: assertValue(contractInfo.mainnet.LANDProxy),
    EstateProxy: assertValue(contractInfo.mainnet.EstateProxy),
    CatalystProxy: assertValue(contractInfo.mainnet.CatalystProxy),
    MANAToken: assertValue(contractInfo.mainnet.MANAToken)
  }
  export const sepolia = {
    wss: 'wss://rpc.decentraland.org/sepolia',
    http: 'https://rpc.decentraland.org/sepolia',
    etherscan: 'https://sepolia.etherscan.io',
    names: 'https://api.studio.thegraph.com/query/49472/marketplace-sepolia/version/latest',

    // contracts
    LANDProxy: assertValue(contractInfo.sepolia.LANDProxy),
    EstateProxy: assertValue(contractInfo.sepolia.EstateProxy),
    CatalystProxy: assertValue(contractInfo.sepolia.Catalyst),
    MANAToken: assertValue(contractInfo.sepolia.MANAToken)
  }
}

const testConfig = {
  isRunningTest: false
}
export const setRunningTest = (test: boolean) => {
  testConfig.isRunningTest = test
}
export const isRunningTest = () => testConfig.isRunningTest
export const PORTABLE_EXPERIENCES_DEBOUNCE_DELAY = () => (isRunningTest() ? 1 : 100)

function addHttpsIfNoProtocolIsSet(domain: string): string
function addHttpsIfNoProtocolIsSet(domain: undefined): undefined
function addHttpsIfNoProtocolIsSet(domain: null): null
function addHttpsIfNoProtocolIsSet(domain: any) {
  if (typeof domain === 'undefined' || domain === null) return domain
  if (!domain.startsWith('http')) {
    return `https://${domain}`
  }
  return domain
}
