import {
  Entity,
  GLTFShape,
  engine,
  Vector3,
  Transform,
  AnimationState,
  Animator,
  Quaternion
} from 'decentraland-ecs/src'

function AddGLTF(path: string, position: Vector3, rotation: Vector3, scale?: Vector3, clip?: string) {
  let entity = new Entity()

  let shape = new GLTFShape(path)
  entity.addComponentOrReplace(shape)

  let transform = new Transform()
  transform.position = position
  transform.rotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z)

  if (scale) {
    transform.scale = scale
  } else {
    transform.scale = Vector3.One()
  }

  entity.addComponentOrReplace(transform)

  if (clip) {
    const animator = new Animator()
    let animation = new AnimationState(clip)
    animator.addClip(animation)
    entity.addComponent(animator)
    animation.play()
  }

  engine.addEntity(entity)
}

AddGLTF('models/Base.gltf', new Vector3(20, 0, 20), new Vector3(0, 270, 0), Vector3.One())
AddGLTF('models/Tree1.gltf', new Vector3(20, 0, 20), new Vector3(0, 270, 0), Vector3.One(), 'Armature_Idle')
AddGLTF('models/Tree2.gltf', new Vector3(20, 0, 20), new Vector3(0, 270, 0), Vector3.One(), 'Armature_Idle')
AddGLTF('models/Tree3.gltf', new Vector3(20, 0, 20), new Vector3(0, 270, 0), Vector3.One(), 'Armature_Idle')
AddGLTF('models/Tree4.gltf', new Vector3(20, 0, 20), new Vector3(0, 270, 0), Vector3.One(), 'Armature_Idle')
AddGLTF('models/Tree5.gltf', new Vector3(20, 0, 20), new Vector3(0, 270, 0), Vector3.One(), 'Armature_Idle')
AddGLTF('models/Water.gltf', new Vector3(20, 0, 20), new Vector3(0, 90, 0), Vector3.One())
AddGLTF('models/WaterFountain.gltf', new Vector3(20, 0, 20), new Vector3(0, 90, 0), Vector3.One())
AddGLTF('models/Fire.gltf', new Vector3(20, 0, 20), new Vector3(0, 90, 0), Vector3.One(), 'FireBig_Idle')
AddGLTF('models/B1.gltf', new Vector3(20, 0, 20), new Vector3(0, 90, 0), Vector3.One(), 'Armature_Idle')
AddGLTF('models/B2.gltf', new Vector3(20, 0, 20), new Vector3(0, 90, 0), Vector3.One(), 'Armature_Idle')
AddGLTF('models/B3.gltf', new Vector3(20, 0, 20), new Vector3(0, 90, 0), Vector3.One(), 'Armature_Idle')
AddGLTF('models/B4.gltf', new Vector3(20, 0, 20), new Vector3(0, 90, 0), Vector3.One(), 'Armature_Idle')
AddGLTF('models/B5.gltf', new Vector3(20, 0, 20), new Vector3(0, 90, 0), Vector3.One(), 'Armature_Idle')
AddGLTF('models/B6.gltf', new Vector3(20, 0, 20), new Vector3(0, 90, 0), Vector3.One(), 'Armature_Idle')
AddGLTF('models/B7.gltf', new Vector3(20, 0, 20), new Vector3(0, 90, 0), Vector3.One(), 'Armature_Idle')
AddGLTF('models/B8.gltf', new Vector3(20, 0, 20), new Vector3(0, 90, 0), Vector3.One(), 'Armature_Idle')
AddGLTF('models/B9.gltf', new Vector3(20, 0, 20), new Vector3(0, 90, 0), Vector3.One(), 'Armature_Idle')
AddGLTF('models/Colliders.gltf', new Vector3(20, 0, 20), new Vector3(0, 90, 0), Vector3.One())
AddGLTF('models/Ground.gltf', new Vector3(20, 0, 20), new Vector3(0, 0, 0), Vector3.One())
