import {
  Entity,
  BoxShape,
  engine,
  Vector3,
  Transform,
  Component,
  ISystem,
  Shape,
  OnClick,
  Scalar
} from 'decentraland-ecs/src'

@Component('Movement')
export class PingPongMovement {
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

let movingCubes = engine.getComponentGroup(PingPongMovement)

export class PingPongMovementSystem implements ISystem {
  update(dt: number) {
    for (let cubeEntity of movingCubes.entities) {
      let transform = cubeEntity.getComponent(Transform)
      let movementData = cubeEntity.getComponent(PingPongMovement)

      if (movementData.speed === 0 || movementData.waypoints.length < 2) continue

      movementData.lerpTime += dt * movementData.speed

      let reachedDestination = movementData.lerpTime >= 1
      if (reachedDestination) {
        movementData.lerpTime = 1
      }

      transform.position = Vector3.Lerp(
        movementData.waypoints[movementData.currentWaypoint],
        movementData.waypoints[movementData.targetWaypoint],
        movementData.lerpTime
      )

      if (reachedDestination) {
        movementData.lerpTime = 0
        movementData.currentWaypoint = movementData.targetWaypoint

        if (this.shouldSwitchDirection(movementData)) {
          movementData.goingForward = !movementData.goingForward
        }

        movementData.targetWaypoint = movementData.currentWaypoint + (movementData.goingForward ? 1 : -1)
      }
    }
  }

  shouldSwitchDirection(movementData: PingPongMovement) {
    return (
      (movementData.goingForward && movementData.currentWaypoint == movementData.waypoints.length - 1) ||
      (!movementData.goingForward && movementData.currentWaypoint == 0)
    )
  }
}
let movementSystem = new PingPongMovementSystem()
engine.addSystem(movementSystem)

export function configureShapeEntityPositions(waypointsPath: Vector3[], speed: number, shape: Shape): Entity {
  let entity = new Entity()
  entity.addComponentOrReplace(shape)
  entity.addComponentOrReplace(
    new Transform({
      position: waypointsPath[0]
    })
  )

  entity.addComponentOrReplace(new PingPongMovement(waypointsPath, speed))

  engine.addEntity(entity)
  return entity
}

// Elevator platform
let elevatorEntity = configureShapeEntityPositions([new Vector3(24, 0, 8), new Vector3(24, 10, 8)], 0.5, new BoxShape())
elevatorEntity.getComponent(Transform).scale = new Vector3(2, 0.25, 2)

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

  shouldSwitchDirection(movementData: PingPongMovement) {
    return (
      (movementData.goingForward && movementData.currentWaypoint == movementData.waypoints.length - 1) ||
      (!movementData.goingForward && movementData.currentWaypoint == 0)
    )
  }
}
let rotationSystem = new ObjectRotationSystem()
engine.addSystem(rotationSystem)

let rotatingPlatformEntity = new Entity()
rotatingPlatformEntity.addComponentOrReplace(new BoxShape())
rotatingPlatformEntity.addComponentOrReplace(
  new Transform({
    position: new Vector3(8, 1, 24),
    scale: new Vector3(5, 0.25, 1.5)
  })
)
rotatingPlatformEntity.addComponentOrReplace(new ObjectRotation(10, new Vector3(0, 1, 0)))
engine.addEntity(rotatingPlatformEntity)

// Moving platform
let movingPlatformEntity = configureShapeEntityPositions(
  [new Vector3(3, 1, 3), new Vector3(3, 1, 15), new Vector3(15, 1, 15), new Vector3(15, 1, 3), new Vector3(3, 1, 3)],
  0.1,
  new BoxShape()
)
movingPlatformEntity.getComponent(Transform).scale = new Vector3(2, 0.25, 2)
movingPlatformEntity.addComponentOrReplace(new ObjectRotation(10, new Vector3(0, 1, 0)))
movingPlatformEntity.addComponentOrReplace(
  new OnClick(e => {
    movingPlatformEntity.getComponent(PingPongMovement).speed *= 1.25;
  })
)

// "Pendulum" platform
let pendulumPivotentity = new Entity()
engine.addEntity(pendulumPivotentity)

pendulumPivotentity.addComponentOrReplace(
  new Transform({
    position: new Vector3(24, 4, 24)
  })
)

let pendulumPlatformtentity = new Entity()
engine.addEntity(pendulumPlatformtentity)

pendulumPlatformtentity.setParent(pendulumPivotentity)
pendulumPlatformtentity.addComponentOrReplace(new BoxShape())
pendulumPlatformtentity.addComponentOrReplace(
  new Transform({
    position: new Vector3(0, -3, 0),
    scale: new Vector3(2, 0.25, 2)
  })
)
pendulumPlatformtentity.addComponentOrReplace(
  new OnClick(e => {
    pendulumPivotentity.addComponentOrReplace(new ObjectRotation(5, new Vector3(0, 0, 1)))
  })
)

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
    position: new Vector3(18.5, 1, 18.5),
    scale: new Vector3(2, 2, 2)
  })
)
scalingCubeEntity.addComponentOrReplace(new ObjectScaling(2, 4, 1))
