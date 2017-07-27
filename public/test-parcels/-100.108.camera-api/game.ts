import { engine, Camera, Entity, BoxShape, Transform, Quaternion } from 'decentraland-ecs/src'

declare var dcl: any

const camera = Camera.instance

dcl.camera = camera

const entitiesWithTransform = engine.getComponentGroup(Transform)

class SomeSystem {
  update() {
    for (let entity of entitiesWithTransform.entities) {
      const transform = entity.get(Transform)
      transform.rotation.copyFrom(Quaternion.Inverse(camera.rotation))
    }
  }
}

const cube = new Entity()
const shape = new BoxShape()
const transform = new Transform()
transform.position.set(5, 1, 5)
cube.set(shape)
cube.set(transform)

engine.addEntity(cube)
engine.addSystem(new SomeSystem())
