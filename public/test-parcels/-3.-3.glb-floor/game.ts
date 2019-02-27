import { Entity, engine, GLTFShape, Transform } from 'decentraland-ecs/src'

const floor = new GLTFShape('floor.glb')

{
  const ent = new Entity()
  engine.addEntity(ent)
  ent.getComponentOrCreate(Transform).position.set(8, 0, 8)
  ent.addComponentOrReplace(floor)
}

{
  const ent = new Entity()
  engine.addEntity(ent)
  ent.getComponentOrCreate(Transform).position.set(24, 0, 8)
  ent.addComponentOrReplace(floor)
}

{
  const ent = new Entity()
  engine.addEntity(ent)
  ent.getComponentOrCreate(Transform).position.set(8, 0, 24)
  ent.addComponentOrReplace(floor)
}

{
  const ent = new Entity()
  engine.addEntity(ent)
  ent.getComponentOrCreate(Transform).position.set(24, 0, 24)
  ent.addComponentOrReplace(floor)
}
