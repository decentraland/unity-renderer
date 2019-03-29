import * as BABYLON from 'babylonjs'
import { deepEqual } from 'atomicHelpers/deepEqual'
import { BaseEntity } from '../entities/BaseEntity'

export interface BaseComponent<T> extends BABYLON.Behavior<BaseEntity> {
  /**
   * Called once when the component is initialized. Used to set up initial state and instantiate variables.
   */
  attach(entity: BaseEntity): void

  init(): void
  /**
   * Called both when the component is initialized and whenever any of the component's properties is updated (via setProperties).
   * Used to modify the entity. If the new properties are the same as the old properties, this method won't be called.
   */
  update(oldValue: T | null, newValue: T): void

  /**
   * Called when the component is removed from the entity or when the entity is detached from the scene.
   * Used to undo all previous modifications to the entity.
   */
  detach(): void

  /**
   * Performs a shallow comparison between the current props
   * @param props newValue
   */
  shouldSceneUpdate(newValue: T): boolean

  /**
   * Called whenever properties change. Used to validate data before calling the `update()` method.
   */
  setValue(newValue: T): void
}

export class BaseComponent<T> implements BABYLON.Behavior<BaseEntity> {
  value: T | null = null
  entity: BaseEntity

  constructor(entity: BaseEntity) {
    this.entity = entity
  }

  init(): void {
    // stub
  }

  transformValue(value: T): T {
    return value
  }

  /**
   * Called whenever properties change. Used to validate data before calling the `update()` method.
   */
  setValue(newValue: T) {
    const transformedValue = this.transformValue(newValue)
    if (this.shouldSceneUpdate(transformedValue)) {
      const oldValue = this.value
      this.value = transformedValue
      this.update(oldValue, this.value)
    }
  }

  /**
   * Performs a shallow comparison between the current props
   * @param props
   */
  shouldSceneUpdate(newValue: T) {
    return !deepEqual(newValue, this.value)
  }

  attach(entity: BaseEntity) {
    /* stub */
  }

  update(oldProperties: T | null, newProperties: T) {
    /* stub */
  }

  detach() {
    /* stub */
  }
}
