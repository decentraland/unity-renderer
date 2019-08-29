import {
  Entity,
  AvatarShape,
  Transform,
  Skin,
  Hair,
  Face,
  Eyes,
  Wearable,
  engine,
  Color4,
  Vector3
  // , Texture
} from 'decentraland-ecs/src'

// avatar instantiation
const avatarEntity = new Entity()
avatarEntity.addComponentOrReplace(new Transform())

const avatarshape = new AvatarShape()
avatarshape.useDummyModel = false
avatarshape.name = 'testAvatar9'
avatarshape.id = avatarshape.name
avatarshape.baseUrl = 'https://s3.amazonaws.com/content-service.decentraland.zone/'

const bodyShape = new Wearable('body_shape', 'BaseMale.glb', [
  {
    hash: 'QmdyPfi4sRYU3eMFWxeXArnCeQ78sZw7oSGxFrntAPqHhy',
    file: 'Avatar_MaleSkinBase.png'
  },
  {
    hash: 'QmRhw6iFmT1r8KTbComWMKFLPxf7pFZqFwKrEY1Az2whZ8',
    file: 'BaseMale.glb'
  },
  {
    hash: 'QmP4823KZpn5d5iAEwXJyGTZy19HriJsJYkCJ22ERmpFMa',
    file: 'EyeBrows_00.png'
  },
  {
    hash: 'QmUYEYptrfbtvAr53C4X3ReAtYYuQsuN41dB5D5cfuLxkh',
    file: 'Eyes_00.png'
  },
  {
    hash: 'QmTZXe8CyZWCfV9KvjKx6LpYwDXAzCeRCozoZRuRUth7qP',
    file: 'Mouth_00.png'
  },
  {
    hash: 'QmbEoKs839SoKDSHF9tkgc3S6ge777ywU9X85W8K42WePu',
    file: 'thumbnail.png'
  }
])
avatarshape.bodyShape = bodyShape

const wearables = [
  new Wearable('eye_wear', 'M_Eyewear_AviatorStyle.glb', [
    {
      hash: 'QmWLrKJFzDCMGXVCef78SDkMHWB94eHP1ZeXfyci3kphTb',
      file: 'AvatarWearables_TX.png'
    },
    {
      hash: 'Qmbv617p43YUd9g2Xo8YzmgutGXjGt8AXU89fL3TzZvN3L',
      file: 'M_Eyewear_AviatorStyle.glb'
    },
    {
      hash: 'QmTnNj1hRjdPbhQPo7Gg71RPT5FXiw2zLeqwZNYhwnhdhQ',
      file: 'thumbnail.png'
    }
  ]),
  new Wearable('facial_hair', 'M_FacialHair_BalbooBeard.glb', [
    {
      hash: 'QmWLrKJFzDCMGXVCef78SDkMHWB94eHP1ZeXfyci3kphTb',
      file: 'AvatarWearables_TX.png'
    },
    {
      hash: 'QmVPP2Y6mqkb7WYw1kLffYq2nGMwx3Y1LmVJJmfafpuZys',
      file: 'M_FacialHair_BalbooBeard.glb'
    },
    {
      hash: 'QmTcMounmnAnWrSXY2za4f8ZxeStP3WKEm5LNvunC1xnS6',
      file: 'thumbnail.png'
    }
  ]),
  new Wearable('feet', 'M_Feet_GreenFlipFlops.glb', [
    {
      hash: 'QmRaHnacT5G7oLYTYGsRWZtLXzXuTNEq7gWAcvXRSxfwEU',
      file: 'AvatarWearables_TX.png'
    },
    {
      hash: 'QmdyPfi4sRYU3eMFWxeXArnCeQ78sZw7oSGxFrntAPqHhy',
      file: 'Avatar_MaleSkinBase.png'
    },
    {
      hash: 'QmVVHB1CqnUSZtQ3sbpoyVss3vnSf4bSzhbiBSaQJVaxNr',
      file: 'M_Feet_GreenFlipFlops.glb'
    },
    {
      hash: 'QmeT9F7gV314xtaB2K6vV8gSSj8EQw83gc3RuJZg4GKPM5',
      file: 'thumbnail.png'
    }
  ]),
  new Wearable('hair', 'M_Hair_Punk_01.glb', [
    {
      hash: 'QmRaHnacT5G7oLYTYGsRWZtLXzXuTNEq7gWAcvXRSxfwEU',
      file: 'AvatarWearables_TX.png'
    },
    {
      hash: 'QmZQPNWdBc1wtcRtknuRvPJdKkTwPFW2K76VimfF9MeX3s',
      file: 'M_Hair_Punk_01.glb'
    },
    {
      hash: 'QmPxPWeVkuX74XS1jDnCPwbZhf23gjQkXKHrqzu9GF24wq',
      file: 'thumbnail.png'
    }
  ]),
  new Wearable('lower_body', 'M_lBody_GreyJoggers.glb', [
    {
      hash: 'QmRaHnacT5G7oLYTYGsRWZtLXzXuTNEq7gWAcvXRSxfwEU',
      file: 'AvatarWearables_TX.png'
    },
    {
      hash: 'QmWNqkuhiCL6YHRvbwYniwqYPC6yvm62cHyky8eUaP2aRM',
      file: 'M_lBody_GreyJoggers.glb'
    },
    {
      hash: 'QmSWqcYdV5dvWSg6MCp4YaPB8vpTPUQjdNmVghdH86znTF',
      file: 'thumbnail.png'
    }
  ]),
  new Wearable('upper_body', 'M_uBody_GreenTShirt.glb', [
    {
      hash: 'QmRaHnacT5G7oLYTYGsRWZtLXzXuTNEq7gWAcvXRSxfwEU',
      file: 'AvatarWearables_TX.png'
    },
    {
      hash: 'QmdyPfi4sRYU3eMFWxeXArnCeQ78sZw7oSGxFrntAPqHhy',
      file: 'Avatar_MaleSkinBase.png'
    },
    {
      hash: 'QmfKssVuMmVWiF1Qy7xrNUdukinmfoovsCGNnuEJVczWEs',
      file: 'M_uBody_GreenTShirt.glb'
    },
    {
      hash: 'Qmayvq1F3tFDoC5dLdpGR6LmKkhzTANjnungxmhGYVBBKw',
      file: 'thumbnail.png'
    }
  ])
]
avatarshape.wearables = wearables

const skin = new Skin(new Color4(0.6683928370475769, 0.9275454878807068, 0.8263601064682007, 1.0))
avatarshape.skin = skin

const hair = new Hair(new Color4(0.47648438811302187, 0.22779148817062379, 0.5416662096977234, 1.0))
avatarshape.hair = hair

// eyes_02
const eyes = new Eyes(
  'QmeTxEdW7kp52RV8vZHsrzqed268D5m41T5dVp9JjLXiEr',
  'QmYs9ec68TjZkRpjz2Sov2GcNhP5AD8JUNGm3MrahpF9RB',
  Color4.Teal()
)
avatarshape.eyes = eyes

// eyebrows_00
const eyebrows = new Face('QmZWwUgq6UYJLCgtPbzVdhThkPTNPHqezWiQKw7Lg3yqKV')
avatarshape.eyebrows = eyebrows

// mouth_00
const mouth = new Face('QmQZ6MAyHpGaPpntHJnU3x4o2dUvz5KhvNtWbbzvbAE34T')
avatarshape.mouth = mouth

avatarEntity.addComponentOrReplace(avatarshape)
engine.addEntity(avatarEntity)

const avatarEntity1 = new Entity()
const t1 = new Transform()
t1.position = new Vector3(1, 2, 3)
avatarEntity1.addComponentOrReplace(t1)

const avatarshape1 = AvatarShape.Dummy()
avatarshape1.name = 'dummy'
avatarshape1.id = avatarshape1.name

avatarEntity1.addComponentOrReplace(avatarshape1)
engine.addEntity(avatarEntity1)
