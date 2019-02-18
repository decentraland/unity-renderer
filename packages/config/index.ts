export const performanceConfigurations = [
  { antialiasing: true, downsampling: 0, shadows: true },
  { antialiasing: false, downsampling: 1, shadows: true },
  { antialiasing: false, downsampling: 1, shadows: false },
  { antialiasing: false, downsampling: 1, shadows: true },
  { antialiasing: false, downsampling: 2, shadows: false }
]

export const NETWORK_HZ = 10

export const playerSphereRadius = 0.4

export namespace interactionLimits {
  /**
   * click distance, this is the lenght of the ray/lens
   */
  export const clickDistance = 10
  /**
   * Scale of the marker on the tip of laser beam from VR controller
   */
  export const markerScale = 0.07
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

  export const visibleRadius = 6

  export const maxParcelX = 3000
  export const maxParcelZ = 3000
  export const minParcelX = -3000
  export const minParcelZ = -3000
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

export const PREVIEW: boolean = !!(global as any)['preview']
export const EDITOR: boolean = !!(global as any)['isEditor']
export const DEBUG = location.search.indexOf('DEBUG') !== -1 || !!(global as any)['mocha'] || PREVIEW || EDITOR
export const MOBILE_DEBUG = location.search.indexOf('MOBILE_DEBUG') !== -1
export const DEBUG_METRICS = DEBUG && location.search.indexOf('DEBUG_METRICS') !== -1

export namespace commConfigurations {
  export const debug = DEBUG
  export const commRadius = 4
  export const antennaRadius = 2

  export const processConnectionsIntervalMs = 500
  export const maxConcurrentConnectionRequests = 5
  export const peerTtlMs = 10 * 1000

  export const maxVisiblePeers = 25

  export const defaultCommServerUrl = 'wss://world-comm.decentraland.today/connector'

  export const webrtcSupportEnabled = false
  export const iceServers = [
    {
      urls: 'stun:stun.l.google.com:19302'
    }
  ]
}

export const isStandaloneHeadset = navigator && navigator.userAgent.includes('Oculus')

// take address from http://contracts.decentraland.org/addresses.json

export enum ETHEREUM_NETWORK {
  MAINNET = 'mainnet',
  ROPSTEN = 'ropsten'
}

export const networkConfigurations = {
  [ETHEREUM_NETWORK.MAINNET]: {
    wss: 'wss://mainnet.infura.io/ws',
    http: 'https://mainnet.infura.io/',
    contractAddress: '0xF87E31492Faf9A91B02Ee0dEAAd50d51d56D5d4d',
    landApi: 'https://api.decentraland.org/v1',
    etherscan: 'https://etherscan.io',

    contracts: {
      serviceLocator: '0x151b11892dd6ab1f91055dcd01d23d03a2c47570'
    },

    paymentTokens: {
      MANA: '0x0F5D2fB29fb7d3CFeE444a200298f468908cC942'
    },

    content: 'https://content.decentraland.org',
    invite: '0xf886313f213c198458eba7ae9329525e64eb763a'
  },
  [ETHEREUM_NETWORK.ROPSTEN]: {
    wss: 'wss://ropsten.infura.io/ws',
    http: 'https://ropsten.infura.io/',
    contractAddress: '0x7a73483784ab79257bb11b96fd62a2c3ae4fb75b',
    landApi: 'https://api.decentraland.zone/v1',
    etherscan: 'https://ropsten.etherscan.io',

    contracts: {
      serviceLocator: '0xb240b30c12d2a9ea6ba3abbf663d9ae265fbebeb'
    },

    paymentTokens: {
      MANA: '0x2a8fd99c19271f4f04b1b7b9c4f7cf264b626edb'
    },

    content: 'https://content.decentraland.zone',
    invite: '0x7557dfa02f3bd7d274851e3f627de2ed2ff390e8'
  }
}

export const isRunningTest: boolean = (global as any)['isRunningTests'] === true

export const ROADS_DISTRICT = 'f77140f9-c7b4-4787-89c9-9fa0e219b079'
