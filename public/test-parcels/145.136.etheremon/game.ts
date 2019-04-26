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
    const animClip = new AnimationState(clip)
    animator.addClip(animClip)
    entity.addComponent(animator)
    animClip.play()
  }

  engine.addEntity(entity)
}

AddGLTF('models/BattleMap_15k.gltf', new Vector3(25, 0, 15.1), new Vector3(0, 270, 0))
AddGLTF(
  'models/Tekagon.gltf',
  new Vector3(23.8, 0, 15.1),
  new Vector3(0, 90, 0),
  new Vector3(0.7, 0.7, 0.7),
  'Armature_Idle'
)
AddGLTF(
  'models/Tenteink.gltf',
  new Vector3(23.8, 0, 15.1),
  new Vector3(0, 90, 0),
  new Vector3(0.7, 0.7, 0.7),
  'Armature_Idle'
)
AddGLTF('models/Trophy1.gltf', new Vector3(24.9, 0, 15.1), new Vector3(0, 90, 0), Vector3.One(), 'Armature_Idle')
AddGLTF('models/Medal3.gltf', new Vector3(24.1, 0, 15.1), new Vector3(0, 90, 0), Vector3.One(), 'Armature_Idle')
AddGLTF('models/Trophy2.gltf', new Vector3(25, 0, 15.1), new Vector3(0, 90, 0), Vector3.One(), 'Armature_Idle')
AddGLTF('models/Trophy3.gltf', new Vector3(25, 0, 15.1), new Vector3(0, 90, 0), Vector3.One(), 'Armature_Idle')
AddGLTF('models/Medal1.gltf', new Vector3(25, 0, 15.1), new Vector3(0, 90, 0), Vector3.One(), 'Armature_Idle')
AddGLTF('models/Medal2.gltf', new Vector3(25, 0, 15.1), new Vector3(0, 90, 0), Vector3.One(), 'Armature_Idle')
AddGLTF(
  'models/Coin.gltf',
  new Vector3(25, 0, 15.1),
  new Vector3(0, 90, 0),
  Vector3.One(),
  'Armature.001_Armature.001Action'
)
AddGLTF('models/Screen.gltf', new Vector3(25, 0, 15.1), new Vector3(0, 90, 0), Vector3.One(), 'Armature_Idle')
AddGLTF('models/Red_Notif.gltf', new Vector3(25, 0, 15.1), new Vector3(0, 270, 0))
