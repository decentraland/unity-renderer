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
  ActionButton,
  OnPointerDown,
  Billboard,
  log,
  Texture
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
bubbleMaterial.texture = new Texture('bubble.png', { samplingMode: 1 })

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

    if (!ent.getComponentOrNull(OnPointerDown)) {
      ent.addComponentOrReplace(
        new OnPointerDown(() => {
          engine.removeEntity(ent)
        })
      )
    }

    if (!ent.getComponentOrNull(PlaneShape)) {
      const shape = new PlaneShape()
      ent.addComponentOrReplace(new Billboard())
      ent.addComponentOrReplace(shape)
    }

    if (!ent.getComponentOrNull(BasicMaterial)) {
      ent.addComponentOrReplace(bubbleMaterial)
    }

    if (!ent.getComponentOrNull(Transform)) {
      const t = new Transform()
      ent.addComponentOrReplace(t)
      t.scale.set(0.3, 0.3, 0.3)
      t.position.set(5, 0, 5)
    } else {
      const t = ent.getComponent(Transform)
      t.position.set(5, 0, 5)
    }

    if (!ent.getComponentOrNull(Velocity)) {
      ent.addComponentOrReplace(new Velocity(newVel.x, newVel.y, newVel.z))
    } else {
      const vel = ent.getComponent(Velocity)
      vel.set(newVel.x, newVel.y, newVel.z)
    }

    engine.addEntity(ent)
  }
}

class BubbleSystem implements ISystem {
  group = engine.getComponentGroup(Transform, Velocity, OnPointerDown)

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
      const transform = entity.getComponent(Transform)
      const velocity = entity.getComponent(Velocity)

      transform.position.x += velocity.x * dt
      transform.position.y += velocity.y * dt
      transform.position.z += velocity.z * dt

      if (this.isOutOfBounds(transform)) {
        engine.removeEntity(entity)
      }
    }

    if (input.isButtonPressed(ActionButton.POINTER)) {
      spawner.spawnBubble()
    }
  }
}

input.subscribe('BUTTON_UP', ActionButton.PRIMARY, true, e => {
  log('pointerUp works', e)

})

input.subscribe('BUTTON_DOWN', ActionButton.PRIMARY, true, e => {
  log('pointerDown works', e)
})

engine.addSystem(new BubbleSystem())

dcl.onEvent(function (event: any) {
  log('event', event)
})

log('init')
