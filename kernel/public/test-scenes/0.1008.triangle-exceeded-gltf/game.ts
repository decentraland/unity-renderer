import { Entity, GLTFShape, engine, Vector3, Transform } from 'decentraland-ecs/src'

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

makeGLTF('models/Forest/Forest_20x20.gltf', new Vector3(10, 0, 0), new Vector3(0.5, 0.5, 0.5))
