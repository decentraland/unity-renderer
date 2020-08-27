import { BoxShape, engine, Entity, log, movePlayerTo, OnPointerDown, Transform, Vector3 } from 'decentraland-ecs/src'

const e = new Entity()
e.addComponent(new BoxShape())
e.addComponent(new Transform({ position: new Vector3(4, 0, 6) }))
e.addComponent(
  new OnPointerDown(
    (e) => {
      log('clicked: ', e)
      movePlayerTo({ x: 10, y: 0, z: 10 }, { x: 4, y: 1, z: 6 })
    },
    { hoverText: 'Move player' }
  )
)

engine.addEntity(e)
