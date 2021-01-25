import { contracts as contractInfo } from './contracts'
const queryString = require('query-string')

export const NETWORK_HZ = 10

export namespace interactionLimits {
  /**
   * click distance, this is the length of the ray/lens
   */
  export const clickDistance = 10
}

export namespace parcelLimits {
  // Maximum numbers for parcelScenes to prevent performance problems
  // Note that more limitations may be added to this with time
  // And we may also measure individual parcelScene performance (as
  // in webgl draw time) and disable parcelScenes based on that too,
  // Performance / anti-ddos work is a fluid area.

  // number of entities
  export const entities = 200

  // Number of faces (per parcel)
  export const triangles = 10000
  export const bodies = 300
  export const textures = 10
  export const materials = 20
  export const height = 20
  export const geometries = 200

  export const parcelSize = 16 /* meters */
  export const halfParcelSize = parcelSize / 2 /* meters */
  export const centimeter = 0.01

  export const visibleRadius = 4
  export const secureRadius = 4

  export const maxX = 3000
  export const maxZ = 3000
  export const minX = -3000
  export const minZ = -3000

  export const maxParcelX = 150
  export const maxParcelZ = 150
  export const minParcelX = -150
  export const minParcelZ = -150

  export const minLandCoordinateX = -150
  export const minLandCoordinateY = -150
  export const maxLandCoordinateX = 150
  export const maxLandCoordinateY = 150
}

export namespace playerConfigurations {
  export const gravity = -0.2
  export const height = 1.6
  export const handFromBodyDistance = 0.5
  // The player speed
  export const speed = 2
  export const runningSpeed = 8
  // The player inertia
  export const inertia = 0.01
  // The mouse sensibility (lower is most sensible)
  export const angularSensibility = 500
}

export namespace visualConfigurations {
  export const fieldOfView = 75
  export const farDistance = parcelLimits.visibleRadius * parcelLimits.parcelSize

  export const near = 0.08
  export const far = farDistance
}

// Entry points
export const PREVIEW: boolean = !!(global as any).preview
export const EDITOR: boolean = !!(global as any).isEditor
export const WORLD_EXPLORER = !EDITOR && !PREVIEW

export const OPEN_AVATAR_EDITOR = location.search.includes('OPEN_AVATAR_EDITOR') && WORLD_EXPLORER

export const STATIC_WORLD = location.search.includes('STATIC_WORLD') || !!(global as any).staticWorld || EDITOR

// Development
export const ENABLE_WEB3 = location.search.includes('ENABLE_WEB3') || !!(global as any).enableWeb3
export const ENV_OVERRIDE = location.search.includes('ENV')
export const GIF_WORKERS = location.search.includes('GIF_WORKERS')

const qs = queryString.parse(location.search)

// Comms
export const USE_LOCAL_COMMS = location.search.includes('LOCAL_COMMS') || PREVIEW
export const COMMS = USE_LOCAL_COMMS ? 'v1-local' : qs.COMMS ? qs.COMMS : 'v2-p2p' // by default
export const COMMS_PROFILE_TIMEOUT = 10000

export const UPDATE_CONTENT_SERVICE = qs.UPDATE_CONTENT_SERVICE
export const FETCH_CONTENT_SERVICE = qs.FETCH_CONTENT_SERVICE
export const COMMS_SERVICE = qs.COMMS_SERVICE
export const RESIZE_SERVICE = qs.RESIZE_SERVICE
export const HOTSCENES_SERVICE = qs.HOTSCENES_SERVICE
export const REALM = qs.realm

export const VOICE_CHAT_DISABLED_FLAG = location.search.includes('VOICE_CHAT_DISABLED')

export const VOICE_CHAT_ENABLED_FLAG = location.search.includes('VOICE_CHAT_ENABLED')

export const ENABLE_BUILDER_IN_WORLD = location.search.includes('ENABLE_BUILDER_IN_WORLD')

export const AUTO_CHANGE_REALM = location.search.includes('AUTO_CHANGE_REALM')

export const LOS = qs.LOS

export const DEBUG = location.search.includes('DEBUG_MODE') || !!(global as any).mocha || PREVIEW || EDITOR
export const DEBUG_ANALYTICS = location.search.includes('DEBUG_ANALYTICS')
export const DEBUG_MOBILE = location.search.includes('DEBUG_MOBILE')
export const DEBUG_MESSAGES = location.search.includes('DEBUG_MESSAGES')
export const DEBUG_MESSAGES_QUEUE_PERF = location.search.includes('DEBUG_MESSAGES_QUEUE_PERF')
export const DEBUG_WS_MESSAGES = location.search.includes('DEBUG_WS_MESSAGES')
export const DEBUG_REDUX = location.search.includes('DEBUG_REDUX')
export const DEBUG_LOGIN = location.search.includes('DEBUG_LOGIN')
export const DEBUG_PM = location.search.includes('DEBUG_PM')
export const DEBUG_SCENE_LOG = DEBUG || location.search.includes('DEBUG_SCENE_LOG')

export const INIT_PRE_LOAD = location.search.includes('INIT_PRE_LOAD')

export const NO_MOTD = location.search.includes('NO_MOTD')
export const RESET_TUTORIAL = location.search.includes('RESET_TUTORIAL')

export const ENGINE_DEBUG_PANEL = location.search.includes('ENGINE_DEBUG_PANEL')
export const SCENE_DEBUG_PANEL = location.search.includes('SCENE_DEBUG_PANEL') && !ENGINE_DEBUG_PANEL
export const SHOW_FPS_COUNTER = location.search.includes('SHOW_FPS_COUNTER') || DEBUG
export const HAS_INITIAL_POSITION_MARK = location.search.includes('position')
export const NO_ASSET_BUNDLES = location.search.includes('NO_ASSET_BUNDLES')
export const WSS_ENABLED = qs.ws !== undefined
export const FORCE_SEND_MESSAGE = location.search.includes('FORCE_SEND_MESSAGE')

export const PIN_CATALYST = qs.CATALYST ? addHttpsIfNoProtocolIsSet(qs.CATALYST) : undefined

export const FORCE_RENDERING_STYLE = qs.FORCE_RENDERING_STYLE

export const TEST_WEARABLES_OVERRIDE = location.search.includes('TEST_WEARABLES')

const META_CONFIG_URL = qs.META_CONFIG_URL

const QUESTS_SERVER_URL = qs.QUESTS_SERVER_URL ?? 'https://quests-api.decentraland.io'

export namespace commConfigurations {
  export const debug = true
  export const commRadius = 4

  export const sendAnalytics = true

  export const peerTtlMs = 60000

  export const maxVisiblePeers = qs.MAX_VISIBLE_PEERS ? parseInt(qs.MAX_VISIBLE_PEERS, 10) : 25

  export const autoChangeRealmInterval = qs.AUTO_CHANGE_INTERVAL ? parseInt(qs.AUTO_CHANGE_INTERVAL, 10) * 1000 : 40000

  export const iceServers = [
    {
      urls: 'stun:stun.l.google.com:19302'
    },
    {
      urls: 'stun:stun2.l.google.com:19302'
    },
    {
      urls: 'stun:stun3.l.google.com:19302'
    },
    {
      urls: 'stun:stun4.l.google.com:19302'
    },
    {
      urls: 'turn:stun.decentraland.org:3478',
      credential: 'passworddcl',
      username: 'usernamedcl'
    }
  ]

  export const voiceChatUseHRTF = location.search.includes('VOICE_CHAT_USE_HRTF')
}
export const loginConfig = {
  org: {
    domain: 'decentraland.auth0.com',
    client_id: 'yqFiSmQsxk3LK46JOIB4NJ3wK4HzZVxG'
  },
  today: {
    domain: 'dcl-stg.auth0.com',
    client_id: '0UB0I7w6QA3AgSvbXh9rGvDuhKrJV1C0'
  },
  zone: {
    domain: 'dcl-test.auth0.com',
    client_id: 'lTUEMnFpYb0aiUKeIRPbh7pBxKM6sccx'
  }
}

// take address from http://contracts.decentraland.org/addresses.json

export enum ETHEREUM_NETWORK {
  MAINNET = 'mainnet',
  ROPSTEN = 'ropsten'
}

export let decentralandConfigurations: any = {}
let contracts: any = null
let network: ETHEREUM_NETWORK | null = null

export function getTLD() {
  if (ENV_OVERRIDE) {
    return location.search.match(/ENV=(\w+)/)![1]
  }
  return location.hostname.match(/(\w+)$/)![0]
}

export const knownTLDs = ['zone', 'org', 'today']

export function getDefaultTLD() {
  const TLD = getTLD()
  if (ENV_OVERRIDE) {
    return TLD
  }

  // web3 is now disabled by default
  if (!ENABLE_WEB3 && TLD === 'localhost') {
    return 'zone'
  }

  if (!TLD || !knownTLDs.includes(TLD)) {
    return network === ETHEREUM_NETWORK.ROPSTEN ? 'zone' : 'org'
  }

  return TLD
}

export function getExclusiveServer() {
  const url = new URL(location.toString())
  if (url.searchParams.has('TEST_WEARABLES')) {
    const value = url.searchParams.get('TEST_WEARABLES')
    if (value) {
      try {
        return new URL(value).toString()
      } catch (e) {
        return `https://${value}/index.json`
      }
    }
    return 'https://dcl-wearables-dev.now.sh/index.json'
  }
  return 'https://wearable-api.decentraland.org/v2/collections'
}

export const ALL_WEARABLES = location.search.includes('ALL_WEARABLES') && getDefaultTLD() !== 'org'
export const WEARABLE_API_DOMAIN = qs.WEARABLE_API_DOMAIN || 'wearable-api.decentraland.org'
export const WEARABLE_API_PATH_PREFIX = qs.WEARABLE_API_PATH_PREFIX || 'v2'
export const ENABLE_EMPTY_SCENES = !DEBUG || knownTLDs.includes(getTLD())

export function getWearablesSafeURL() {
  return 'https://content.decentraland.org'
}

export function getNetworkFromTLD(tld: string = getTLD()): ETHEREUM_NETWORK | null {
  if (tld === 'zone') {
    return ETHEREUM_NETWORK.ROPSTEN
  }

  if (tld === 'today' || tld === 'org') {
    return ETHEREUM_NETWORK.MAINNET
  }

  // if localhost
  return null
}

export function getServerConfigurations() {
  const TLDDefault = getDefaultTLD()
  const notToday = TLDDefault === 'today' ? 'org' : TLDDefault

  const synapseUrl = TLDDefault === 'zone' ? `https://matrix.decentraland.zone` : `https://decentraland.modular.im`

  const metaConfigBaseUrl = META_CONFIG_URL || `https://config.decentraland.${notToday}/explorer.json`
  const ASSET_BUNDLES_DOMAIN = qs.ASSET_BUNDLES_DOMAIN || `content-assets-as-bundle.decentraland.${TLDDefault}`

  return {
    contentAsBundle: `https://${ASSET_BUNDLES_DOMAIN}`,
    wearablesApi: `https://${WEARABLE_API_DOMAIN}/${WEARABLE_API_PATH_PREFIX}`,
    explorerConfiguration: `${metaConfigBaseUrl}?t=${new Date().getTime()}`,
    synapseUrl,
    questsUrl: QUESTS_SERVER_URL,
    fallbackResizeServiceUrl: `${PIN_CATALYST ?? 'https://peer.decentraland.' + notToday}/lambdas/images`,
    avatar: {
      snapshotStorage: `https://avatars-storage.decentraland.${TLDDefault}/`, // ** TODO - unused, remove - moliva - 03/07/2020
      catalog: getExclusiveServer(),
      presets: `https://avatars-storage.decentraland.org/mobile-avatars` // ** TODO - unused, remove - moliva - 03/07/2020
    }
  }
}

export async function setNetwork(net: ETHEREUM_NETWORK) {
  try {
    const json = contractInfo

    network = net
    contracts = json[net]

    contracts['CatalystProxy'] =
      net === ETHEREUM_NETWORK.MAINNET
        ? '0x4a2f10076101650f40342885b99b6b101d83c486'
        : '0xadd085f2318e9678bbb18b3e0711328f902b374b'

    decentralandConfigurations = {
      ...contracts,
      contractAddress: contracts.LANDProxy,
      dao: contracts.CatalystProxy,
      ens: contracts.CatalystProxy,
      contracts: {
        serviceLocator: contracts.ServiceLocator
      },
      paymentTokens: {
        MANA: contracts.MANAToken
      }
    }
  } catch (e) {
    // Could not fetch addresses. You might be offline. Setting sensitive defaults for contract addresses...

    network = net
    contracts = {}

    decentralandConfigurations = {
      contractAddress: '',
      dao: '',
      contracts: {
        serviceLocator: ''
      },
      paymentTokens: {
        MANA: ''
      }
    }
  }
}

export namespace ethereumConfigurations {
  export const mainnet = {
    wss: 'wss://mainnet.infura.io/ws/v3/074a68d50a7c4e6cb46aec204a50cbf0',
    http: 'https://mainnet.infura.io/v3/074a68d50a7c4e6cb46aec204a50cbf0/',
    etherscan: 'https://etherscan.io',
    names: 'https://api.thegraph.com/subgraphs/name/decentraland/marketplace'
  }
  export const ropsten = {
    wss: 'wss://ropsten.infura.io/ws/v3/074a68d50a7c4e6cb46aec204a50cbf0',
    http: 'https://ropsten.infura.io/v3/074a68d50a7c4e6cb46aec204a50cbf0/',
    etherscan: 'https://ropsten.etherscan.io',
    names: 'https://api.thegraph.com/subgraphs/name/decentraland/marketplace-ropsten'
  }
}

export const isRunningTest: boolean = (global as any)['isRunningTests'] === true

// @todo replace before merge
export const WALLET_API_KEYS = new Map<ETHEREUM_NETWORK, Map<string, string>>([
  [ETHEREUM_NETWORK.ROPSTEN, new Map([['Fortmatic', 'pk_test_198DDD3CA646DE2F']])],
  [ETHEREUM_NETWORK.MAINNET, new Map([['Fortmatic', 'pk_live_D7297F51E9776DD2']])]
])

export const genericAvatarSnapshots: Record<string, string> = {
  face: '/images/avatar_snapshot_default.png',
  body: '/images/image_not_found.png',
  face256: '/images/avatar_snapshot_default256.png',
  face128: '/images/avatar_snapshot_default128.png'
}

export function getCatalystNodesDefaultURL() {
  return `https://peer.decentraland.${getDefaultTLD()}/lambdas/contracts/servers`
}

function addHttpsIfNoProtocolIsSet(domain: string): string {
  if (!domain.startsWith('http')) {
    return `https://${domain}`
  }
  return domain
}
