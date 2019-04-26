import {
  Entity,
  GLTFShape,
  Transform,
  engine,
  Vector3,
  OnPointerDown,
  AnimationState,
  Animator,
  log
} from 'decentraland-ecs/src'

onerror = function() {
  debugger
}

// First way to
const shark = new Entity()
const shape = new GLTFShape('shark_anim.gltf')
const animator = shark.getComponentOrCreate(Animator)
const clip = animator.getClip('shark_skeleton_swim')
clip.setParams({ weight: 0.7, speed: 3.0 })
animator.addClip(clip)
clip.play()

shark.addComponentOrReplace(shape)
shark.addComponentOrReplace(
  new Transform({
    position: new Vector3(2, 1, 6)
  })
)

const shark2 = new Entity()
const clip2 = new AnimationState('shark_skeleton_bite', { weight: 0.7, speed: 5 })
const clip3 = new AnimationState('shark_skeleton_swim', { weight: 0.7, speed: 0.5 })
const animator2 = shark2.getComponentOrCreate(Animator)
animator2.addClip(clip2)
animator2.addClip(clip3)
clip2.play()
clip3.play()

shark2.addComponentOrReplace(shape)
shark2.addComponentOrReplace(
  new Transform({
    position: new Vector3(6, 1, 6)
  })
)

shark.addComponentOrReplace(
  new OnPointerDown(() => {
    // just to test getting a clip
    const clip = animator.getClip('shark_skeleton_swim')
    if (clip) {
      clip.playing = !clip.playing
    }
  })
)

engine.addEntity(shark)
engine.addEntity(shark2)

declare var dcl: any

dcl.onEvent(function(event: any) {
  log('event', event)
})
