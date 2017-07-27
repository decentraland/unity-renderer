import { engine, Entity, Camera, ConeShape, Transform, Vector3, Quaternion } from 'decentraland-ecs'

const camera = Camera.instance

const wrapper = new Entity()
wrapper.set(
  new Transform({
    position: new Vector3(5, 1, 5)
  })
)

const cube = new Entity()
cube.set(new ConeShape())
cube.set(
  new Transform({
    rotation: Quaternion.Euler(90, 0, 0)
  })
)
engine.addEntity(wrapper)
engine.addEntity(cube)

cube.setParent(wrapper)

class LookerSystem {
  update() {
    const t = wrapper.get(Transform)
    t.lookAt(camera.position)
  }
}

engine.addSystem(new LookerSystem())
