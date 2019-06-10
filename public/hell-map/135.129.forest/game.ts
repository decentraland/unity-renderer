import { Entity, GLTFShape, engine, Vector3, Transform } from 'decentraland-ecs/src'

let entity = new Entity()

let shape = new GLTFShape('models/Forest_30-01-19.gltf')
entity.addComponentOrReplace(shape)

let transform = new Transform()
transform.position = new Vector3(15, 0, 15)
entity.addComponentOrReplace(transform)

engine.addEntity(entity)
