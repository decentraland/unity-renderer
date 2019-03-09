import { DisposableComponent } from '../DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { UIFullScreenShape } from 'decentraland-ecs/src/decentraland/UIShapes'

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

  async updateData(_: UIFullScreenShape): Promise<void> {
    // noop
  }
}

DisposableComponent.registerClassId(CLASS_ID.UI_FULLSCREEN_SHAPE, UIFullScreenTexture)
