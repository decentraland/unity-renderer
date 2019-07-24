import { NFTShape, Entity, engine, Transform, Vector3, Color3 } from 'decentraland-ecs/src'

const entity = new Entity()
const shapeComponent = new NFTShape('ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536', Color3.Green())
entity.addComponent(shapeComponent)
entity.addComponent(
  new Transform({
    position: new Vector3(4, 1.5, 4)
  })
)
engine.addEntity(entity)
