import {
  BoxShape,
  Color3,
  Component,
  engine,
  Entity,
  ISystem,
  Material,
  PhysicsCast,
  Transform
} from 'decentraland-ecs'

const boxShape = new BoxShape()

const cube = new Entity()
const transform = new Transform()

const defaultMaterial = new Material()
defaultMaterial.metallic = 0
defaultMaterial.roughness = 1
defaultMaterial.albedoColor = new Color3(48, 48, 48)
const hitMaterial = new Material()
hitMaterial.metallic = 1
hitMaterial.roughness = 0.5
hitMaterial.albedoColor = new Color3(240, 24, 24)
cube.addComponent(defaultMaterial)

@Component('Hitteable')
class Hitteable {
  hit: boolean = false
  isColored: boolean = false
}

var isCasting = false
class TurningRedSystem implements ISystem {
  hitteable = engine.getComponentGroup(Hitteable)
  update(dt: number) {
    for (let entity of this.hitteable.entities) {
      const status = entity.getComponent('Hitteable')
      if (status.hit && !status.isColored) {
        entity.addComponent(hitMaterial)
        entity.removeComponent(defaultMaterial)
      } else if (!status.hit && status.isColored) {
        entity.addComponent(defaultMaterial)
        entity.removeComponent(hitMaterial)
      }
    }
    if (!isCasting) {
      isCasting = true
      PhysicsCast.instance.hitAll(
        {
          origin: nearBaseOfParcel,
          direction: inTheZdirection,
          distance: limit
        },
        event => {
          isCasting = false

          if (!event.didHit || event.entities === undefined)
            return

          // Clear all hits
          const hitteable = engine.getComponentGroup(Hitteable)
          for (let entity of hitteable.entities) {
            entity.getComponent(Hitteable).hit = false
          }
          // Mark new hits
          event.entities
            .filter(entity => engine.entities[entity.entity.entityId].hasComponent('Hitteable'))
            .forEach(entity => (engine.entities[entity.entity.entityId].getComponent(Hitteable).hit = true))
        }
      )
    }
  }
}
engine.addSystem(new TurningRedSystem())

// Trigger a ray from starting 1 unit away in all axes from the base of the parcel
const nearBaseOfParcel = { x: 1, y: 1, z: 1 }
// Going in depth (Z)
const inTheZdirection = { x: 0, y: 0, z: 1 }
// Tops 12 units of length
const limit = 12

cube.addComponentOrReplace(transform)
transform.position.set(5, 0, 5)

cube.addComponentOrReplace(boxShape)

engine.addEntity(cube)
