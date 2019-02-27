import { Entity, GLTFShape, engine, Vector3, Transform } from 'decentraland-ecs/src'

CreateMesh(new Vector3(1.2, 0, 0.5))
CreateMesh(new Vector3(4.2, 0, 0.5))
CreateMesh(new Vector3(7.2, 0, 0.5))

CreateMesh(new Vector3(8.8, 0, 3))
CreateMesh(new Vector3(5.8, 0, 3))
CreateMesh(new Vector3(2.8, 0, 3))

CreateMesh(new Vector3(1.2, 0, 7))
CreateMesh(new Vector3(4.2, 0, 7))

function CreateMesh(pos: Vector3) {
  const entity = new Entity()
  entity.addComponent(new GLTFShape('models/test.gltf'))
  entity.addComponent(new Transform({ position: pos }))
  engine.addEntity(entity)
}
