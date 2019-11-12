import { Entity, engine, Transform, GLTFShape, Vector3, Quaternion } from 'decentraland-ecs/src'

const entity = new Entity()
entity.addComponentOrReplace(new GLTFShape('models/irregular.glb'))
entity.addComponentOrReplace(
  new Transform({
    position: new Vector3(16, 0, 0),
    rotation: Quaternion.Euler(0, -90, 0)
  }))
engine.addEntity(entity)
