import { Component, ISystem, Transform, engine, Entity, BoxShape, Vector3 } from 'decentraland-ecs'

const boxShape = new BoxShape()

@Component('velocity')
export class Velocity extends Vector3 {
  constructor(x: number, y: number, z: number) {
    super(x, y, z)
  }
}

export class MovementSystem implements ISystem {
  group = engine.getComponentGroup(Transform, Velocity)

  update(dt: number) {
    for (let entity of this.group.entities) {
      const transform = entity.getComponent(Transform)
      const velocity = entity.getComponent(Velocity)

      transform.position.x += velocity.x * dt
      transform.position.y += velocity.y * dt
      transform.position.z += velocity.z * dt

      if (
        transform.position.y > 14 ||
        transform.position.x > 9 ||
        transform.position.z > 9 ||
        transform.position.x < 1 ||
        transform.position.z < 1
      ) {
        engine.removeEntity(entity)
        spawn()
      }
    }
  }
}

function spawn() {
  const cube = new Entity()
  const transform = new Transform()
  transform.position.set(Math.random() * 8 + 1, Math.random() * 4 + 1, Math.random() * 8 + 1)

  cube.addComponentOrReplace(transform)
  cube.addComponentOrReplace(new Velocity((Math.random() - Math.random()) / 10, Math.random() / 10, (Math.random() - Math.random()) / 10))
  cube.addComponentOrReplace(boxShape)

  engine.addEntity(cube)
}

engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())
engine.addSystem(new MovementSystem())

for (let i = 0; i < 50; i++) {
  spawn()
}
