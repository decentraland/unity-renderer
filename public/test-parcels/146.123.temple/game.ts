import { Entity, GLTFShape, engine, Vector3, Transform, Quaternion } from 'decentraland-ecs/src'

let entity = new Entity()

let shape = new GLTFShape('models/Temple.gltf')
entity.addComponentOrReplace(shape)

let transform = new Transform()
transform.position = new Vector3(20, 0, 20)
transform.rotation = Quaternion.Euler(0, 90, 0)
entity.addComponentOrReplace(transform)

engine.addEntity(entity)
