import { BaseComponent } from '../BaseComponent'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { CLASS_ID } from 'decentraland-ecs/src'

const defaultValue = {
  x: true,
  y: true,
  z: true
}

export class Billboard extends BaseComponent<typeof defaultValue> {
  transformValue(value: any) {
    return {
      x: !!value.x,
      y: !!value.y,
      z: !!value.z
    }
  }

  update() {
    const { x, y, z } = this.value
    this.entity.billboardMode = (x ? 1 : 0) | (y ? 2 : 0) | (z ? 4 : 0)
  }

  detach() {
    this.entity.billboardMode = 0
  }
}

export function setEntityBillboard(entity: BaseEntity, data: typeof defaultValue) {
  entity.context.UpdateEntityComponent({
    classId: CLASS_ID.BILLBOARD,
    entityId: entity.id,
    json: JSON.stringify(data),
    name: 'billboard'
  })
}
