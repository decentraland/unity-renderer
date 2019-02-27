import { engine, Entity, BoxShape, Vector3, Transform, OnPointerDown, log } from 'decentraland-ecs'

log('start')

const cube = new Entity()

cube.getComponentOrCreate(Transform)
cube.getComponentOrCreate(BoxShape)

engine.addEntity(cube)

const onClickComponent = new OnPointerDown(event => {
  const t = cube.getComponent(Transform)
  t.rotate(Vector3.Up(), 30)
  log(JSON.stringify(event))
})

cube.addComponent(onClickComponent)
