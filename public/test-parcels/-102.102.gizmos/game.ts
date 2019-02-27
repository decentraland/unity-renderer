import { Entity, engine, BoxShape, Gizmos, Transform, OnPointerDown, log, OnGizmoEvent } from 'decentraland-ecs/src'

const shape = new BoxShape()
shape.visible = true

let z = 1

function createCube(i: number) {
  const cube = new Entity()
  cube.addComponent(shape)

  cube.getComponentOrCreate(Transform).position.z = z += 0.5
  cube.getComponentOrCreate(Transform).position.y = 1
  cube.getComponentOrCreate(Transform).position.x = i & 8 ? 1 : 5
  cube.getComponentOrCreate(Transform).scale.z = 0.4
  cube.getComponentOrCreate(Transform).scale.y = 0.4
  cube.getComponentOrCreate(Transform).scale.x = 0.4

  const gizmo = new Gizmos()
  gizmo.position = !!(i & 1)
  gizmo.scale = !!(i & 2)
  gizmo.rotation = !!(i & 4)
  gizmo.cycle = !!(i & 8)

  cube.addComponentOrReplace(gizmo)
  cube.addComponentOrReplace(
    new OnGizmoEvent(evt => {
      log(evt)
      if (evt.type === 'gizmoDragEnded') {
        // stub
      }
    })
  )

  engine.addEntity(cube)
  return cube
}

for (let i = 0; i < 16; i++) {
  createCube(i).addComponentOrReplace(new OnPointerDown(e => log(`click on ${i}`, e)))
}

log('initialized the cubes')
