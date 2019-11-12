import {
  Component,
  ISystem,
  engine,
  Entity,
  BoxShape,
  Vector3,
  Transform,
  Material,
  Color3,
  MessageBus,
  ReadOnlyVector3,
  Input,
  ActionButton
} from 'decentraland-ecs/src'

const G = 6.674e-11
const boxShape = new BoxShape()
const tempVec3 = new Vector3()
const material = new Material()
const material2 = new Material()
material.albedoColor = Color3.Black()
material.emissiveColor = Color3.White()

material2.albedoColor = Color3.Red()

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

          const l = tempVec3.length()
          if (l !== 0) {
            const gravityForce = (G * (physicsE.mass * physics.mass)) / tempVec3.length()
            const delta = tempVec3.scale(-gravityForce)

            physics.acceleration.addInPlace(delta)
          }
        }
      }
      transform.lookAt(transform.position.add(physics.velocity))
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

function spawnSun(x: number, y: number, z: number) {
  const cube = new Entity()
  const physics = new Physics()
  const transform = new Transform()
  transform.position.set(x, y, z)

  physics.rigid = true

  physics.mass = 300

  cube.addComponentOrReplace(physics)
  cube.addComponentOrReplace(transform)
  cube.addComponentOrReplace(material)

  cube.addComponentOrReplace(boxShape)

  engine.addEntity(cube)
}

engine.addSystem(new PhysicsSystem())
engine.addSystem(new BoundaryCheckSystem())

spawnSun(8, 3, 8)

type SpawnEvent = {
  pos: ReadOnlyVector3
  vel: ReadOnlyVector3
  mass: number
}

const messageBus = new MessageBus()

messageBus.on('spawn', (evt: SpawnEvent) => {
  const { pos, vel, mass } = evt
  const cube = new Entity()
  const physics = new Physics()
  const transform = new Transform()
  transform.position.copyFrom(pos)

  physics.rigid = false
  physics.velocity.copyFrom(vel)
  physics.mass = mass

  cube.addComponentOrReplace(physics)
  cube.addComponentOrReplace(transform)
  cube.addComponentOrReplace(material2)
  cube.addComponentOrReplace(boxShape)
  transform.scale.set(physics.mass / 2000, physics.mass / 2000, 0.01 + physics.mass / 1000)

  engine.addEntity(cube)
})

Input.instance.subscribe('BUTTON_DOWN', ActionButton.POINTER, true, event => {
  const spawn: SpawnEvent = {
    pos: event.origin,
    vel: event.direction.scale(0.015),
    mass: 80 + Math.random() * 50 + (Math.random() > 0.9 ? Math.random() * 400 : 0)
  }

  messageBus.emit('spawn', spawn)
})

function infinidesimalRandom() {
  return (Math.random() - Math.random()) * 0.1
}

for (let i = 0; i < 10; i++) {
  const spawn: SpawnEvent = {
    pos: new Vector3(8 + infinidesimalRandom(), 8 + infinidesimalRandom(), 8 + infinidesimalRandom()),
    vel: new Vector3(Math.random() - Math.random(), Math.random() - Math.random(), Math.random() - Math.random())
      .normalize()
      .scale(0.05)
      .multiplyByFloats(1, 0.3, 1),
    mass: 80 + Math.random() * 50
  }

  messageBus.emit('spawn', spawn)
}
