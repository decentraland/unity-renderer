import { Vector3Component, QuaternionComponent } from './types'

export type PointerEvent = {
  /** Origin of the ray */
  from: Vector3Component
  /** Direction vector of the ray (normalized) */
  direction: Vector3Component
  /** Length of the ray */
  length: number
  /** ID of the pointer that triggered the event */
  pointerId: number
}

export interface IEvents {
  /**
   * `positionChanged` is triggered when the position of the camera changes
   * This event is throttled to 10 times per second.
   */
  positionChanged: {
    /** Position relative to the base parcel of the scene */
    position: Vector3Component

    /** Camera position, this is a absolute world position */
    cameraPosition: Vector3Component

    /** Eye height, in meters. */
    playerHeight: number
  }

  /**
   * `rotationChanged` is triggered when the rotation of the camera changes.
   * This event is throttled to 10 times per second.
   */
  rotationChanged: {
    /** {X,Y,Z} Degree vector. Same as entities */
    rotation: Vector3Component
    /** Rotation quaternion, useful in some scenarios. */
    quaternion: QuaternionComponent
  }

  /**
   * `click` is triggered when a user points and the ray (from mouse or controller) hits the entity.
   * Notice: Only entities with ID will be listening for click events.
   */
  click: {
    /** ID of the entitiy of the event */
    elementId: string

    /** ID of the pointer that triggered the event */
    pointerId: number
  }

  /**
   * `pointerUp` is triggered when the user releases an input pointer.
   * It could be a VR controller, a touch screen or the mouse.
   */
  pointerUp: PointerEvent

  /**
   * `pointerDown` is triggered when the user press an input pointer.
   * It could be a VR controller, a touch screen or the mouse.
   */
  pointerDown: PointerEvent

  /**
   * `chatMessage` is triggered when the user sends a message through chat entity.
   */
  chatMessage: {
    id: string
    sender: string
    message: string
    isCommand: boolean
  }

  /**
   * `onChange` is triggered when an entity changes its own internal state.
   * Dispatched by the `ui-*` entities when their value is changed. It triggers a callback.
   * Notice: Only entities with ID will be listening for click events.
   */
  onChange: {
    value?: any
    /** ID of the pointer that triggered the event */
    pointerId?: number
  }

  /**
   * `onFocus` is triggered when an entity focus is active.
   * Dispatched by the `ui-input` and `ui-password` entities when the value is changed.
   * It triggers a callback.
   *
   * Notice: Only entities with ID will be listening for click events.
   */
  onFocus: {
    /** ID of the entitiy of the event */
    elementId: string
    /** ID of the pointer that triggered the event */
    pointerId: number
  }

  /**
   * `onBlur` is triggered when an entity loses its focus.
   * Dispatched by the `ui-input` and `ui-password` entities when the value is changed.
   *  It triggers a callback.
   *
   * Notice: Only entities with ID will be listening for click events.
   */
  onBlur: {
    /** ID of the entitiy of the event */
    elementId: string
    /** ID of the pointer that triggered the event */
    pointerId: number
  }

  onClick: {
    pointerId: number
  }

  limitsExceeded: {
    given: Record<string, number>
    limit: Record<string, number>
  }
}

export type IEventNames = keyof IEvents

export type RPCEvent<K extends IEventNames, D = any> = {
  event: K
  data: D
}

export function createEvent<T extends IEventNames>(event: T, data: IEvents[T]): RPCEvent<T, IEvents[T]> {
  return { event, data }
}
