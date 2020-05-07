import { Entity, engine, Transform, Vector3, TextShape, Font, Fonts } from 'decentraland-ecs/src'

const e1 = new Entity()
const e2 = new Entity()
const e3 = new Entity()

const text1 = new TextShape('SanFrancisco')
text1.font = new Font(Fonts.SanFrancisco)

const text2 = new TextShape('SanFrancisco_Heavy')
text2.font = new Font(Fonts.SanFrancisco_Heavy)

const text3 = new TextShape('SanFrancisco_Semibold')
text3.font = new Font(Fonts.SanFrancisco_Semibold)

e1.addComponent(new Transform({ position: new Vector3(8, 1, 8) }))
e2.addComponent(new Transform({ position: new Vector3(8, 2, 8) }))
e3.addComponent(new Transform({ position: new Vector3(8, 3, 8) }))

e1.addComponent(text1)
e2.addComponent(text2)
e3.addComponent(text3)

engine.addEntity(e1)
engine.addEntity(e2)
engine.addEntity(e3)
