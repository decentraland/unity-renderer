import { Entity, GLTFShape, engine, Vector3, Transform, AnimationState, Animator } from 'decentraland-ecs/src'

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

  let shape = new GLTFShape('models/test.gltf')
  entity.addComponent(shape)
  entity.addComponent(new Transform({ position: pos }))

  const animator = new Animator()
  let animClip = new AnimationState('Animation')
  animator.addClip(animClip)
  entity.addComponent(animator)

  animClip.play()

  engine.addEntity(entity)
}
