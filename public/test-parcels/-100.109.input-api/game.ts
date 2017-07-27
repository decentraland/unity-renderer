import {
  engine,
  Input,
  Entity,
  Transform,
  ISystem,
  BasicMaterial,
  PlaneShape,
  Component,
  Vector3,
  Pointer,
  OnClick,
  Billboard
} from 'decentraland-ecs/src'

declare var dcl: any

const input = Input.instance

dcl.input = input

@Component('velocity')
export class Velocity extends Vector3 {
  constructor(x: number, y: number, z: number) {
    super(x, y, z)
  }
}

const bubbleMaterial = new BasicMaterial()
bubbleMaterial.texture = 'bubble.png'
bubbleMaterial.samplingMode = 1

const spawner = {
  MAX_POOL_SIZE: 20,
  pool: [] as Entity[],

  getEntityFromPool(): Entity | null {
    for (let i = 0; i < spawner.pool.length; i++) {
      if (!spawner.pool[i].alive) {
        return spawner.pool[i]
      }
    }

    if (spawner.pool.length < spawner.MAX_POOL_SIZE) {
      const instance = new Entity()
      spawner.pool.push(instance)
      return instance
    }

    return null
  },

  spawnBubble() {
    const ent = spawner.getEntityFromPool()

    if (!ent) return

    const newVel = {
      x: (Math.random() - Math.random()) / 2,
      y: Math.random() / 2,
      z: (Math.random() - Math.random()) / 2
    }

    if (!ent.getOrNull(OnClick)) {
      ent.set(
        new OnClick(() => {
          engine.removeEntity(ent)
        })
      )
    }

    if (!ent.getOrNull(PlaneShape)) {
      const shape = new PlaneShape()
      ent.set(new Billboard())
      ent.set(shape)
    }

    if (!ent.getOrNull(BasicMaterial)) {
      ent.set(bubbleMaterial)
    }

    if (!ent.getOrNull(Transform)) {
      const t = new Transform()
      ent.set(t)
      t.scale.set(0.3, 0.3, 0.3)
      t.position.set(5, 0, 5)
    } else {
      const t = ent.get(Transform)
      t.position.set(5, 0, 5)
    }

    if (!ent.getOrNull(Velocity)) {
      ent.set(new Velocity(newVel.x, newVel.y, newVel.z))
    } else {
      const vel = ent.get(Velocity)
      vel.set(newVel.x, newVel.y, newVel.z)
    }

    engine.addEntity(ent)
  }
}

class BubbleSystem implements ISystem {
  group = engine.getComponentGroup(Transform, Velocity, OnClick)

  isOutOfBounds(transform: Transform) {
    if (
      transform.position.y > 14 ||
      transform.position.x > 9 ||
      transform.position.z > 9 ||
      transform.position.x < 1 ||
      transform.position.z < 1
    ) {
      return true
    }
    return false
  }

  update(dt: number) {
    for (let entity of this.group.entities) {
      const transform = entity.get(Transform)
      const velocity = entity.get(Velocity)

      transform.position.x += velocity.x * dt
      transform.position.y += velocity.y * dt
      transform.position.z += velocity.z * dt

      if (this.isOutOfBounds(transform)) {
        engine.removeEntity(entity)
      }
    }

    if (input.state[Pointer.PRIMARY].BUTTON_A_DOWN) {
      spawner.spawnBubble()
    }
  }
}

input.subscribe('BUTTON_A_UP', e => {
  console['log']('pointerUp works', e)
})

input.subscribe('BUTTON_A_DOWN', e => {
  console['log']('pointerDown works', e)
})

engine.addSystem(new BubbleSystem())
