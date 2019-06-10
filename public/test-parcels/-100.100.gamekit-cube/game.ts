import {
  Transform,
  engine,
  Entity,
  BoxShape,
  ISystem,
  log,
  OnPointerDown,
  Material,
  Color3,
  MessageBus
} from 'decentraland-ecs/src'

export class RotatorSystem implements ISystem {
  group = engine.getComponentGroup(Transform)

  update(dt: number) {
    for (let entity of this.group.entities) {
      const transform = entity.getComponent(Transform)
      const euler = transform.rotation.eulerAngles
      euler.y += 2 * dt * 13
      euler.x += 2 * dt * 17
      euler.z += 2 * dt * 11
      transform.rotation.eulerAngles = euler
      // TODO: ECS this doesnt work as expected
    }
  }
}

const bus = new MessageBus()

log('hello a a s')

const cube = new Entity()
const material = new Material()
material.emissiveColor = Color3.Yellow()

cube.addComponentOrReplace(material)
cube.addComponentOrReplace(new Transform())
cube.getComponent(Transform).position.set(8, 1, 8)
cube.addComponentOrReplace(new BoxShape())

bus.on('click', (evt, sender) => {
  log('cubeClick1', evt, sender)
  if (cube.hasComponent(Transform)) {
    // this will place the entity at the scene origin (out of bounds)
    cube.removeComponent(Transform)
  } else {
    cube.getComponentOrCreate(Transform).rotation.set(0, 0, 0, 1)
  }
})

cube.addComponentOrReplace(new OnPointerDown(e => bus.emit('click', e)))

engine.addEntity(cube)

engine.addSystem(new RotatorSystem())

declare var dcl: any

dcl.onEvent(function(event: any) {
  log('event', event)
})
