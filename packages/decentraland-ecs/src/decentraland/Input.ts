// tslint:disable:ter-indent
// tslint:disable:ter-indent

import { GlobalInputEventResult, InputEventType } from './Types'
import { Vector3 } from './math'
import { Component, DisposableComponent } from '../ecs/Component'

/** @public */
export type InputEventKind = 'BUTTON_DOWN' | 'BUTTON_UP'

/**
 * @public
 */
export enum Pointer {
  PRIMARY = 'PRIMARY',
  SECONDARY = 'SECONDARY'
}

/** @public */
export type InputState = Record<
  Pointer,
  {
    BUTTON_DOWN: boolean
  }
>

/** @public */
export type LocalPointerEvent = GlobalInputEventResult & {
  origin: Vector3
  direction: Vector3
  pointer: Pointer
  hit?: GlobalInputEventResult['hit'] & {
    hitPoint: Vector3
    normal: Vector3
    worldNormal: Vector3
  }
}

/**
 * @public
 */
export class PointerEventComponent {
  constructor(public readonly callback: (event: LocalPointerEvent) => void) {
    if (!callback || !('apply' in callback) || !('call' in callback)) {
      throw new Error('Callback is not a function')
    }
    // tslint:disable-next-line:no-use-before-declare
    Input.ensureInstance()
  }
}

/**
 * @public
 */
@Component('pointerDown')
export class GlobalPointerDown extends PointerEventComponent {}

/**
 * @public
 */
@Component('pointerUp')
export class GlobalPointerUp extends PointerEventComponent {}

/**
 * @public
 */
export class Input {
  private static _instance: Input

  static get instance(): Input {
    Input.ensureInstance()
    return Input._instance
  }

  public get state(): Readonly<InputState> {
    return this.internalState
  }

  private subscriptions: Record<InputEventKind, Array<(e: LocalPointerEvent) => void>> = {
    BUTTON_DOWN: [],
    BUTTON_UP: []
  }

  private internalState: InputState = {
    [Pointer.PRIMARY]: {
      BUTTON_DOWN: false
    },
    [Pointer.SECONDARY]: {
      BUTTON_DOWN: false
    }
  }

  private constructor() {}

  static ensureInstance(): any {
    if (!Input._instance) {
      Input._instance = new Input()
    }
  }

  /**
   * Subscribes to an input event and triggers the provided callback.
   *
   * Returns a function that can be called to remove the subscription.
   * @param eventName - The name of the event (see InputEventKind).
   * @param fn - A callback function to be called when the event is triggered.
   */
  public subscribe(eventName: InputEventKind, fn: (e: LocalPointerEvent) => void) {
    this.subscriptions[eventName].push(fn)
    return () => this.unsubscribe(eventName, fn)
  }

  /**
   * Removes an existing input event subscription.
   * @param eventName - The name of the event (see InputEventKind).
   * @param fn - The callback function used when subscribing to the event.
   */
  public unsubscribe(eventName: InputEventKind, fn: (e: LocalPointerEvent) => void) {
    const index = this.subscriptions[eventName].indexOf(fn)
    if (index > -1) {
      return this.subscriptions[eventName].splice(index, 1)
    }
    return false
  }

  public handlePointerEvent(data: GlobalInputEventResult) {
    const pointer = this.getPointerById(data.pointerId)
    const newData: LocalPointerEvent = {
      ...data,
      pointer,
      direction: new Vector3().copyFrom(data.direction),
      origin: new Vector3().copyFrom(data.origin),
      hit: data.hit
        ? {
            ...data.hit,
            hitPoint: new Vector3().copyFrom(data.hit.hitPoint),
            normal: new Vector3().copyFrom(data.hit.normal),
            worldNormal: new Vector3().copyFrom(data.hit.worldNormal)
          }
        : undefined
    }

    if (data.type === InputEventType.DOWN) {
      this.internalState[Pointer.PRIMARY].BUTTON_DOWN = true

      for (let i = 0; i < this.subscriptions['BUTTON_DOWN'].length; i++) {
        this.subscriptions['BUTTON_DOWN'][i](newData)
      }

      if (newData.hit && newData.hit.entityId && DisposableComponent.engine) {
        const entity = DisposableComponent.engine.entities[newData.hit.entityId]
        const handler = entity && entity.getComponentOrNull(GlobalPointerDown)
        if (handler) {
          handler.callback(newData)
        }
      }
    } else {
      this.internalState[Pointer.PRIMARY].BUTTON_DOWN = false

      for (let i = 0; i < this.subscriptions['BUTTON_UP'].length; i++) {
        this.subscriptions['BUTTON_UP'][i](newData)
      }

      if (newData.hit && newData.hit.entityId && DisposableComponent.engine) {
        const entity = DisposableComponent.engine.entities[newData.hit.entityId]
        const handler = entity && entity.getComponentOrNull(GlobalPointerUp)
        if (handler) {
          handler.callback(newData)
        }
      }
    }
  }

  private getPointerById(id: number): Pointer {
    if (id === 0) return Pointer.PRIMARY
    return Pointer.SECONDARY
  }
}
