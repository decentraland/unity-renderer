import {
  Transform,
  engine,
  Entity,
  BoxShape,
  ISystem,
  log,
  OnPointerDown,
  Material,
  Color3
} from 'decentraland-ecs/src'

export class RotatorSystem implements ISystem {
  group = engine.getComponentGroup(Transform)

  update(dt: number) {
    for (let entity of this.group.entities) {
      const transform = entity.get(Transform)
      const euler = transform.rotation.eulerAngles
      euler.y += 2 * dt * 13
      euler.x += 2 * dt * 17
      euler.z += 2 * dt * 11
      transform.rotation.eulerAngles = euler
      // TODO: ECS this doesnt work as expected
    }
  }
}

const cube = new Entity()
const material = new Material()
material.emissiveColor = Color3.Yellow()

cube.set(material)
cube.set(new Transform())
cube.get(Transform).position.set(8, 1, 8)
cube.set(new BoxShape())

cube.set(
  new OnPointerDown(evt => {
    log('cubeClick', evt)
    if (cube.has(Transform)) {
      // this will place the entity at the scene origin (out of bounds)
      cube.remove(Transform)
    } else {
      cube.getOrCreate(Transform).rotation.set(0, 0, 0, 1)
    }
  })
)

engine.addEntity(cube)

engine.addSystem(new RotatorSystem())

declare var dcl: any

dcl.onEvent(function(event: any) {
  log('event', event)
})
