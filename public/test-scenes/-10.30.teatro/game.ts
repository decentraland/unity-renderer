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

makeGLTF('models/Theatre/Theatre_01.glb', new Vector3(0, 0, 0), new Vector3(1, 1, 1))
makeGLTF('models/Theatre.glb', new Vector3(10, 0, 0), new Vector3(1, 1, 1))
makeGLTF('models/Theatre_gltf/Theatre.gltf', new Vector3(-10, 0, 10), new Vector3(1, 1, 1))
