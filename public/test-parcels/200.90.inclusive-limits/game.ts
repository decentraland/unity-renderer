import { Entity, BoxShape, engine, Vector3, Transform } from 'decentraland-ecs/src'

export function makeBox(position: Vector3) {
  const ent = new Entity()
  const box = new BoxShape()
  ent.set(box)
  ent.set(
    new Transform({
      position
    })
  )
  engine.addEntity(ent)
  return ent
}

makeBox(new Vector3(0.5, 0.5, 0.5))
makeBox(new Vector3(0.5, 0.5, 9.5))
makeBox(new Vector3(9.5, 0.5, 0.5))
makeBox(new Vector3(9.5, 0.5, 9.5))
