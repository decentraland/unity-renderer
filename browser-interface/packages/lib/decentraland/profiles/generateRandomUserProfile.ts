import type { Avatar } from '@dcl/schemas'

const bodyShapes = [
  'urn:decentraland:off-chain:base-avatars:BaseFemale',
  'urn:decentraland:off-chain:base-avatars:BaseMale'
]

const skins = [
  {
    color: {
      r: 0.4901960790157318,
      g: 0.364705890417099,
      b: 0.27843138575553894
    }
  },
  {
    color: {
      r: 0.800000011920929,
      g: 0.6078431606292725,
      b: 0.46666666865348816
    }
  }
]

const hairs = [
  {
    color: {
      r: 0.5960784554481506,
      g: 0.37254902720451355,
      b: 0.21568627655506134
    }
  },
  {
    color: {
      r: 0.23529411852359772,
      g: 0.12941177189350128,
      b: 0.04313725605607033
    }
  }
]

const eyes = [
  {
    color: {
      r: 0.37254902720451355,
      g: 0.2235294133424759,
      b: 0.19607843458652496
    }
  },
  {
    color: {
      r: 0.125490203499794,
      g: 0.7019608020782471,
      b: 0.9647058844566345
    }
  }
]

const wearables = [
  [
    'urn:decentraland:off-chain:base-avatars:f_sweater',
    'urn:decentraland:off-chain:base-avatars:f_jeans',
    'urn:decentraland:off-chain:base-avatars:bun_shoes',
    'urn:decentraland:off-chain:base-avatars:standard_hair',
    'urn:decentraland:off-chain:base-avatars:f_eyes_00',
    'urn:decentraland:off-chain:base-avatars:f_eyebrows_00',
    'urn:decentraland:off-chain:base-avatars:f_mouth_00'
  ],
  [
    'urn:decentraland:off-chain:base-avatars:f_sport_purple_tshirt',
    'urn:decentraland:off-chain:base-avatars:f_roller_leggings',
    'urn:decentraland:off-chain:base-avatars:sport_black_shoes',
    'urn:decentraland:off-chain:base-avatars:hair_coolshortstyle',
    'urn:decentraland:off-chain:base-avatars:f_mouth_08'
  ],
  [
    'urn:decentraland:off-chain:base-avatars:turtle_neck_sweater',
    'urn:decentraland:off-chain:base-avatars:kilt',
    'urn:decentraland:off-chain:base-avatars:m_mountainshoes.glb',
    'urn:decentraland:off-chain:base-avatars:keanu_hair',
    'urn:decentraland:off-chain:base-avatars:full_beard'
  ]
]

const snapshots = [
  {
    face256: `QmZbyGxDnZ4PaMVX7kpA2NuGTrmnpwTJ8heKKTSCk4GRJL`,
    body: `QmaQvcBWg57Eqf5E9R3Ts1ttPKKLhKueqdyhshaLS1tu2g`
  },
  {
    body: 'bafkreifheg3u2lkq3dtxokid6nxtmqsd4elafoyx4hhemudosersmsdhlq',
    face256: 'bafkreiayztsbgjwellug2s52j7grfhknxhl5ybvkqdxu2fs2iit62rwo2i'
  }
]

export function generateRandomUserProfile(userId: string): Avatar {
  // Randomly select a value for each attribute
  const randomBodyShape = bodyShapes[Math.floor(Math.random() * bodyShapes.length)]
  const randomSkin = skins[Math.floor(Math.random() * skins.length)]
  const randomHair = hairs[Math.floor(Math.random() * hairs.length)]
  const randomEyes = eyes[Math.floor(Math.random() * eyes.length)]
  const randomWearables = wearables[Math.floor(Math.random() * wearables.length)]
  const randomSnapshots = snapshots[Math.floor(Math.random() * snapshots.length)]

  return {
    userId,
    email: '',
    version: -1,
    hasClaimedName: false,
    ethAddress: '0x0000000000000000000000000000000000000000',
    tutorialStep: 0,
    name: 'Guest',
    description: '',
    avatar: {
      bodyShape: randomBodyShape,
      skin: randomSkin,
      hair: randomHair,
      eyes: randomEyes,
      wearables: randomWearables,
      snapshots: randomSnapshots
    }
  }
}
