import { engine, PlaneShape, Entity, Transform, BasicMaterial } from 'decentraland-ecs/src'

const plane = new PlaneShape()
const niceMaterial = new BasicMaterial()

niceMaterial.texture = 'atlas.png'
niceMaterial.samplingMode = 0
plane.uvs = [0, 0.75, 0.25, 0.75, 0.25, 1, 0, 1, 0, 0.75, 0.25, 0.75, 0.25, 1, 0, 1]

const ent = new Entity()
const transform = new Transform()
transform.position.set(5, 1, 5)

ent.set(plane)
ent.set(niceMaterial)
ent.set(transform)
engine.addEntity(ent)
