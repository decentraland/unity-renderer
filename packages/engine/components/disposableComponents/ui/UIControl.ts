import { DisposableComponent } from '../DisposableComponent'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { UIFullScreenTexture } from './UIFullscreenTexture'
import { UIScreenSpace } from './UIScreenSpace'

export abstract class UIControl<T, K extends BABYLON.GUI.Control> extends DisposableComponent {
  data!: T
  control!: K

  onAttach(_entity: BaseEntity): void {
    // noop
  }

  onDetach(_entity: BaseEntity): void {
    // noop
  }

  dispose() {
    if (this.control) {
      this.control.dispose()
    }
    super.dispose()
  }

  setParent(id: string) {
    const parent = this.context.disposableComponents.get(id)

    if (parent instanceof UIFullScreenTexture) {
      parent.fullscreenTexture.addControl(this.control)
    } else if (parent instanceof UIScreenSpace) {
      parent.screenSpace.addControl(this.control)
    } else if (parent instanceof UIControl && 'addControl' in parent.control) {
      parent.control.addControl(this.control)
    }
  }
}
