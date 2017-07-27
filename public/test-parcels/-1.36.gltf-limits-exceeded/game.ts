import { Entity, GLTFShape, engine, Vector3, Transform } from 'decentraland-ecs/src'

function makeGLTF(src: string, position: Vector3, scale: Vector3) {
  const ent = new Entity()
  ent.set(new GLTFShape(src))
  ent.set(
    new Transform({
      position,
      scale
    })
  )
  engine.addEntity(ent)
  return ent
}

makeGLTF('models/Suzanne/Suzanne.gltf', new Vector3(5, 5, 5), new Vector3(1, 1, 1))
makeGLTF('models/Suzanne/Suzanne.gltf', new Vector3(5, 5, 5), new Vector3(1, 1, 1))
makeGLTF('models/Suzanne/Suzanne.gltf', new Vector3(5, 5, 5), new Vector3(1, 1, 1))
makeGLTF('models/Suzanne/Suzanne.gltf', new Vector3(5, 5, 5), new Vector3(1, 1, 1))
