import { Entity, engine, Vector3, Transform, OBJShape, Quaternion } from 'decentraland-ecs/src'

function makeOBJ(src: string, position: Vector3, rotation: Vector3, scale: Vector3) {
  const ent = new Entity()
  ent.addComponentOrReplace(new OBJShape(src))
  ent.addComponentOrReplace(
    new Transform({
      position,
      rotation: Quaternion.Euler(rotation.x, rotation.y, rotation.z),
      scale
    })
  )
  engine.addEntity(ent)
  return ent
}

makeOBJ('Forest10x10.obj', new Vector3(-5, 0, -5), new Vector3(0, 90, 0), new Vector3(0.5, 0.5, 0.5))
makeOBJ('Forest10x10.obj', new Vector3(5, 0, -5), new Vector3(0, 90, 0), new Vector3(0.5, 0.5, 0.5))
makeOBJ('Forest10x10.obj', new Vector3(10, 0, -5), new Vector3(0, 90, 0), new Vector3(0.5, 0.5, 0.5))

makeOBJ('Forest10x10.obj', new Vector3(-5, 0, 5), new Vector3(0, 90, 0), new Vector3(0.5, 0.5, 0.5))
makeOBJ('Forest10x10.obj', new Vector3(5, 0, 5), new Vector3(0, 90, 0), new Vector3(0.5, 0.5, 0.5))
makeOBJ('Forest10x10.obj', new Vector3(10, 0, 5), new Vector3(0, 90, 0), new Vector3(0.5, 0.5, 0.5))

makeOBJ('Forest10x10.obj', new Vector3(-5, 0, 15), new Vector3(0, 90, 0), new Vector3(0.5, 0.5, 0.5))
makeOBJ('Forest10x10.obj', new Vector3(5, 0, 15), new Vector3(0, 90, 0), new Vector3(0.5, 0.5, 0.5))
makeOBJ('Forest10x10.obj', new Vector3(10, 0, 15), new Vector3(0, 90, 0), new Vector3(0.5, 0.5, 0.5))
