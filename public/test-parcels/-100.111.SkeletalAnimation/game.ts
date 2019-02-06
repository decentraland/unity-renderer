import { Entity, GLTFShape, Transform, engine, Vector3, OnClick, AnimationClip, Animator } from 'decentraland-ecs'

// First way to
const shark = new Entity()
const shape = new GLTFShape('shark_anim.gltf')
const animator = shark.getOrCreate(Animator)
const clip = animator.getClip('shark_skeleton_swim')
clip.setParams({ weight: 0.7, speed: 3.0 })
animator.addClip(clip)
clip.play()

shark.set(shape)
shark.set(
  new Transform({
    position: new Vector3(2, 1, 6)
  })
)

const shark2 = new Entity()
const clip2 = new AnimationClip('shark_skeleton_bite', { weight: 0.7, speed: 5 })
const clip3 = new AnimationClip('shark_skeleton_swim', { weight: 0.7, speed: 0.5 })
const animator2 = shark2.getOrCreate(Animator)
animator2.addClip(clip2)
animator2.addClip(clip3)
clip2.play()
clip3.play()

shark2.set(shape)
shark2.set(
  new Transform({
    position: new Vector3(6, 1, 6)
  })
)

shark.set(
  new OnClick(() => {
    // just to test getting a clip
    const clip = animator.getClip('shark_skeleton_swim')
    if (clip) {
      clip.playing = !clip.playing
    }
  })
)

engine.addEntity(shark)
engine.addEntity(shark2)
