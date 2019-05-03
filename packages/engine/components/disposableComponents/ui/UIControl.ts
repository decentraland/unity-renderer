import { DisposableComponent } from '../DisposableComponent'
import { BaseEntity } from 'engine/entities/BaseEntity'

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
    // noop
  }

  setParent(id: string) {
    // noop
  }
}
