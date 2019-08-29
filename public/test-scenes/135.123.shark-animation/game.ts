import { Entity, GLTFShape, engine, Vector3, Transform, AnimationState, Animator } from 'decentraland-ecs/src'
import { OnClick } from 'decentraland-ecs/src/decentraland/UIEvents'

// Add Shark
let shark = new Entity()
shark.addComponent(
  new Transform({
    position: new Vector3(10, 3, 10)
  })
)
shark.addComponent(new GLTFShape('models/shark.gltf'))

// Add animations
/*
NOTE: when you try to get an animation clip that hasn't been created
from a GLTFShape component, the clip is created automatically.
*/
const animator = new Animator()
let clipSwim = new AnimationState('swim')
let clipBite = new AnimationState('bite')
animator.addClip(clipBite)
animator.addClip(clipSwim)

shark.addComponent(animator)

// Activate swim animation
clipSwim.play()

// Add click interaction
shark.addComponent(
  new OnClick(e => {
    clipBite.playing = !clipBite.playing
  })
)

// Add shark to engine
engine.addEntity(shark)

// Add 3D model for scenery
const seaBed = new Entity()
seaBed.addComponent(new GLTFShape('models/Underwater.gltf'))
seaBed.addComponent(
  new Transform({
    position: new Vector3(10, 0, 10)
  })
)
engine.addEntity(seaBed)
