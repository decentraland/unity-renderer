import { Entity, engine, Vector3, Transform, GLTFShape } from 'decentraland-ecs/src'

function makeGLTF(src: string, position: Vector3) {
  const ent = new Entity()
  ent.addComponentOrReplace(new GLTFShape(src))
  ent.addComponentOrReplace(
    new Transform({
      position
    })
  )
  engine.addEntity(ent)
  return ent
}

makeGLTF('glTF/200k.gltf', new Vector3(5, 0, 5))
