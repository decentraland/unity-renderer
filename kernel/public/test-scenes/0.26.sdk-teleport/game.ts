import { Entity, engine, Transform, Vector3, BoxShape, OnPointerDown, teleportTo } from 'decentraland-ecs/src'

const e = new Entity()
e.addComponent(new BoxShape())
e.addComponent(new Transform({ position: new Vector3(4, 0, 6) }))
e.addComponent(
  new OnPointerDown(
    () => {
      teleportTo('-149,-144')
    },
    { hoverText: '/goto -149,-144' }
  )
)

engine.addEntity(e)

const e2 = new Entity()
e2.addComponent(new BoxShape())
e2.addComponent(new Transform({ position: new Vector3(6, 0, 6) }))
e2.addComponent(
  new OnPointerDown(
    () => {
      teleportTo('magic')
    },
    { hoverText: '/goto magic' }
  )
)

engine.addEntity(e2)

const e3 = new Entity()
e3.addComponent(new BoxShape())
e3.addComponent(new Transform({ position: new Vector3(8, 0, 6) }))
e3.addComponent(
  new OnPointerDown(
    () => {
      teleportTo('crowd')
    },
    { hoverText: '/goto crowd' }
  )
)

engine.addEntity(e3)
