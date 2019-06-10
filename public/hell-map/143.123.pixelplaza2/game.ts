import { Entity, GLTFShape, engine, Vector3, Transform } from 'decentraland-ecs/src'

let entity = new Entity()

let shape = new GLTFShape('models/PixelPlaza2.gltf')
entity.addComponentOrReplace(shape)

let transform = new Transform()
transform.position = new Vector3(10, -0.04, 10)
entity.addComponentOrReplace(transform)

engine.addEntity(entity)
