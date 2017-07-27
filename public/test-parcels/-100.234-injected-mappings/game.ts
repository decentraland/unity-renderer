import { engine, Entity, GLTFShape, Transform } from 'decentraland-ecs'

{
  const gltf = new Entity()
  ;(gltf as any).uuid = '__test_id__'
  const transform = new Transform()
  transform.position.set(5, 1, 5)

  gltf.add(transform)
  gltf.add(new GLTFShape('AnimatedCube.gltf'))

  engine.addEntity(gltf)
}
