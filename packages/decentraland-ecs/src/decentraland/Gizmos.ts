import { Component, ObservableComponent } from '../ecs/Component'
import { CLASS_ID, OnUUIDEvent } from './Components'

/**
 * This event is triggered after the user finalizes dragging a gizmo.
 * @beta
 */
@Component('engine.dragEnded', CLASS_ID.UUID_CALLBACK)
export class OnDragEnded extends OnUUIDEvent {
  @ObservableComponent.readonly
  readonly type: string = 'dragEnded'
}

/**
 * Enables gizmos in the entity. Gizmos only work in EDITOR, PREVIEW or DEBUG modes.
 * @beta
 */
@Component('engine.gizmos', CLASS_ID.GIZMOS)
export class Gizmos extends ObservableComponent {
  /**
   * Enable position gizmo
   */
  @ObservableComponent.field
  position: boolean = true

  /**
   * Enable rotation gizmo
   */
  @ObservableComponent.field
  rotation: boolean = true

  /**
   * Enable scale gizmo
   */
  @ObservableComponent.field
  scale: boolean = true

  /**
   * Update entity while dragging. Also let the entity in it's final place after
   * releasing the gizmo.
   */
  @ObservableComponent.field
  updateEntity: boolean = true
}
