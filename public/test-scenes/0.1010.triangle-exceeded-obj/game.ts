import { Entity, engine, Vector3, Transform, OBJShape } from 'decentraland-ecs/src'

function makeOBJ(src: string, position: Vector3, scale: Vector3) {
  const ent = new Entity()
  ent.addComponentOrReplace(new OBJShape(src))
  ent.addComponentOrReplace(
    new Transform({
      position,
      scale
    })
  )
  engine.addEntity(ent)
  return ent
}

makeOBJ('models/Forest/Forest_10x10.obj', new Vector3(10, 0, 0), new Vector3(0.5, 0.5, 0.5))
