import { DisposableComponent } from '../DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { UIControl } from './UIControl'
import { UIContainerRect as UIContainerRectShape } from 'decentraland-ecs/src/decentraland/UIShapes'

export class UIContainerRect extends UIControl<UIContainerRectShape, BABYLON.GUI.Rectangle> {
  control = new BABYLON.GUI.Rectangle('rect')

  onAttach(_entity: BaseEntity): void {
    // noop
  }

  onDetach(_entity: BaseEntity): void {
    // noop
  }

  dispose() {
    super.dispose()
  }

  async updateData(data: UIContainerRectShape): Promise<void> {
    // noop
  }
}

DisposableComponent.registerClassId(CLASS_ID.UI_CONTAINER_RECT, UIContainerRect)
