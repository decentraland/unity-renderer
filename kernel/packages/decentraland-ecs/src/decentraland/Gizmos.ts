import { Component, ObservableComponent } from '../ecs/Component'
import { CLASS_ID, OnUUIDEvent } from './Components'

/**
 * Gizmo identifiers
 * @beta
 */
export enum Gizmo {
  MOVE = 'MOVE',
  ROTATE = 'ROTATE',
  SCALE = 'SCALE',
  NONE = 'NONE'
}

/**
 * This event is triggered after the user finalizes dragging a gizmo.
 * @beta
 */
@Component('engine.gizmoEvent', CLASS_ID.UUID_CALLBACK)
export class OnGizmoEvent extends OnUUIDEvent<'gizmoEvent'> {
  @ObservableComponent.readonly
  readonly type: string = 'gizmoEvent'
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
   * Cycle through gizmos using click.
   */
  @ObservableComponent.field
  cycle: boolean = true

  /**
   * If cycle is false, this will be the selected gizmo
   */
  @ObservableComponent.field
  selectedGizmo?: Gizmo

  /**
   * Align the gizmos to match the local reference system
   */
  @ObservableComponent.field
  localReference: boolean = false
}
