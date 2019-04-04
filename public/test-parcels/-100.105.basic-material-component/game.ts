import { engine, PlaneShape, Entity, Transform, BasicMaterial, Texture } from 'decentraland-ecs/src'

const plane = new PlaneShape()
const niceMaterial = new BasicMaterial()

const texture = new Texture('atlas.png', { samplingMode: 0 })

niceMaterial.texture = texture

plane.uvs = [0, 0.75, 0.25, 0.75, 0.25, 1, 0, 1, 0, 0.75, 0.25, 0.75, 0.25, 1, 0, 1]

const ent = new Entity()
const transform = new Transform()
transform.position.set(8, 1, 8)

ent.addComponentOrReplace(plane)
ent.addComponentOrReplace(niceMaterial)
ent.addComponentOrReplace(transform)
engine.addEntity(ent)
