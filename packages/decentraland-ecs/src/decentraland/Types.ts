export type Component = any

export type ModuleDescriptor = {
  rpcHandle: string
  methods: MethodDescriptor[]
}

export type MethodDescriptor = { name: string }

export type EngineEvent = {
  /** eventName */
  type: string

  data: Record<string, any>
}

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
