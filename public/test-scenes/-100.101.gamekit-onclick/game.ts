import { IEntity, ISystem, Transform, engine, Entity, BoxShape, EventManager, EventConstructor } from 'decentraland-ecs'

const boxShape = new BoxShape()
const events = new EventManager()

@EventConstructor()
class UpdateEvent {
  constructor(public entity: IEntity, public dt: number) {}
}

export class RotatorSystem implements ISystem {
  group = engine.getComponentGroup(Transform)

  update(dt: number) {
    for (let entity of this.group.entities) {
      events.fireEvent(new UpdateEvent(entity, dt))
    }
  }
}

events.addListener(UpdateEvent, null, ({ entity, dt }) => {
  const transform = entity.getComponent(Transform)
  const euler = transform.rotation.eulerAngles
  euler.y += dt * 10
  transform.rotation.eulerAngles = euler
})

const cube = new Entity()
const transform = new Transform()

cube.addComponentOrReplace(transform)
transform.position.set(5, 0, 5)

cube.addComponentOrReplace(boxShape)

engine.addEntity(cube)

engine.addSystem(new RotatorSystem())
