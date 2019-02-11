import { BaseComponent } from '../BaseComponent'
import { validators } from '../helpers/schemaValidator'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { CLASS_ID, ReadOnlyVector3, ReadOnlyQuaternion } from 'decentraland-ecs/src'

const defaultValue = {
  position: { x: 0, y: 0, z: 0 },
  rotation: { x: 0, y: 0, z: 0, w: 1 },
  scale: { x: 1, y: 1, z: 1 }
}

export class Transform extends BaseComponent<any> {
  transformValue(value: any) {
    return {
      position: validators.vector3(value.position, defaultValue.position),
      rotation: validators.quaternion(value.rotation, defaultValue.rotation),
      scale: validators.vector3(value.scale, defaultValue.scale),
      nonce: value.nonce
    }
  }

  update() {
    const { position, scale, rotation } = this.value

    if (!this.entity.rotationQuaternion) {
      this.entity.rotationQuaternion = this.entity.rotation.toQuaternion()
    }

    if (rotation) {
      this.entity.rotationQuaternion.set(rotation.x || 0, rotation.y || 0, rotation.z || 0, rotation.w || 0)
    } else {
      this.entity.rotationQuaternion.set(0, 0, 0, 1)
    }

    if (position) {
      this.entity.position.copyFrom(position)
    } else {
      this.entity.position.set(0, 0, 0)
    }

    if (scale) {
      this.entity.scaling.copyFrom(scale)
    } else {
      this.entity.scaling.set(1, 1, 1)
    }
  }

  detach() {
    this.entity.position.set(0, 0, 0)
    this.entity.rotationQuaternion = null
    this.entity.rotation.set(0, 0, 0)
    this.entity.scaling.set(1, 1, 1)
  }
}

export function setEntityTransform(
  entity: BaseEntity,
  transform: { position?: ReadOnlyVector3; scale?: ReadOnlyVector3; rotation?: ReadOnlyQuaternion }
) {
  entity.context.UpdateEntityComponent({
    classId: CLASS_ID.TRANSFORM,
    entityId: entity.id,
    json: JSON.stringify({
      position: transform.position || entity.position,
      scale: transform.scale || entity.scaling,
      rotation: transform.rotation || entity.rotationQuaternion
    }),
    name: 'transform'
  })
}
