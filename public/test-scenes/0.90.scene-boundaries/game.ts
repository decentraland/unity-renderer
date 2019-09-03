import { Entity, BoxShape, engine, Vector3, Transform, Component, ISystem, Shape, GLTFShape, Animator, AnimationState, OnClick, NFTShape} from 'decentraland-ecs/src'

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

// PUSHABLE COLLIDING TREE
let treeEntity = configureShapeEntityPositions([new Vector3(15, 0, 0)], 0.7, new GLTFShape('models/PalmTree_01.glb'))

let treeLeftMovementTrigger = new Entity()
treeLeftMovementTrigger.addComponentOrReplace(new BoxShape());
treeLeftMovementTrigger.setParent(treeEntity)
treeLeftMovementTrigger.addComponent(
  new Transform({
    position: new Vector3(-0.25, 2, 0),
    scale: new Vector3(0.3, 1, 3)
  })
)
treeLeftMovementTrigger.addComponent(
  new OnClick(e => {
    treeEntity.getComponent(Transform).position.x += 1
  })
)
engine.addEntity(treeLeftMovementTrigger)

let treeRightMovementTrigger = new Entity()
treeRightMovementTrigger.addComponentOrReplace(new BoxShape());
treeRightMovementTrigger.setParent(treeEntity)
treeRightMovementTrigger.addComponent(
  new Transform({
    position: new Vector3(0.25, 2, 0),
    scale: new Vector3(0.3, 1, 3)
  })
)
treeRightMovementTrigger.addComponent(
  new OnClick(e => {
    treeEntity.getComponent(Transform).position.x -= 1
  })
)
engine.addEntity(treeRightMovementTrigger)

let treeVisibilityTrigger = new Entity()
treeVisibilityTrigger.addComponentOrReplace(new BoxShape());
treeVisibilityTrigger.setParent(treeEntity)
treeVisibilityTrigger.addComponent(
  new Transform({
    position: new Vector3(-0.25, 3, 0),
    scale: new Vector3(0.3, 0.3, 0.3)
  })
)
treeVisibilityTrigger.addComponent(
  new OnClick(e => {
    let shapeComponent = treeEntity.getComponent(GLTFShape)
    shapeComponent.visible = !shapeComponent.visible
  })
)
engine.addEntity(treeVisibilityTrigger)
