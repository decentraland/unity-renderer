import { Entity, engine, Vector3, Transform, GLTFShape } from 'decentraland-ecs/src'

function makeGLTF(src: string, position: Vector3) {
  const ent = new Entity()
  ent.set(new GLTFShape(src))
  ent.set(
    new Transform({
      position
    })
  )
  engine.addEntity(ent)
  return ent
}

makeGLTF('glTF/200k.gltf', new Vector3(5, 0, 5))
