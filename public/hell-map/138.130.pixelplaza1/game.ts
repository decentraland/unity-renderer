import { Entity, GLTFShape, engine, Vector3, Transform } from 'decentraland-ecs/src'

let entity = new Entity()

let shape = new GLTFShape('models/PixelPlaza1.gltf')
entity.addComponentOrReplace(shape)

let transform = new Transform()
transform.position = new Vector3(10, 0, 10)
entity.addComponentOrReplace(transform)

engine.addEntity(entity)
