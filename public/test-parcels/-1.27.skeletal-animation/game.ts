import { Entity, GLTFShape, engine, Vector3, Transform, AnimationClip, Animator } from 'decentraland-ecs/src'

function makeGLTF(src: string, position: Vector3) {
  const ent = new Entity()
  ent.set(new GLTFShape(src))
  ent.set(
    new Transform({
      position
    })
  )
  engine.addEntity(ent)
  return ent
}

const m1 = makeGLTF('models/shark_anim.gltf', new Vector3(5, 3, 5))

const clip1 = new AnimationClip('shark_skeleton_bite', {
  weight: 0.1,
  looping: true
})
const clip2 = new AnimationClip('shark_skeleton_swim', {
  weight: 0.1,
  looping: true
})
const animator = m1.getOrCreate(Animator)
animator.addClip(clip1)
animator.addClip(clip2)

clip1.play()
clip2.play()
