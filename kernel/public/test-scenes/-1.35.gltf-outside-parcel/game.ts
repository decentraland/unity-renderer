import { Entity, GLTFShape, engine, Vector3, Transform, OBJShape } from 'decentraland-ecs/src'

function makeGLTF(src: string, position: Vector3, scale: Vector3) {
  const ent = new Entity()
  ent.addComponentOrReplace(new GLTFShape(src))
  ent.addComponentOrReplace(
    new Transform({
      position,
      scale
    })
  )
  engine.addEntity(ent)
  return ent
}

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

makeGLTF('models/Lantern/Lantern.gltf', new Vector3(-1.5, 0, 10), new Vector3(0.3, 0.3, 0.3))

makeOBJ('models/candyCane.obj', new Vector3(-1.5, 0, 13), new Vector3(1.3, 1.3, 1.3))
