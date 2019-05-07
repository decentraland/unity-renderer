import { NFTShape, Entity, engine, Transform, Vector3 } from 'decentraland-ecs/src'

const entity = new Entity()
entity.addComponent(new NFTShape('ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536'))
entity.addComponent(
  new Transform({
    position: new Vector3(4, 2, 4)
  })
)
engine.addEntity(entity)

const entity2 = new Entity()
entity2.addComponent(new NFTShape('ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536'))
entity2.addComponent(
  new Transform({
    position: new Vector3(5, 2, 5)
  })
)
engine.addEntity(entity2)
