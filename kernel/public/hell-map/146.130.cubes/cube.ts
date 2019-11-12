import { Entity, Vector3, Transform, BoxShape } from 'decentraland-ecs/src'

export class Cube extends Entity {
  constructor(position: Vector3, scale: Vector3) {
    super()

    let transform = new Transform()
    transform.position = position
    transform.scale = scale
    this.addComponent(transform)

    let shape = new BoxShape()
    this.addComponent(shape)
  }
}
