import {
  engine,
  PlaneShape,
  Entity,
  Transform,
  BoxShape,
  CylinderShape,
  ConeShape,
  SphereShape
} from 'decentraland-ecs/src'

const shapes = [PlaneShape, BoxShape, CylinderShape, ConeShape, SphereShape]

shapes.forEach((Shape, i) => {
  const ent = new Entity()
  const transform = new Transform()
  const shape = new Shape()
  transform.position.set(2 * i + 1, 1, 5)
  ent.addComponentOrReplace(transform)
  ent.addComponentOrReplace(shape)
  engine.addEntity(ent)
})
