import { Vector3, Transform, Entity, PlaneShape, Component, IEntity, Scalar } from 'decentraland-ecs/src'
import { greenMaterial } from './Params';

let planeShape = new PlaneShape()

@Component("progressBar")
export class ProgressBar {
  value: number = 0
  fullLength: number = 1
  entity: IEntity
  foregroundTransform: Transform
  foregroundEntity: IEntity

  constructor(entity: IEntity) {
    this.entity = entity

    this.foregroundEntity = new Entity()
    this.foregroundEntity.addComponent(planeShape)
    this.foregroundEntity.setParent(entity)

    this.foregroundTransform = new Transform({
      scale: new Vector3(1, 0.15, 0.1)
    })

    this.foregroundEntity.addComponent(this.foregroundTransform)

    this.foregroundEntity.addComponentOrReplace(greenMaterial)
    // engine.addEntity(this.foregroundEntity)
  }

  UpdateNormalizedValue(newValue: number) {
    this.value = Math.min(1, Math.max(0, newValue))

    let width = Scalar.Lerp(0, this.fullLength, this.value)
    this.foregroundTransform.scale.x = width
    this.foregroundTransform.position.x = -this.fullLength / 2 + width / 2
  }
}
