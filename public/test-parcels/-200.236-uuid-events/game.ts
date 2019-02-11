import { engine, Entity, BoxShape, Vector3, Transform, OnPointerDown, log } from 'decentraland-ecs'

log('start')

const cube = new Entity()

cube.getOrCreate(Transform)
cube.getOrCreate(BoxShape)

engine.addEntity(cube)

const onClickComponent = new OnPointerDown(event => {
  const t = cube.get(Transform)
  t.rotate(Vector3.Up(), 30)
  log(JSON.stringify(event))
})

cube.add(onClickComponent)
