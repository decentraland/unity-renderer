import { DisposableComponent } from '../DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { UIButton as UIButtonShape } from 'decentraland-ecs/src/decentraland/UIShapes'
import { SharedSceneContext } from 'engine/entities/SharedSceneContext'
import { UIControl } from './UIControl'

class UIButton extends UIControl<UIButtonShape, BABYLON.GUI.Button> {
  control = new BABYLON.GUI.Button('button')

  constructor(ctx: SharedSceneContext, uuid: string) {
    super(ctx, uuid)
  }

  onAttach(_entity: BaseEntity): void {
    // noop
  }

  onDetach(_entity: BaseEntity): void {
    // noop
  }

  dispose() {
    // noop
  }

  async updateData(data: UIButtonShape): Promise<void> {
    // noop
  }

  dispatchOnClick = (pointerId: number) => {
    this.entities.forEach($ =>
      $.dispatchUUIDEvent('onClick', {
        entityId: $.uuid
      })
    )
  }
}

DisposableComponent.registerClassId(CLASS_ID.UI_BUTTON_SHAPE, UIButton)
