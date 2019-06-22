import { Entity, AvatarShape, Transform, Skin, Hair, Face, Eyes, Wearable , engine, Color4
  // , Texture
 } from 'decentraland-ecs/src'

// resources
/* const maleSkinTex = new Texture("avatarassets/Avatar_MaleSkinBase.png")
const eyebrowsBaseTex = new Texture("avatarassets/eyeBrows_00.png")
const eyebrowsTex = new Texture("avatarassets/eyebrows_texture.png")
const eyesBaseTex = new Texture("avatarassets/eyes_00.png")
const eyesTex = new Texture("avatarassets/eyes_texture.png")
const eyesMaskTex = new Texture("avatarassets/eyes_mask.png")
const mouthBaseTex = new Texture("avatarassets/Mouth_00.png")
const mouthTex = new Texture("avatarassets/mouth_texture.png")
const weareablesTex = new Texture("avatarassets/AvatarWearables_TX.png") */

// avatar instantiation
const avatarEntity = new Entity()
avatarEntity.addComponentOrReplace(new Transform())

const avatarshape = new AvatarShape()
avatarshape.name = "testAvatar"

const bodyShape = new Wearable()
bodyShape.contentName = "avatarassets/basemale.glb",
bodyShape.category = "body_shape"
avatarshape.bodyShape = bodyShape

const wearables = [new Wearable(), new Wearable(), new Wearable(), new Wearable(), new Wearable(), new Wearable()]
wearables[0].category = "eye_wear"
wearables[0].contentName = "avatarassets/m_eyewear_aviatorstyle.glb"
// wearables[0].contentName = "QmZNtkiiUFQ6uLnyVb5WpiS7Hdkq9MeszfcnLozEdiE3Rc"
wearables[1].category = "facial_hair"
wearables[1].contentName = "avatarassets/m_facialhair_frenchmustache.glb"
// wearables[1].contentName = "QmQLtmNwBETwf4aAPQ3WSZniRUdk18ZK3kfvuA9d8RpUzM"
wearables[2].category = "feet"
wearables[2].contentName = "avatarassets/m_feet_greenflipflops.glb"
// wearables[2].contentName = "QmVVHB1CqnUSZtQ3sbpoyVss3vnSf4bSzhbiBSaQJVaxNr"
wearables[3].category = "hair"
wearables[3].contentName = "avatarassets/m_hair_punk_01.glb"
// wearables[3].contentName = "QmZQPNWdBc1wtcRtknuRvPJdKkTwPFW2K76VimfF9MeX3s"
wearables[4].category = "lower_body"
wearables[4].contentName = "avatarassets/m_lbody_greyjoggers.glb"
// wearables[4].contentName = "QmWNqkuhiCL6YHRvbwYniwqYPC6yvm62cHyky8eUaP2aRM"
wearables[5].category = "upper_body"
wearables[5].contentName = "avatarassets/m_ubody_greentshirt.glb"
// wearables[5].contentName = "QmfKssVuMmVWiF1Qy7xrNUdukinmfoovsCGNnuEJVczWEs"
avatarshape.wearables = wearables

const skin = new Skin()
skin.color = new Color4(0.6683928370475769,0.9275454878807068,0.8263601064682007,1.0)
avatarshape.skin = skin

const hair = new Hair()
hair.color = new Color4(0.47648438811302187,0.22779148817062379,0.5416662096977234,1.0)
avatarshape.hair = hair

const eyes = new Eyes()
eyes.color = Color4.Red()
// eyes.texture = "avatarassets/eyes_texture.png"
eyes.texture = "QmUr2oKJ3NEXsnbwU6dpbSwJQqBU3ABAa3jdgmXn5sVonx"
// eyes.mask = "avatarassets/eyes_mask.png"
eyes.mask = "QmdUoZqYJQyNYVWgEqzNUdeUZtQ9KMtceDFboGQ8EqoWo7"
avatarshape.eyes = eyes

const eyebrows = new Face()
// eyebrows.texture = "avatarassets/eyebrows_texture.png"
eyebrows.texture = "QmdMRHXcGHJgRiwWvrEscS7KuVE491WgEKGHGvm3tSZSgS"
avatarshape.eyebrows = eyebrows

const mouth = new Face()
// mouth.texture = "avatarassets/mouth_texture.png"
mouth.texture = "QmQvzorahi1Ew7epDqmpkcnRd986RVVJ49agBpVeo5YQcW"
avatarshape.mouth = mouth

avatarEntity.addComponentOrReplace(avatarshape)
engine.addEntity(avatarEntity)
