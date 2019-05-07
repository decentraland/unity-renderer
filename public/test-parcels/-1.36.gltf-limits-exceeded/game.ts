import { Entity, GLTFShape, engine, Vector3, Transform, log } from 'decentraland-ecs/src'

let totalTime = 0

class AWaiterSystem {
  static instance = new AWaiterSystem()

  update(dt: number) {
    totalTime += dt

    if (totalTime > 3) {
      engine.removeSystem(AWaiterSystem.instance)
      log('adding models')
      makeGLTF('models/Suzanne/Suzanne.gltf', new Vector3(5, 5, 2), new Vector3(1, 1, 1))
      makeGLTF('models/Suzanne/Suzanne.gltf', new Vector3(5, 5, 5), new Vector3(1, 1, 1))
      makeGLTF('models/Suzanne/Suzanne.gltf', new Vector3(5, 5, 7), new Vector3(1, 1, 1))
    }
  }
}

engine.addSystem(AWaiterSystem.instance)

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

engine.addEntity(new Entity())
