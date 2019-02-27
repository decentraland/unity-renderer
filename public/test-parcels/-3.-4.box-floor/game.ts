import { Entity, Transform, engine, BoxShape } from 'decentraland-ecs/src'

const floor = new BoxShape()

{
  const ent = new Entity()
  engine.addEntity(ent)
  ent.getComponentOrCreate(Transform).position.set(8, 0, 8)
  ent.getComponentOrCreate(Transform).scale.set(16, 0.1, 16)
  ent.addComponentOrReplace(floor)
}

{
  const ent = new Entity()
  engine.addEntity(ent)
  ent.getComponentOrCreate(Transform).position.set(24, 0, 8)
  ent.getComponentOrCreate(Transform).scale.set(16, 0.1, 16)
  ent.addComponentOrReplace(floor)
}

{
  const ent = new Entity()
  engine.addEntity(ent)
  ent.getComponentOrCreate(Transform).position.set(8, 0, 24)
  ent.getComponentOrCreate(Transform).scale.set(16, 0.1, 16)
  ent.addComponentOrReplace(floor)
}

{
  const ent = new Entity()
  engine.addEntity(ent)
  ent.getComponentOrCreate(Transform).position.set(24, 0, 24)
  ent.getComponentOrCreate(Transform).scale.set(16, 0.1, 16)
  ent.addComponentOrReplace(floor)
}
