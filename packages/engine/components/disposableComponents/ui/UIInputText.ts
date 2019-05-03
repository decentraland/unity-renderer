import { DisposableComponent } from '../DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { UIInputText as UIInputTextShape } from 'decentraland-ecs/src/decentraland/UIShapes'
import { SharedSceneContext } from 'engine/entities/SharedSceneContext'
import { UIControl } from './UIControl'
import { IEvents } from 'decentraland-ecs/src/decentraland/Types'

class UIInputText extends UIControl<UIInputTextShape, BABYLON.GUI.InputText> {
  control = new BABYLON.GUI.InputText('input')

  constructor(ctx: SharedSceneContext, uuid: string) {
    super(ctx, uuid)
  }

  onAttach(_entity: BaseEntity): void {
    // noop
  }

  onDetach(_entity: BaseEntity): void {
    // noop
  }

  async updateData(data: UIInputTextShape): Promise<void> {
    // noop
  }

  dispatchOnChanged = (data: IEvents['onChange']) => {
    this.entities.forEach($ => $.dispatchUUIDEvent('onChange', data))
  }

  dispatchOnFocus = (data: IEvents['onFocus']) => {
    this.entities.forEach($ => $.dispatchUUIDEvent('onFocus', data))
  }

  dispatchOnBlur = (data: IEvents['onBlur']) => {
    this.entities.forEach($ => $.dispatchUUIDEvent('onBlur', data))
  }
}

DisposableComponent.registerClassId(CLASS_ID.UI_INPUT_TEXT_SHAPE, UIInputText)
