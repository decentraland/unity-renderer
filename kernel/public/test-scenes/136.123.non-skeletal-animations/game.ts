import { Entity, GLTFShape, engine, Vector3, Transform, AnimationState, Animator } from 'decentraland-ecs/src'

// Scaling cube
let scalingCubeEntity = new Entity()
scalingCubeEntity.addComponent(
  new Transform({
    position: new Vector3(6.1, 2, 13.9)
  })
)
scalingCubeEntity.addComponent(new GLTFShape('models/Scale.glb'))

const scalingCubeAnimator = new Animator()
let scalingClip = new AnimationState('Scale')
scalingCubeAnimator.addClip(scalingClip)
scalingCubeEntity.addComponent(scalingCubeAnimator)
scalingClip.play()
engine.addEntity(scalingCubeEntity)

// Rotating cube
let rotatingCubeEntity = new Entity()
rotatingCubeEntity.addComponent(
  new Transform({
    position: new Vector3(14, 2, 14)
  })
)
rotatingCubeEntity.addComponent(new GLTFShape('models/Rotation.glb'))

const rotatingCubeAnimator = new Animator()
let rotatingClip = new AnimationState('Rotation')
rotatingCubeAnimator.addClip(rotatingClip)
rotatingCubeEntity.addComponent(rotatingCubeAnimator)
rotatingClip.play()
engine.addEntity(rotatingCubeEntity)

// Moving cube
let movingCubeEntity = new Entity()
movingCubeEntity.addComponent(
  new Transform({
    position: new Vector3(14, 2, 2)
  })
)
movingCubeEntity.addComponent(new GLTFShape('models/Move.glb'))

const movingCubeAnimator = new Animator()
let movingClip = new AnimationState('Move')
movingCubeAnimator.addClip(movingClip)
movingCubeEntity.addComponent(movingCubeAnimator)
movingClip.play()
engine.addEntity(movingCubeEntity)

// All-transformations cube
let allCubeEntity = new Entity()
allCubeEntity.addComponent(
  new Transform({
    position: new Vector3(14, 2, 6)
  })
)
allCubeEntity.addComponent(new GLTFShape('models/All.glb'))

const allCubeAnimator = new Animator()
let allTransformationsClip = new AnimationState('All')
allCubeAnimator.addClip(allTransformationsClip)
allCubeEntity.addComponent(allCubeAnimator)
allTransformationsClip.play()
engine.addEntity(allCubeEntity)
