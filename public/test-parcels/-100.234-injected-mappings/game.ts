import { engine, Entity, GLTFShape, Transform } from 'decentraland-ecs'

const shape = new GLTFShape('AnimatedCube.gltf')

const gltf = new Entity()
const transform = new Transform()

transform.position.set(5, 1, 5)

gltf.add(transform)
gltf.add(shape)

engine.addEntity(gltf)
