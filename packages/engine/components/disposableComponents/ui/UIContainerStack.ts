import { DisposableComponent } from '../DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { UIControl } from './UIControl'
import { UIContainerStack as UIContainerStackShape } from 'decentraland-ecs/src/decentraland/UIShapes'

export class UIContainerStack extends UIControl<UIContainerStackShape, BABYLON.GUI.StackPanel> {
  control = new BABYLON.GUI.StackPanel('stack')

  onAttach(_entity: BaseEntity): void {
    // noop
  }

  onDetach(_entity: BaseEntity): void {
    // noop
  }

  dispose() {
    // noop
  }

  async updateData(data: UIContainerStackShape): Promise<void> {
    // noop
  }
}

DisposableComponent.registerClassId(CLASS_ID.UI_CONTAINER_STACK, UIContainerStack)
