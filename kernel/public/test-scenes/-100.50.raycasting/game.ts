import {
  BoxShape,
  Color3,
  engine,
  Entity,
  ISystem,
  Material,
  Transform,
  PhysicsCast,
  Ray,
  Vector3,
  log,
} from 'decentraland-ecs'

//-------------------------------------------------------
// Configuration constants
const boxesCount = 10
const speed = 1
const rayDistance = 128

//-------------------------------------------------------
// Materials
const defaultMaterial = new Material()
defaultMaterial.metallic = 0
defaultMaterial.roughness = 1
defaultMaterial.albedoColor = new Color3(.2, .2, 1)

const hitMaterial = new Material()
hitMaterial.metallic = 1
hitMaterial.roughness = 0.5
hitMaterial.albedoColor = new Color3(1, 1, 30)

const hitMaterial2 = new Material()
hitMaterial2.metallic = 1
hitMaterial2.roughness = 0.5
hitMaterial2.albedoColor = new Color3(.2, 1, .2)

const rayMaterial = new Material()
rayMaterial.metallic = 1
rayMaterial.roughness = 0.5
rayMaterial.albedoColor = new Color3(30, 1, 1)

//-------------------------------------------------------
// Parent cube
const rayCube = new Entity()
const rayTransform = new Transform()
rayCube.addComponentOrReplace(rayTransform)

rayTransform.position.x = -8
rayTransform.position.y = 1.5

//-------------------------------------------------------
// Child cube with renderer
const rayCubeObject = new Entity()
const rayBoxShape = new BoxShape()
const rayObjectTransform = new Transform()

rayBoxShape.withCollisions = false
rayCubeObject.addComponent(rayBoxShape)
rayCubeObject.addComponentOrReplace(rayMaterial)
rayCubeObject.addComponentOrReplace(rayObjectTransform)

rayCubeObject.setParent(rayCube)

// This is just to show a ray-like object to represent
// the casted ray
rayObjectTransform.scale.x = 0.1
rayObjectTransform.scale.y = 0.1
rayObjectTransform.scale.z = rayDistance
rayObjectTransform.position.z = rayDistance / 2

engine.addEntity(rayCube)

//-------------------------------------------------------
// Create all the boxes with default values
for (let i = 0; i < boxesCount; i++) {
  const boxShape = new BoxShape()
  const cube = new Entity()
  const transform = new Transform()

  cube.addComponent(defaultMaterial)
  cube.addComponentOrReplace(transform)
  transform.position.set(-32 + i * 3, 1.5, 16)
  cube.addComponentOrReplace(boxShape)
  engine.addEntity(cube)
}

//-------------------------------------------------------
// System that casts the rays
class RaycastingSystem implements ISystem {
  group = engine.getComponentGroup(Transform)

  update(dt: number) {
    // Fixed ray
    const ray: Ray = {
      origin: rayCube.getComponent(Transform).position,
      direction: Vector3.Forward(),
      distance: 1000
    }

    // Ray from camera
    const rayFromCamera: Ray = PhysicsCast.instance.getRayFromCamera(1000)

    // For the fixed ray, we cast a hit first
    PhysicsCast.instance.hitFirst(ray, (e) => {
      if (e.didHit) {
        log(e.hitPoint)
        engine.entities[e.entity.entityId].addComponentOrReplace(hitMaterial)
      }
      else {
        for (let entity of this.group.entities) {
          if (entity != rayCube && entity != rayCubeObject) {
            entity.addComponentOrReplace(defaultMaterial)
          }
        }
      }
    }, 0)

    // For the camera ray, we cast a hit all
    PhysicsCast.instance.hitAll(rayFromCamera, (e) => {
      if (e.didHit) {
        for (let entity of e.entities) {
          if (entity.didHit) {
            engine.entities[entity.entity.entityId].addComponentOrReplace(hitMaterial2)
          }
        }
      }
    }, 0)
  }
}

//-------------------------------------------------------
// This system moves all the cubes to the right and when
// a limit is reached, teleports the cube to the left
class MovingCubes implements ISystem {
  group = engine.getComponentGroup(Transform)
  update(dt: number) {
    const delta = speed * dt
    for (let entity of this.group.entities) {
      if (entity != rayCube && entity != rayCubeObject) {
        const transform = entity.getComponent(Transform)

        transform.position.x += delta

        if (transform.position.x >= 0)
          transform.position.x -= 32
      }
    }
  }
}

//-------------------------------------------------------
// Adds systems to the engine
engine.addSystem(new RaycastingSystem())
engine.addSystem(new MovingCubes())

