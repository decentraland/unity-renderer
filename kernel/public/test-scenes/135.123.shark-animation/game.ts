import { Entity, GLTFShape, engine, Vector3, Transform, AnimationState, Animator, ActionButton } from 'decentraland-ecs/src'
import { OnPointerDown, OnPointerUp } from 'decentraland-ecs/src/decentraland/UIEvents'

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
let onClickComponent = new OnPointerDown(
  e => {
    clipBite.playing = !clipBite.playing
  }, {
    button: ActionButton.POINTER,
     hoverText: "OnPointerDown!", distance: 100 })

shark.addComponent(onClickComponent)

shark.addComponent(new OnPointerUp(
  e => {}, {
    button: ActionButton.POINTER,
     hoverText: "OnPointerUp!", distance: 100 }))

// Add shark to engine
engine.addEntity(shark)

// Add second shark
let shark2 = new Entity()
shark2.addComponent(
  new Transform({
    position: new Vector3(13, 5, 10)
  })
)
shark2.addComponent(new GLTFShape('models/shark.gltf'))
shark2.addComponent(new OnPointerDown(
  e => {}, {
    button: ActionButton.POINTER,
     hoverText: "OnPointerDown!", distance: 100 }))
shark2.addComponent(new OnPointerUp(
  e => {}, {
    button: ActionButton.POINTER,
      hoverText: "OnPointerUp!", distance: 100 }))
engine.addEntity(shark2)

// Add 3D model for scenery
const seaBed = new Entity()
seaBed.addComponent(new GLTFShape('models/Underwater.gltf'))
seaBed.addComponent(
  new Transform({
    position: new Vector3(10, 0, 8),
    scale: new Vector3(0.5, 0.5, 0.5)
  })
)
engine.addEntity(seaBed)
