import { DisposableComponent } from './DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { IEventNames } from 'shared/events'

export type UUIDComponentData = {
  uuid: string
  type: IEventNames
}

export class UUIDComponent extends DisposableComponent {
  data: UUIDComponentData

  onAttach(entity: BaseEntity): void {
    if (this.data && this.data.type && this.data.uuid) {
      entity.addUUIDEvent(this.data.type, this.data.uuid)
    }
  }

  onDetach(entity: BaseEntity): void {
    if (this.data && this.data.type) {
      entity.removeUUIDEvent(this.data.type)
    }
  }

  async updateData(data: UUIDComponentData): Promise<void> {
    this.data = data
    this.entities.forEach($ => this.onAttach($))
  }
}

DisposableComponent.registerClassId(CLASS_ID.UUID_CALLBACK, UUIDComponent)
