import { Avatar } from '@dcl/schemas'

export function backupProfile(userId: string): Avatar {
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
      bodyShape: 'urn:decentraland:off-chain:base-avatars:BaseFemale',
      skin: {
        color: {
          r: 0.4901960790157318,
          g: 0.364705890417099,
          b: 0.27843138575553894
        }
      },
      hair: {
        color: {
          r: 0.5960784554481506,
          g: 0.37254902720451355,
          b: 0.21568627655506134
        }
      },
      eyes: {
        color: {
          r: 0.37254902720451355,
          g: 0.2235294133424759,
          b: 0.19607843458652496
        }
      },
      wearables: [
        'urn:decentraland:off-chain:base-avatars:f_sweater',
        'urn:decentraland:off-chain:base-avatars:f_jeans',
        'urn:decentraland:off-chain:base-avatars:bun_shoes',
        'urn:decentraland:off-chain:base-avatars:standard_hair',
        'urn:decentraland:off-chain:base-avatars:f_eyes_00',
        'urn:decentraland:off-chain:base-avatars:f_eyebrows_00',
        'urn:decentraland:off-chain:base-avatars:f_mouth_00'
      ],
      snapshots: {
        face256: `QmZbyGxDnZ4PaMVX7kpA2NuGTrmnpwTJ8heKKTSCk4GRJL`,
        body: `QmaQvcBWg57Eqf5E9R3Ts1ttPKKLhKueqdyhshaLS1tu2g`
      }
    }
  }
}
