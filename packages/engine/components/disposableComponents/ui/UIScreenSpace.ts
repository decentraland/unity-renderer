import { DisposableComponent } from '../DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src/decentraland/Components'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { SharedSceneContext } from 'engine/entities/SharedSceneContext'

export class UIScreenSpace extends DisposableComponent {
  data: { visible: boolean } = { visible: true }

  screenSpace: BABYLON.GUI.Rectangle = new BABYLON.GUI.Rectangle('screenspace')
  private _isEnabled: boolean = false
  private maximizeButton: BABYLON.GUI.Image = new BABYLON.GUI.Image('maximize-button')

  set isEnabled(enabled: boolean) {
    this._isEnabled = enabled
    this.maximizeButton.isEnabled = enabled
  }

  get isEnabled() {
    return this._isEnabled
  }

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
    super.dispose()
  }

  async updateData(data: { visible: boolean }): Promise<void> {
    // noop
  }

  dispatchOnClick = (pointerId: number) => {
    this.entities.forEach($ => {
      $.dispatchUUIDEvent('onClick', { entityId: $.uuid })
    })
  }
}

DisposableComponent.registerClassId(CLASS_ID.UI_SCREEN_SPACE_SHAPE, UIScreenSpace)
