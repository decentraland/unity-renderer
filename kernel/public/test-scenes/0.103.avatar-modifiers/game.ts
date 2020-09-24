import { Entity, engine, Vector3, Transform, BoxShape, AvatarModifierArea, AvatarModifiers, TextShape, Billboard, Quaternion } from 'decentraland-ecs/src'

const SIZE = 4

// HIDE_AVATARS
const hideAvatarsEntity = new Entity()
hideAvatarsEntity.addComponent(new AvatarModifierArea({ area: { box: new Vector3(SIZE, SIZE, SIZE) }, modifiers: [AvatarModifiers.HIDE_AVATARS] }))
hideAvatarsEntity.addComponent(new Transform({ position: new Vector3(5, SIZE / 2, 8), rotation: new Quaternion(0, 0.5) }))
hideAvatarsEntity.addComponent(new TextShape('Invisible'))
hideAvatarsEntity.addComponent(new Billboard(true, true, true))
engine.addEntity(hideAvatarsEntity)

const box1 = new Entity()
const shape1 = new BoxShape()
shape1.withCollisions = false
box1.addComponent(shape1)
box1.addComponent(new Transform({ position: new Vector3(5, 0.125, 8), scale: new Vector3(SIZE, 0.25, SIZE), rotation: new Quaternion(0, 0.5) }))
engine.addEntity(box1)

// DISABLE_PASSPORTS
const disablePassportsEntity = new Entity()
disablePassportsEntity.addComponent(new AvatarModifierArea({ area: { box: new Vector3(SIZE, SIZE, SIZE) }, modifiers: [AvatarModifiers.DISABLE_PASSPORTS] }))
disablePassportsEntity.addComponent(new Transform({ position: new Vector3(11, SIZE / 2, 8) }))
disablePassportsEntity.addComponent(new TextShape('No Passports'))
disablePassportsEntity.addComponent(new Billboard(true, true, true))
engine.addEntity(disablePassportsEntity)

const box2 = new Entity()
const shape2 = new BoxShape()
shape2.withCollisions = false
box2.addComponent(shape2)
box2.addComponent(new Transform({ position: new Vector3(11, 0.125, 8), scale: new Vector3(SIZE, 0.25, SIZE) }))
engine.addEntity(box2)

