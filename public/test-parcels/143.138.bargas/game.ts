import { Entity, GLTFShape, engine, Vector3, Transform, Quaternion } from 'decentraland-ecs/src'

let entity = new Entity()

let shape = new GLTFShape('models/Monument_Mati.gltf')

let transform = new Transform()
transform.position = new Vector3(10, 0, 5)
transform.rotation = Quaternion.Euler(0, 90, 0)
transform.scale = Vector3.One()

entity.addComponentOrReplace(shape)
entity.addComponentOrReplace(transform)

engine.addEntity(entity)
