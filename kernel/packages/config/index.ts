import { getUserProfile } from 'shared/comms/peers'
import { tutorialStepId } from '../decentraland-loader/lifecycle/tutorial/tutorial'
import { contracts as contractInfo } from './contracts'
const queryString = require('query-string')
declare var window: any

export const performanceConfigurations = [
  { antialiasing: true, downsampling: 0, shadows: true },
  { antialiasing: false, downsampling: 1, shadows: true },
  { antialiasing: false, downsampling: 1, shadows: false },
  { antialiasing: false, downsampling: 1, shadows: true },
  { antialiasing: false, downsampling: 2, shadows: false }
]

export const NETWORK_HZ = 10

export namespace interactionLimits {
  /**
   * click distance, this is the lenght of the ray/lens
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

export const OPEN_AVATAR_EDITOR = location.search.indexOf('OPEN_AVATAR_EDITOR') !== -1 && WORLD_EXPLORER

export const STATIC_WORLD = location.search.indexOf('STATIC_WORLD') !== -1 || !!(global as any).staticWorld || EDITOR

// Development
export const ENABLE_WEB3 = location.search.indexOf('ENABLE_WEB3') !== -1 || !!(global as any).enableWeb3
export const ENV_OVERRIDE = location.search.indexOf('ENV') !== -1
export const USE_NEW_CHAT = location.search.indexOf('USE_OLD_CHAT') === -1

const qs = queryString.parse(location.search)

// Comms
export const USE_LOCAL_COMMS = location.search.indexOf('LOCAL_COMMS') !== -1 || PREVIEW
export const COMMS = USE_LOCAL_COMMS ? 'v1-local' : qs.COMMS ? qs.COMMS : 'v2-p2p' // by default

export const FETCH_PROFILE_SERVICE = qs.FETCH_PROFILE_SERVICE
export const UPDATE_CONTENT_SERVICE = qs.UPDATE_CONTENT_SERVICE
export const FETCH_CONTENT_SERVICE = qs.FETCH_CONTENT_SERVICE
export const FETCH_META_CONTENT_SERVICE = qs.FETCH_META_CONTENT_SERVICE
export const COMMS_SERVICE = qs.COMMS_SERVICE
export const REALM = qs.realm

export const AUTO_CHANGE_REALM = location.search.indexOf('AUTO_CHANGE_REALM') !== -1

export const DEBUG =
  location.search.indexOf('DEBUG_MODE') !== -1 ||
  location.search.indexOf('DEBUG_LOG') !== -1 ||
  !!(global as any).mocha ||
  PREVIEW ||
  EDITOR
export const DEBUG_ANALYTICS = location.search.indexOf('DEBUG_ANALYTICS') !== -1
export const DEBUG_MOBILE = location.search.indexOf('DEBUG_MOBILE') !== -1
export const DEBUG_MESSAGES = location.search.indexOf('DEBUG_MESSAGES') !== -1
export const DEBUG_WS_MESSAGES = location.search.indexOf('DEBUG_WS_MESSAGES') !== -1
export const DEBUG_REDUX = location.search.indexOf('DEBUG_REDUX') !== -1
export const DEBUG_LOGIN = location.search.indexOf('DEBUG_LOGIN') !== -1
export const DEBUG_PM = location.search.indexOf('DEBUG_PM') !== -1

export const AWS = location.search.indexOf('AWS') !== -1
export const NO_MOTD = location.search.indexOf('NO_MOTD') !== -1

export const DISABLE_AUTH = location.search.indexOf('DISABLE_AUTH') !== -1 || DEBUG
export const ENGINE_DEBUG_PANEL = location.search.indexOf('ENGINE_DEBUG_PANEL') !== -1
export const SCENE_DEBUG_PANEL = location.search.indexOf('SCENE_DEBUG_PANEL') !== -1 && !ENGINE_DEBUG_PANEL
export const SHOW_FPS_COUNTER = location.search.indexOf('SHOW_FPS_COUNTER') !== -1 || DEBUG
export const RESET_TUTORIAL = location.search.indexOf('RESET_TUTORIAL') !== -1
export const NO_TUTORIAL = true
export const HAS_INITIAL_POSITION_MARK = location.search.indexOf('position') !== -1

export function tutorialEnabled() {
  return (
    !NO_TUTORIAL &&
    WORLD_EXPLORER &&
    !HAS_INITIAL_POSITION_MARK &&
    (RESET_TUTORIAL || getUserProfile().profile.tutorialStep !== tutorialStepId.FINISHED)
  )
}

export function tutorialSceneEnabled() {
  return tutorialEnabled() && (RESET_TUTORIAL || getUserProfile().profile.tutorialStep === tutorialStepId.INITIAL_SCENE)
}

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
    return window.location.search.match(/ENV=(\w+)/)[1]
  }
  if (window) {
    return window.location.hostname.match(/(\w+)$/)[0]
  }
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
  if (window.location.search.match(/TEST_WEARABLES/)) {
    return 'https://dcl-wearables-dev.now.sh/index.json'
  }
  return 'https://wearable-api.decentraland.org/v2/collections'
}

export const ALL_WEARABLES = location.search.indexOf('ALL_WEARABLES') !== -1 && getDefaultTLD() !== 'org'

export const ENABLE_EMPTY_SCENES = !DEBUG || knownTLDs.includes(getTLD())

export function getWearablesSafeURL() {
  return 'https://content.decentraland.org'
}

export function getServerConfigurations() {
  const TLDDefault = getDefaultTLD()

  const synapseUrl = TLDDefault === 'zone' ? `https://matrix.decentraland.zone` : `https://decentraland.modular.im`

  return {
    contentAsBundle: `https://content-assets-as-bundle.decentraland.org`,
    wearablesApi: `https://wearable-api.decentraland.org/v2`,
    explorerConfiguration: `https://explorer-config.decentraland.${
      TLDDefault === 'today' ? 'org' : TLDDefault
    }/configuration.json`,
    synapseUrl,
    avatar: {
      snapshotStorage: `https://avatars-storage.decentraland.${TLDDefault}/`,
      catalog: getExclusiveServer(),
      presets: `https://avatars-storage.decentraland.org/mobile-avatars`
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
