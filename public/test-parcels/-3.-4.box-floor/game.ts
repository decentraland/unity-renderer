import { Entity, Transform, engine, BoxShape } from 'decentraland-ecs/src'

const floor = new BoxShape()

{
  const ent = new Entity()
  engine.addEntity(ent)
  ent.getOrCreate(Transform).position.set(8, 0, 8)
  ent.getOrCreate(Transform).scale.set(16, 0.1, 16)
  ent.set(floor)
}

{
  const ent = new Entity()
  engine.addEntity(ent)
  ent.getOrCreate(Transform).position.set(24, 0, 8)
  ent.getOrCreate(Transform).scale.set(16, 0.1, 16)
  ent.set(floor)
}

{
  const ent = new Entity()
  engine.addEntity(ent)
  ent.getOrCreate(Transform).position.set(8, 0, 24)
  ent.getOrCreate(Transform).scale.set(16, 0.1, 16)
  ent.set(floor)
}

{
  const ent = new Entity()
  engine.addEntity(ent)
  ent.getOrCreate(Transform).position.set(24, 0, 24)
  ent.getOrCreate(Transform).scale.set(16, 0.1, 16)
  ent.set(floor)
}
