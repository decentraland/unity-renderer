import { Entity, BoxShape, engine, Vector3, Transform, Component, ISystem, Shape, GLTFShape, Animator, AnimationState, OnClick, NFTShape, Scalar} from 'decentraland-ecs/src'

@Component('Movement')
export class PathMovement {
  waypoints: Vector3[]
  currentWaypoint: number = 0
  targetWaypoint: number = 0
  lerpTime: number = 0
  speed: number = 3
  goingForward: boolean = true

  constructor(newPath: Vector3[], speed: number) {
    this.waypoints = newPath
    this.speed = speed
  }
}

const movingCubes = engine.getComponentGroup(PathMovement)

export class PingPongMovementSystem implements ISystem
{
  update(dt: number) {
    for (let cubeEntity of movingCubes.entities) {
      let transform = cubeEntity.getComponent(Transform)
      let movementData = cubeEntity.getComponent(PathMovement)

      if(movementData.speed == 0 || movementData.waypoints.length < 2) continue

      movementData.lerpTime += dt * movementData.speed

      let reachedDestination = movementData.lerpTime >= 1
      if(reachedDestination){
        movementData.lerpTime = 1
      }

      transform.position = Vector3.Lerp(movementData.waypoints[movementData.currentWaypoint], movementData.waypoints[movementData.targetWaypoint], movementData.lerpTime)

      if(reachedDestination) {
        movementData.lerpTime = 0
        movementData.currentWaypoint = movementData.targetWaypoint

        if(this.shouldSwitchDirection(movementData)) {
          movementData.goingForward = !movementData.goingForward
        }

        movementData.targetWaypoint = movementData.currentWaypoint + (movementData.goingForward ? 1 : -1)
      }
    }
  }

  shouldSwitchDirection (movementData: PathMovement) {
    return  (movementData.goingForward && movementData.currentWaypoint == movementData.waypoints.length - 1) ||
            (!movementData.goingForward && movementData.currentWaypoint == 0)
  }
}
let movementSystem = new PingPongMovementSystem()
engine.addSystem(movementSystem)

export function configureShapeEntityPositions(waypointsPath: Vector3[], speed: number, shape: Shape) {
  const entity = new Entity()
  entity.addComponentOrReplace(shape)
  entity.addComponentOrReplace(
    new Transform({
      position: waypointsPath[0]
    }))

  entity.addComponentOrReplace(new PathMovement(waypointsPath, speed))

  engine.addEntity(entity)
  return entity
}

configureShapeEntityPositions([new Vector3(-3, 1, -8), new Vector3(35, 1, -8)], 0.2, new NFTShape('ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536'))
configureShapeEntityPositions([new Vector3(8, 50, 8)], 0, new BoxShape())
configureShapeEntityPositions([new Vector3(16, 1, 16)], 0, new BoxShape())
configureShapeEntityPositions([new Vector3(-1, 1, 8), new Vector3(17, 1, 8)], 0.8, new BoxShape())
configureShapeEntityPositions([new Vector3(8, 1, 16),
          new Vector3(8, 1, 0),
          new Vector3(8, 1, -16),
          new Vector3(8, 1, -24),
          new Vector3(24, 1, -24),
          new Vector3(40, 1, -24),
          new Vector3(40, 1, -40),
          new Vector3(24, 1, -40),
          new Vector3(24, 1, -24)
        ], 0.7, new BoxShape())

// PUSHABLE ANIMATED SHARK
let sharkEntity = configureShapeEntityPositions([new Vector3(31.5, 1.2, -16)], 0.7, new GLTFShape('models/shark.gltf'))
let animator = new Animator()
let clipSwim = new AnimationState('swim')
animator.addClip(clipSwim)
sharkEntity.addComponent(animator)
clipSwim.play()

let sharkLeftMovementTrigger = new Entity()
sharkLeftMovementTrigger.addComponentOrReplace(new BoxShape());
sharkLeftMovementTrigger.setParent(sharkEntity)
sharkLeftMovementTrigger.addComponent(
  new Transform({
    position: new Vector3(-0.25, 2, 0),
    scale: new Vector3(0.3, 1, 3)
  })
)
sharkLeftMovementTrigger.addComponent(
  new OnClick(e => {
    sharkEntity.getComponent(Transform).position.x += 1
  })
)
engine.addEntity(sharkLeftMovementTrigger)

let sharkRightMovementTrigger = new Entity()
sharkRightMovementTrigger.addComponentOrReplace(new BoxShape());
sharkRightMovementTrigger.setParent(sharkEntity)
sharkRightMovementTrigger.addComponent(
  new Transform({
    position: new Vector3(0.25, 2, 0),
    scale: new Vector3(0.3, 1, 3)
  })
)
sharkRightMovementTrigger.addComponent(
  new OnClick(e => {
    sharkEntity.getComponent(Transform).position.x -= 1
  })
)
engine.addEntity(sharkRightMovementTrigger)

let sharkVisibilityTrigger = new Entity()
sharkVisibilityTrigger.addComponentOrReplace(new BoxShape());
sharkVisibilityTrigger.setParent(sharkEntity)
sharkVisibilityTrigger.addComponent(
  new Transform({
    position: new Vector3(-0.25, 3, 0),
    scale: new Vector3(0.3, 0.3, 0.3)
  })
)
sharkVisibilityTrigger.addComponent(
  new OnClick(e => {
    let shapeComponent = sharkEntity.getComponent(GLTFShape)
    shapeComponent.visible = !shapeComponent.visible
  })
)
engine.addEntity(sharkVisibilityTrigger)

// PUSHABLE COLLIDING NPC
let npcEntity = configureShapeEntityPositions([new Vector3(16, 0, 0)], 0.7, new GLTFShape('models/Avatar_Idle.glb'))

let npcLeftMovementTrigger = new Entity()
npcLeftMovementTrigger.addComponentOrReplace(new BoxShape());
npcLeftMovementTrigger.setParent(npcEntity)
npcLeftMovementTrigger.addComponent(
  new Transform({
    position: new Vector3(-0.25, 3, 0),
    scale: new Vector3(0.3, 0.5, 2)
  })
)
npcLeftMovementTrigger.addComponent(
  new OnClick(e => {
    npcEntity.getComponent(Transform).position.x += 1
  })
)
engine.addEntity(npcLeftMovementTrigger)

let npcRightMovementTrigger = new Entity()
npcRightMovementTrigger.addComponentOrReplace(new BoxShape());
npcRightMovementTrigger.setParent(npcEntity)
npcRightMovementTrigger.addComponent(
  new Transform({
    position: new Vector3(0.25, 3, 0),
    scale: new Vector3(0.3, 0.5, 2)
  })
)
npcRightMovementTrigger.addComponent(
  new OnClick(e => {
    npcEntity.getComponent(Transform).position.x -= 1
  })
)
engine.addEntity(npcRightMovementTrigger)

let npcVisibilityTrigger = new Entity()
npcVisibilityTrigger.addComponentOrReplace(new BoxShape());
npcVisibilityTrigger.setParent(npcEntity)
npcVisibilityTrigger.addComponent(
  new Transform({
    position: new Vector3(-0.25, 4, 0),
    scale: new Vector3(0.3, 0.3, 0.3)
  })
)
npcVisibilityTrigger.addComponent(
  new OnClick(e => {
    let shapeComponent = npcEntity.getComponent(GLTFShape)
    shapeComponent.visible = !shapeComponent.visible
  })
)
engine.addEntity(npcVisibilityTrigger)

// Dynamically scaled platform
@Component('ObjectScaling')
export class ObjectScaling {
  speed: number = 1
  initialScale: number = 1
  targetScale: number = 2
  lerpTime: number = 0
  scalingUp: boolean = true

  constructor(initialScale: number, targetScale: number, speed: number) {
    this.speed = speed
    this.initialScale = initialScale
    this.targetScale = targetScale
  }
}
let scalingCubes = engine.getComponentGroup(ObjectScaling)

export class ObjectScalingSystem implements ISystem {
  update(dt: number) {
    for (let cubeEntity of scalingCubes.entities) {
      let scalingComponent = cubeEntity.getComponent(ObjectScaling)

      let transform = cubeEntity.getComponent(Transform)

      if (scalingComponent.speed == 0 || scalingComponent.initialScale == scalingComponent.targetScale) continue

      scalingComponent.lerpTime += dt * scalingComponent.speed

      let reachedDestination = scalingComponent.lerpTime >= 1
      if (reachedDestination) {
        scalingComponent.lerpTime = 1
      }

      let currentScale = Scalar.Lerp(
        scalingComponent.initialScale,
        scalingComponent.targetScale,
        scalingComponent.lerpTime
      )
      transform.scale = transform.scale.normalize().scale(currentScale)

      if (reachedDestination) {
        // Invert target and initial scale when reached destination
        scalingComponent.lerpTime = 0
        let targetScale = scalingComponent.targetScale
        scalingComponent.targetScale = scalingComponent.initialScale
        scalingComponent.initialScale = targetScale
      }
    }
  }
}
let scalingSystem = new ObjectScalingSystem()
engine.addSystem(scalingSystem)

let scalingCubeEntity = new Entity()
engine.addEntity(scalingCubeEntity)
scalingCubeEntity.addComponentOrReplace(new BoxShape())
scalingCubeEntity.addComponentOrReplace(
  new Transform({
    position: new Vector3(8, 1, 15),
    scale: new Vector3(2, 2, 2)
  })
)
scalingCubeEntity.addComponentOrReplace(new ObjectScaling(2, 4, 1))

// Rotating platform
@Component('ObjectRotation')
export class ObjectRotation {
  speed: number = 1
  rotationAxis: Vector3

  constructor(speed: number, axis: Vector3) {
    this.speed = speed
    this.rotationAxis = axis
  }
}
let rotatingCubes = engine.getComponentGroup(ObjectRotation)

export class ObjectRotationSystem implements ISystem {
  update(dt: number) {
    for (let cubeEntity of rotatingCubes.entities) {
      let rotationComponent = cubeEntity.getComponent(ObjectRotation)

      let transform = cubeEntity.getComponent(Transform)
      transform.rotate(rotationComponent.rotationAxis, rotationComponent.speed * dt)
    }
  }
}
let rotationSystem = new ObjectRotationSystem()
engine.addSystem(rotationSystem)

let rotatingPlatformEntity = new Entity()
rotatingPlatformEntity.addComponentOrReplace(new BoxShape())
rotatingPlatformEntity.addComponentOrReplace(
  new Transform({
    position: new Vector3(2, 1, 2),
    scale: new Vector3(5, 0.25, 1.5)
  })
)
rotatingPlatformEntity.addComponentOrReplace(new ObjectRotation(10, new Vector3(0, 1, 0)))
engine.addEntity(rotatingPlatformEntity)
