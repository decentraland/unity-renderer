import { ComponentLike, ComponentConstructor } from './Component'
import { EventConstructor, EventManager } from './EventManager'
import { Attachable } from './Attachable'

/**
 * @public
 */
export interface ISystem {
  active?: boolean

  activate?(engine: IEngine): void
  deactivate?(): void

  update?(dt: number): void

  onAddEntity?(entity: IEntity): void
  onRemoveEntity?(entity: IEntity): void
}

/**
 * @public
 */
export interface IEngine {
  rootEntity: IEntity
  readonly playerEntity: IEntity
  readonly avatarEntity: IEntity
  readonly entities: Readonly<Record<string, IEntity>>

  addEntity(entity: IEntity): void
  removeEntity(entity: IEntity): void
  addSystem(system: ISystem, priority: number): void
  removeSystem(system: ISystem): void
}

/**
 * @public
 */
export interface IEntity {
  children: Record<string, IEntity>
  eventManager: EventManager | null
  alive: boolean
  readonly uuid: string
  readonly components: Record<string, any>

  isAddedToEngine(): boolean
  getParent(): IEntity | null
  setParent(e: IEntity | Attachable | null): void

  getComponent<T = any>(component: string): T
  getComponent<T>(component: ComponentConstructor<T>): T
  getComponent<T>(component: ComponentConstructor<T> | string): T

  /**
   * Gets a component, if it doesn't exist, it returns null.
   * @param component - component class or name
   */
  getComponentOrNull<T = any>(component: string): T | null
  getComponentOrNull<T>(component: ComponentConstructor<T>): T | null
  getComponentOrNull<T>(component: ComponentConstructor<T> | string): T | null

  getComponentOrCreate<T>(component: ComponentConstructor<T> & { new(): T }): T

  /**
   * Adds a component. If the component already exist, it throws an Error.
   * @param component - component instance.
   */
  addComponent<T extends object>(component: T): void

  addComponentOrReplace<T extends object>(component: T): void

  removeComponent(component: string, triggerRemovedEvent?: boolean): void
  removeComponent<T extends object>(component: T, triggerRemovedEvent?: boolean): void
  removeComponent(component: ComponentConstructor<any>, triggerRemovedEvent?: boolean): void
  removeComponent(component: object | string | Function, triggerRemovedEvent: any): void

  hasComponent<T = any>(component: string): boolean
  hasComponent<T>(component: ComponentConstructor<T>): boolean
  hasComponent<T extends object>(component: T): boolean
  hasComponent<T>(component: ComponentConstructor<T> | string): boolean
}

/**
 * @public
 */
@EventConstructor()
export class ComponentRemoved {
  constructor(public entity: IEntity, public componentName: string, public component: ComponentLike) {
    // stub
  }
}

/**
 * @public
 */
@EventConstructor()
export class ComponentAdded {
  constructor(public entity: IEntity, public componentName: string, public classId: number | null) {
    // stub
  }
}

/**
 * @public
 */
@EventConstructor()
export class ParentChanged {
  constructor(public entity: IEntity, public parent: IEntity | null) {
    // stub
  }
}
