import { Entity, GLTFShape, engine, Vector3, Transform, AnimationState, Animator } from 'decentraland-ecs/src'

function makeGLTF(src: string, position: Vector3) {
  const ent = new Entity()
  ent.addComponentOrReplace(new GLTFShape(src))
  ent.addComponentOrReplace(
    new Transform({
      position
    })
  )
  engine.addEntity(ent)
  return ent
}

const m1 = makeGLTF('models/shark_anim.gltf', new Vector3(5, 3, 5))

const clip1 = new AnimationState('shark_skeleton_bite', {
  weight: 0.1,
  looping: true
})
const clip2 = new AnimationState('shark_skeleton_swim', {
  weight: 0.1,
  looping: true
})
const animator = m1.getComponentOrCreate(Animator)
animator.addClip(clip1)
animator.addClip(clip2)
clip1.reset();
clip1.play()
clip2.play()


let totalTime = 0

class AWaiterSystem {
  static instance = new AWaiterSystem()

  update(dt: number) {
    totalTime += dt

    if (totalTime > 2) {
      clip1.reset();
      clip2.reset();
	  totalTime = 0;
    }
  }
}

engine.addSystem(AWaiterSystem.instance)