import { NFTShape, Entity, engine, Transform, Vector3, Color3 } from 'decentraland-ecs/src'

const entity = new Entity()
const shapeComponent = new NFTShape('ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536')
entity.addComponent(shapeComponent)
entity.addComponent(
  new Transform({
    position: new Vector3(3, 1.5, 4)
  })
)
engine.addEntity(entity)

const entity2 = new Entity()
const shapeComponent2 = new NFTShape('ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536', Color3.Green())
entity2.addComponent(shapeComponent2)
entity2.addComponent(
  new Transform({
    position: new Vector3(5, 1.5, 4)
  })
)
engine.addEntity(entity2)

const entity3 = new Entity()
const shapeComponent3 = new NFTShape('ethereum://0xb932a70a57673d89f4acffbe830e8ed7f75fb9e0/9184', Color3.Blue())
entity3.addComponent(shapeComponent3)
entity3.addComponent(
  new Transform({
    position: new Vector3(7, 1.5, 4)
  })
)
engine.addEntity(entity3)
