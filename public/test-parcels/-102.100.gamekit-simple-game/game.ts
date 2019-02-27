import { Component, ISystem, engine, Entity, BoxShape, Vector3, Transform, Material, Color3 } from 'decentraland-ecs'

const G = 6.674e-11
const boxShape = new BoxShape()
const tempVec3 = new Vector3()
const material = new Material()
material.albedoColor = Color3.Black()
material.emissiveColor = Color3.White()

@Component('physics')
export class Physics {
  velocity = Vector3.Zero()
  acceleration = Vector3.Zero()

  mass: number = 0

  rigid: boolean = false
}

export class PhysicsSystem implements ISystem {
  group = engine.getComponentGroup(Transform, Physics)

  update(deltaTime: number) {
    const dt = deltaTime * 150
    for (let entity of this.group.entities) {
      const physics = entity.getComponent(Physics)

      if (physics.rigid) continue

      const transform = entity.getComponent(Transform)

      physics.acceleration.setAll(0)

      for (let e of this.group.entities) {
        if (e !== entity) {
          const positionE = e.getComponent(Transform).position
          const physicsE = e.getComponent(Physics)

          tempVec3.copyFrom(transform.position).subtractInPlace(positionE)

          const gravityForce = (G * (physicsE.mass * physics.mass)) / tempVec3.length()
          const delta = tempVec3.scale(-gravityForce)

          physics.acceleration.addInPlace(delta)
        }
      }

      physics.velocity.addInPlace(physics.acceleration.scale(dt))
      transform.position.addInPlace(physics.velocity.scale(dt))
    }
  }
}

export class BoundaryCheckSystem implements ISystem {
  group = engine.getComponentGroup(Transform)

  update() {
    for (let entity of this.group.entities) {
      const { position } = entity.getComponent(Transform)
      if (position.x > 15.5) position.x = 15.5
      if (position.y > 15.5) position.y = 15.5
      if (position.z > 15.5) position.z = 15.5
      if (position.x < 0.5) position.x = 0.5
      if (position.y < 0.5) position.y = 0.5
      if (position.z < 0.5) position.z = 0.5
    }
  }
}

function spawn(x: number, y: number, z: number, rigid: boolean) {
  const cube = new Entity()
  const physics = new Physics()
  const transform = new Transform()
  transform.position.set(x, y, z)

  physics.rigid = rigid

  physics.mass = physics.rigid ? 10000 : Math.random() * 100

  cube.addComponentOrReplace(physics)
  cube.addComponentOrReplace(transform)
  cube.addComponentOrReplace(material)

  if (physics.rigid) {
    cube.addComponentOrReplace(boxShape)
  } else {
    cube.addComponentOrReplace(boxShape)
    transform.scale.set(0.01 + physics.mass / 1000, 0.01 + physics.mass / 1000, 0.01 + physics.mass / 1000)
  }

  engine.addEntity(cube)

  physics.velocity.x = z > 5 ? 0.02 : -0.02
}

engine.addSystem(new PhysicsSystem())
engine.addSystem(new BoundaryCheckSystem())

for (let x = 0; x < 8; x++) {
  for (let z = 0; z < 8; z++) {
    spawn(Math.random() + 7.5, 5 + Math.random() / 2, Math.random() * 15 + 1, false)
  }
}

spawn(8, 5, 8, true)
