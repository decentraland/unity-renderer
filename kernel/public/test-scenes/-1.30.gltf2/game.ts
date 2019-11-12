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

makeGLTF('models/BoomBoxWithAxes/BoomBoxWithAxes.gltf', new Vector3(5, 1, 5), new Vector3(50, 50, 50))

makeGLTF('models/Lantern.glb', new Vector3(1.5, 0, 10), new Vector3(0.3, 0.3, 0.3))

makeGLTF('models/Lantern.glb', new Vector3(1.5, 0, 13), new Vector3(0.3, 0.3, 0.3))

makeGLTF('models/Lantern/Lantern.gltf', new Vector3(1.5, 0, 20), new Vector3(0.3, 0.3, 0.3))
