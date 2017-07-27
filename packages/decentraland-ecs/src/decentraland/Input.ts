import { DecentralandInterface } from './Types'
import { Vector3 } from './math'

declare let dcl: DecentralandInterface | void

export type InputEventKind = 'BUTTON_A_DOWN' | 'BUTTON_A_UP'

/**
 * @public
 */
export enum Pointer {
  PRIMARY = 'PRIMARY',
  SECONDARY = 'SECONDARY'
}

export type InputState = Record<
  Pointer,
  {
    BUTTON_A_DOWN: boolean
  }
>

export type EnginePointerEvent = {
  /** Origin of the ray */
  from: { x: number; y: number; z: number }
  /** Direction vector of the ray (normalized) */
  direction: { x: number; y: number; z: number }
  /** Length of the ray */
  length: number
  /** ID of the pointer that triggered the event */
  pointerId: number
}

export type PointerEvent = {
  /** Origin of the ray */
  from: Vector3
  /** Direction vector of the ray (normalized) */
  direction: Vector3
  /** Length of the ray */
  length: number
  /** ID of the pointer that triggered the event */
  pointerId: Pointer
}

/**
 * @public
 */
export class Input {
  private static _instance: Input

  static get instance(): Input {
    if (!Input._instance) {
      Input._instance = new Input()
    }
    return Input._instance
  }

  public get state(): Readonly<InputState> {
    return this.internalState
  }

  private subscriptions: Record<InputEventKind, Array<(e: PointerEvent) => void>> = {
    BUTTON_A_DOWN: [],
    BUTTON_A_UP: []
  }

  private internalState: InputState = {
    [Pointer.PRIMARY]: {
      BUTTON_A_DOWN: false
    },
    [Pointer.SECONDARY]: {
      BUTTON_A_DOWN: false
    }
  }

  constructor() {
    if (typeof dcl !== 'undefined') {
      dcl.subscribe('pointerUp')
      dcl.subscribe('pointerDown')

      dcl.onEvent(event => {
        if (event.type === 'pointerUp') {
          this.handlePointerUp(event.data as EnginePointerEvent)
        } else if (event.type === 'pointerDown') {
          this.handlePointerDown(event.data as EnginePointerEvent)
        }
      })
    }
  }

  /**
   * Subscribes to an input event and triggers the provided callback.
   *
   * Returns a function that can be called to remove the subscription.
   * @param eventName - The name of the event (see InputEventKind).
   * @param fn - A callback function to be called when the event is triggered.
   */
  public subscribe(eventName: InputEventKind, fn: (e: PointerEvent) => void) {
    this.subscriptions[eventName].push(fn)
    return () => this.unsubscribe(eventName, fn)
  }

  /**
   * Removes an existing input event subscription.
   * @param eventName - The name of the event (see InputEventKind).
   * @param fn - The callback function used when subscribing to the event.
   */
  public unsubscribe(eventName: InputEventKind, fn: (e: PointerEvent) => void) {
    const index = this.subscriptions[eventName].indexOf(fn)
    if (index > -1) {
      this.subscriptions[eventName].splice(index, 1)
    }
  }

  private getPointerById(id: number): Pointer {
    if (id === 1) return Pointer.PRIMARY
    return Pointer.SECONDARY
  }

  private handlePointerUp(data: EnginePointerEvent) {
    const pointer = this.getPointerById(data.pointerId)
    const newData = {
      length: data.length,
      from: new Vector3(data.from.x, data.from.y, data.from.z),
      direction: new Vector3(data.direction.x, data.direction.y, data.direction.z),
      pointerId: pointer
    }

    this.internalState[Pointer.PRIMARY].BUTTON_A_DOWN = false

    for (let i = 0; i < this.subscriptions['BUTTON_A_UP'].length; i++) {
      this.subscriptions['BUTTON_A_UP'][i](newData)
    }
  }

  private handlePointerDown(data: EnginePointerEvent) {
    const pointer = this.getPointerById(data.pointerId)
    const newData = {
      length: data.length,
      from: new Vector3(data.from.x, data.from.y, data.from.z),
      direction: new Vector3(data.direction.x, data.direction.y, data.direction.z),
      pointerId: pointer
    }

    this.internalState[Pointer.PRIMARY].BUTTON_A_DOWN = true

    for (let i = 0; i < this.subscriptions['BUTTON_A_DOWN'].length; i++) {
      this.subscriptions['BUTTON_A_DOWN'][i](newData)
    }
  }
}
