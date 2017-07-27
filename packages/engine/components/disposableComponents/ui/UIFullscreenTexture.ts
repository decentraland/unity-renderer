import { DisposableComponent } from '../DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { UIScreenSpaceShape } from 'decentraland-ecs/src/decentraland/UIShapes'

export class UIFullScreenTexture extends DisposableComponent {
  fullscreenTexture: BABYLON.GUI.AdvancedDynamicTexture = BABYLON.GUI.AdvancedDynamicTexture.CreateFullscreenUI(
    'fullscreen-texture'
  )

  onAttach(_entity: BaseEntity): void {
    // noop
  }

  onDetach(_entity: BaseEntity): void {
    // noop
  }

  dispose() {
    if (this.fullscreenTexture) {
      this.fullscreenTexture.dispose()
      delete this.fullscreenTexture
    }

    super.dispose()
  }

  async updateData(_: UIScreenSpaceShape): Promise<void> {
    // noop
  }
}

DisposableComponent.registerClassId(CLASS_ID.UI_SCREEN_SPACE_SHAPE, UIFullScreenTexture)
