import { Entity, GLTFShape, engine, Vector3, Transform, AnimationClip, Animator, OnClick } from 'decentraland-ecs/src'

// Add Shark
let shark = new Entity()
shark.add(new Transform({
  position: new Vector3(10, 3, 10)
}))
shark.add(new GLTFShape("models/shark.gltf"))

// Add animations
/* 
NOTE: when you try to get an animation clip that hasn't been created
from a GLTFShape component, the clip is created automatically.
*/
const animator = new Animator();
let clipSwim = new AnimationClip("swim")
let clipBite = new AnimationClip("bite")
animator.addClip(clipBite);
animator.addClip(clipSwim);

shark.add(animator);

// Activate swim animation
clipSwim.play()

// Add click interaction
shark.add(new OnClick(e => {
  clipBite.playing =! clipBite.playing
}))

// Add shark to engine
engine.addEntity(shark)

// Add 3D model for scenery
const seaBed = new Entity()
seaBed.add(new GLTFShape("models/Underwater.gltf"))
seaBed.add(new Transform({
  position: new Vector3(10, 0, 10)
}))
engine.addEntity(seaBed)