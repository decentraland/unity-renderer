import { ReadOnlyVector3, ReadOnlyQuaternion, ReadOnlyColor4 } from './math'
import { RaycastResponse } from './Events'

/** @public */
export type ModuleDescriptor = {
  rpcHandle: string
  methods: MethodDescriptor[]
}

/** @public */
export type MethodDescriptor = { name: string }

/** @public */
export type DecentralandInterface = {
  /** are we running in debug mode? */
  DEBUG: boolean

  /** update the entity shape */
  updateEntity?: never

  /** log function */
  log(...a: any[]): void

  /** error function */
  error(message: string, data?: any): void

  // LIFECYCLE

  /** update tick */
  onUpdate(cb: (deltaTime: number) => void): void

  /** called when it is time to wake the sandbox */
  onStart(cb: Function): void

  // ENTITIES

  /** create the entity in the engine */
  addEntity(entityId: string): void

  /** remove the entity from the engine */
  removeEntity(entityId: string): void

  /** called after adding a component to the entity or after updating a component */
  updateEntityComponent(entityId: string, componentName: string, classId: number, json: string): void

  /** called after adding a DisposableComponent to the entity */
  attachEntityComponent(entityId: string, componentName: string, componentId: string): void

  /** called after removing a component from the entity */
  removeEntityComponent(entityId: string, componentName: string): void

  /** set a new parent for the entity */
  setParent(entityId: string, parentId: string): void

  // QUERY

  query(queryType: string, payload: any): void

  // COMPONENTS

  /** called after creating a component in the kernel  */
  componentCreated(componentId: string, componentName: string, classId: number): void

  /** colled after removing a component from the kernel */
  componentDisposed(componentId: string): void

  /** called after globally updating a component */
  componentUpdated(componentId: string, json: string): void

  // EVENTS

  /** event from the engine */
  onEvent(cb: (event: EngineEvent) => void): void

  /** subscribe to specific events, events will be handled by the onEvent function */
  subscribe(eventName: string): void

  /** unsubscribe to specific event */
  unsubscribe(eventName: string): void

  // MODULES

  /** load a module */
  loadModule(moduleName: string): PromiseLike<ModuleDescriptor>

  /** called when calling a module method */
  callRpc(rpcHandle: string, methodName: string, args: ArrayLike<any>): PromiseLike<any>
}

/** @public */
export type InputEventResult = {
  /** Origin of the ray, relative to the scene */
  origin: ReadOnlyVector3
  /** Direction vector of the ray (normalized) */
  direction: ReadOnlyVector3
  /** ID of the pointer that triggered the event */
  buttonId: number
  /** Does this pointer event hit any object? */
  hit?: {
    /** Length of the ray */
    length: number
    /** If the ray hits a mesh the intersection point will be this */
    hitPoint: ReadOnlyVector3
    /** If the mesh has a name, it will be assigned to meshName */
    meshName: string
    /** Normal of the hit */
    normal: ReadOnlyVector3
    /** Normal of the hit, in world space */
    worldNormal: ReadOnlyVector3
    /** Hit entity ID if any */
    entityId: string
  }
}

/** @public */
export enum InputEventType {
  DOWN,
  UP
}

/** @public */
export type GlobalInputEventResult = InputEventResult & {
  type: InputEventType
}

/**
 * @public
 */
export interface IEvents {
  /**
   * `positionChanged` is triggered when the position of the camera changes
   * This event is throttled to 10 times per second.
   */
  positionChanged: {
    /** Camera position relative to the base parcel of the scene */
    position: ReadOnlyVector3

    /** Camera position, this is a absolute world position */
    cameraPosition: ReadOnlyVector3

    /** Eye height, in meters. */
    playerHeight: number
  }

  /**
   * `rotationChanged` is triggered when the rotation of the camera changes.
   * This event is throttled to 10 times per second.
   */
  rotationChanged: {
    /** Degree vector. Same as entities */
    rotation: ReadOnlyVector3
    /** Rotation quaternion, useful in some scenarios. */
    quaternion: ReadOnlyQuaternion
  }

  /**
   * `pointerUp` is triggered when the user releases an input pointer.
   * It could be a VR controller, a touch screen or the mouse.
   */
  pointerUp: InputEventResult

  /**
   * `pointerDown` is triggered when the user press an input pointer.
   * It could be a VR controller, a touch screen or the mouse.
   */
  pointerDown: InputEventResult

  /**
   * `pointerEvent` is triggered when the user press or releases an input pointer.
   * It could be a VR controller, a touch screen or the mouse.
   */
  pointerEvent: GlobalInputEventResult

  /**
   * `raycastResponse` is triggered in response to a raycast query
   */
  raycastResponse: RaycastResponse<any>

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
   * `onEnter` is triggered when the user hits the "Enter" key from the keyboard
   * Used principally by the Chat internal scene
   */
  onEnter: {}

  /**
   * @internal
   * `onPointerLock` is triggered when the user clicks the world canvas and the
   * pointer locks to it so the pointer moves the camera
   */
  onPointerLock: {}

  /**
   * `onAnimationEnd` is triggered when an animation clip gets finish
   */
  onAnimationEnd: {
    clipName: string
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
    entityId: string
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
    entityId: string
    /** ID of the pointer that triggered the event */
    pointerId: number
  }

  /** The onClick event is only used for UI elements */
  onClick: {
    entityId: string
  }

  /**
   * This event gets triggered when an entity leaves the scene fences.
   */
  entityOutOfScene: {
    entityId: string
  }

  /**
   * This event gets triggered when an entity enters the scene fences.
   */
  entityBackInScene: {
    entityId: string
  }

  /**
   * This event gets triggered after receiving a comms message.
   */
  comms: {
    sender: string
    message: string
  }

  /**
   * This is triggered once the scene should start.
   */
  sceneStart: {}

  /**
   * This is triggered once the builder scene is loaded.
   */
  builderSceneStart: {}

  /**
   * This is triggered once the builder scene is unloaded.
   */
  builderSceneUnloaded: {}

  /**
   * After checking entities outside the fences, if any is outside, this event
   * will be triggered with all the entities outside the scene.
   */
  entitiesOutOfBoundaries: {
    entities: string[]
  }

  uuidEvent: {
    uuid: string
    payload: any
  }

  onTextSubmit: {
    text: string
  }

  metricsUpdate: {
    given: Record<string, number>
    limit: Record<string, number>
  }

  limitsExceeded: {
    given: Record<string, number>
    limit: Record<string, number>
  }

  /** For gizmos */
  gizmoEvent: GizmoDragEndEvent | GizmoSelectedEvent

  // @internal
  externalAction: {
    type: string
    [key: string]: any
  }
}

/** @public */
export type GizmoDragEndEvent = {
  type: 'gizmoDragEnded'
  transforms: {
    position: ReadOnlyVector3
    rotation: ReadOnlyQuaternion
    scale: ReadOnlyVector3
    entityId: string
  }[]
}

/** @public */
export type GizmoSelectedEvent = {
  type: 'gizmoSelected'
  gizmoType: 'MOVE' | 'ROTATE' | 'SCALE' | 'NONE'
  entities: string[]
}

/** @public */
export type IEventNames = keyof IEvents

/** @public */
export type EngineEvent<T extends IEventNames = IEventNames, V = IEvents[T]> = {
  /** eventName */
  type: T
  data: V
}

// @internal
export const AVATAR_OBSERVABLE = 'AVATAR_OBSERVABLE'

/**
 * @public
 */
export type WearableId = string

/**
 * @public
 */
export type AvatarForRenderer = {
  bodyShape: WearableId
  skinColor: ReadOnlyColor4
  hairColor: ReadOnlyColor4
  eyeColor: ReadOnlyColor4
  wearables: WearableId[]
}

/**
 * @public
 */
export type Wearable = {
  id: WearableId
  type: 'wearable'
  category: string
  baseUrl: string
  tags: string[]
  representations: BodyShapeRespresentation[]
}

/**
 * @public
 */
export type BodyShapeRespresentation = {
  bodyShapes: string[]
  mainFile: string
  contents: FileAndHash[]
}

/**
 * @public
 */
export type FileAndHash = {
  file: string
  hash: string
}

/**
 * @public
 */
export type ProfileForRenderer = {
  userId: string
  name: string
  description: string
  email: string
  avatar: AvatarForRenderer
  inventory: WearableId[]
  snapshots: {
    face: string
    body: string
  }
  version: number
  hasConnectedWeb3: boolean
  updatedAt?: number
  createdAt?: number
}

/**
 * @public
 */
export type MinimapSceneInfo = {
  name: string
  owner: string
  description: string
  previewImageUrl: string | undefined
  type: number
  parcels: { x: number; y: number }[]
  isPOI: boolean
}
