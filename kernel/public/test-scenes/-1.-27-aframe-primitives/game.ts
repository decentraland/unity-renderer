import {
  Vector3,
  Entity,
  Transform,
  BoxShape,
  ConeShape,
  CylinderShape,
  engine,
  PlaneShape,
  SphereShape,
  CircleShape,
  Quaternion
} from 'decentraland-ecs/src'

function makeEntity(shape: any, position: Vector3, scale?: Vector3) {
  const ent = new Entity()
  const s = new shape()
  ent.addComponentOrReplace(s)
  ent.addComponentOrReplace(
    new Transform({
      position,
      scale
    })
  )
  engine.addEntity(ent)
  return ent
}

makeEntity(BoxShape, new Vector3(0, 2, -2))
makeEntity(BoxShape, new Vector3(0, 2, 0))
makeEntity(BoxShape, new Vector3(0, 2, 2), new Vector3(1, 3, 2))

makeEntity(ConeShape, new Vector3(2, 2, -2))
makeEntity(ConeShape, new Vector3(2, 2, 0))
makeEntity(ConeShape, new Vector3(2, 2, 2), new Vector3(1, 3, 1))
makeEntity(ConeShape, new Vector3(2, 2, 12))

const c1 = makeEntity(ConeShape, new Vector3(2, 2, 4), new Vector3(1, 3, 1))
c1.getComponent(ConeShape).radiusTop = 0.5

const c2 = makeEntity(ConeShape, new Vector3(2, 2, 6), new Vector3(1, 3, 1))
c2.getComponent(ConeShape).radiusBottom = 0.5

const c3 = makeEntity(ConeShape, new Vector3(2, 2, 8))
const c3shape = c3.getComponent(ConeShape)
c3shape.segmentsRadial = 8
c3shape.radiusBottom = 0.5

const c4 = makeEntity(ConeShape, new Vector3(2, 2, 10))
const c4shape = c4.getComponent(ConeShape)
c4shape.segmentsHeight = 1

const c5 = makeEntity(ConeShape, new Vector3(2, 1, 14), new Vector3(1, 0.5, 1))
const c5shape = c5.getComponent(ConeShape)
c5shape.radiusTop = 0.5
c5shape.openEnded = true

makeEntity(CylinderShape, new Vector3(4, 2, -2))
makeEntity(CylinderShape, new Vector3(4, 2, 0))
makeEntity(CylinderShape, new Vector3(4, 2, 2), new Vector3(1, 3, 1))
makeEntity(CylinderShape, new Vector3(4, 2, 12))

const ci1 = makeEntity(CylinderShape, new Vector3(4, 2, 4), new Vector3(1, 3, 1))
ci1.getComponent(CylinderShape).radiusTop = 0.5
ci1.getComponent(CylinderShape).radiusBottom = 0.5
// TODO(ecs): cylinders shouldn't be cones

const ci2 = makeEntity(CylinderShape, new Vector3(4, 2, 6), new Vector3(1, 3, 1))
ci2.getComponent(CylinderShape).radiusBottom = 0.5
ci2.getComponent(CylinderShape).radiusTop = 0.5

const ci3 = makeEntity(CylinderShape, new Vector3(4, 2, 8))
const ci3shape = ci3.getComponent(CylinderShape)
ci3shape.segmentsRadial = 8
ci3shape.radiusBottom = 0.5
ci3shape.radiusTop = 0.5

const ci4 = makeEntity(CylinderShape, new Vector3(4, 2, 10))
const ci4shape = ci4.getComponent(CylinderShape)
ci4shape.segmentsHeight = 1

const ci5 = makeEntity(CylinderShape, new Vector3(4, 1, 14), new Vector3(1, 0.5, 1))
const ci5shape = ci5.getComponent(CylinderShape)
ci5shape.radiusTop = 0.5
ci5shape.radiusBottom = 0.5
ci5shape.openEnded = true

makeEntity(PlaneShape, new Vector3(12, 2, -2))
makeEntity(PlaneShape, new Vector3(12, 2, 0), new Vector3(1, 1, 0.5))
makeEntity(PlaneShape, new Vector3(12, 2, 2), new Vector3(0.5, 3, 1))
makeEntity(PlaneShape, new Vector3(12, 2, 4))

makeEntity(SphereShape, new Vector3(14, 2, -2))
makeEntity(SphereShape, new Vector3(14, 2, 0))
makeEntity(SphereShape, new Vector3(14, 2, 2), new Vector3(0.5, 0.5, 0.5))
makeEntity(SphereShape, new Vector3(14, 2, 4))
makeEntity(SphereShape, new Vector3(14, 2, 6))
makeEntity(SphereShape, new Vector3(14, 2, 8))

makeEntity(CircleShape, new Vector3(26, 2, -2))
makeEntity(CircleShape, new Vector3(26, 2, 0))
const circle = makeEntity(CircleShape, new Vector3(26, 2, 2))
const circletrans = circle.getComponent(Transform)
circletrans.rotation = Quaternion.Euler(60, 0, 0)
const circleshape = circle.getComponent(CircleShape)
circleshape.arc = 90

const circle2 = makeEntity(CircleShape, new Vector3(26, 2, 4))
const circle2shape = circle2.getComponent(CircleShape)
circle2shape.segments = 6
