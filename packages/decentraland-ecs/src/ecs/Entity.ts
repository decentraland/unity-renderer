import { getComponentName, ComponentConstructor, getComponentClassId, ComponentLike } from './Component'
import { log, Engine } from './Engine'
import { EventManager, EventConstructor } from './EventManager'
import { uuid } from './helpers'

// tslint:disable:no-use-before-declare

/**
 * @public
 */
export class Entity {
  public children: Record<string, Entity> = {}
  public eventManager: EventManager | null = null
  public alive: boolean = false

  public readonly uuid: string = uuid()
  public readonly components: Record<string, any> = {}

  // @internal
  public engine: Engine | null = null

  // @internal
  private _parent: Entity | null = null

  constructor(_parent: Entity | null = null, public name?: string) {
    if (!_parent && this.engine) {
      this._parent = this.engine.rootEntity
    } else {
      this._parent = _parent
    }
  }

  /**
   * Adds or replaces a component in the entity.
   * @param component - component instance.
   */
  set<T extends object>(component: T) {
    const componentName = getComponentName(component)
    if (this.components[componentName]) {
      this.components[componentName] = component
    } else {
      this.add(component)
    }
  }

  /**
   * Returns a boolean indicating if a component is present in the entity.
   * @param component - component class or name
   */
  has(component: ComponentConstructor<any>): boolean {
    const componentName = getComponentName(component)
    return !!this.components[componentName]
  }

  /**
   * Gets a component, if it doesn't exist, it throws an Error.
   * @param component - component class or name
   */
  get<T = any>(component: string): T
  get<T>(component: ComponentConstructor<T>): T
  get<T>(component: ComponentConstructor<T> | string): T {
    const componentName = typeof component === 'string' ? component : getComponentName(component)

    if (!this.components[componentName]) {
      throw new Error(`Can not get component "${componentName}" from entity "${this.identifier}"`)
    }

    return this.components[componentName]
  }

  /**
   * Gets a component, if it doesn't exist, it returns null.
   * @param component - component class or name
   */
  getOrNull<T = any>(component: string): T | null
  getOrNull<T>(component: ComponentConstructor<T>): T | null
  getOrNull<T>(component: ComponentConstructor<T> | string): T | null {
    const componentName = typeof component === 'string' ? component : getComponentName(component)
    return this.components[componentName] || null
  }

  /**
   * Gets a component, if it doesn't exist, it creates the component and returns it.
   * @param component - component class
   */
  getOrCreate<T>(component: ComponentConstructor<T> & { new (): T }): T {
    const componentName = getComponentName(component)
    let ret = this.components[componentName] || null
    if (!ret) {
      ret = new component()
      this.set(ret)
    }
    return ret
  }

  /**
   * Adds a component. If the component already exist, it throws an Error.
   * @param component - component instance.
   */
  add<T extends object>(component: T) {
    const componentName = getComponentName(component)
    const classId = getComponentClassId(component)

    if (this.components[componentName]) {
      throw new Error(`A component of type "${componentName}" is already present in entity "${this.identifier}"`)
    }

    this.components[componentName] = component

    if (this.eventManager) {
      this.eventManager.fireEvent(new ComponentAdded(this, componentName, classId))
    }
  }

  /**
   * Removes a component instance from the entity.
   * @param component - component instance to remove
   */
  remove<T extends object>(component: T) {
    const componentName = getComponentName(component)

    if (this.components[componentName]) {
      delete this.components[componentName]
    } else {
      log(`Entity Warning: Trying to remove inexisting component "${componentName}" from entity "${this.identifier}"`)
    }

    if (this.eventManager) {
      this.eventManager.fireEvent(new ComponentRemoved(this, componentName, component))
    }
  }

  /**
   * Returns true if the entity is already added to the engine.
   * Returns false if no engine was defined.
   */
  isAddedToEngine(): boolean {
    if (!this.engine || !(this.uuid in this.engine.entities)) {
      return false
    }

    return true
  }

  /**
   * Sets the parent entity
   */
  setParent(entity: Entity) {
    let parent = !entity && this.engine ? this.engine.rootEntity : entity
    let currentParent = this.getParent()

    if (entity === this) {
      throw new Error(
        `Failed to set parent for entity "${this.identifier}": An entity can't set itself as a its own parent`
      )
    }

    const circularAncestor = this.getCircularAncestor(entity)

    if (circularAncestor) {
      throw new Error(
        `Failed to set parent for entity "${
          this.identifier
        }": Circular parent references are not allowed (See entity "${circularAncestor}")`
      )
    }

    if (currentParent) {
      delete currentParent.children[this.uuid]
    }

    this._parent = parent || null
    this.registerAsChild()

    if (this.eventManager && this.engine) {
      this.eventManager.fireEvent(new ParentChanged(this, parent))
    }
  }

  /**
   * Gets the parent entity
   */
  getParent(): Entity | null {
    return this._parent
  }

  private get identifier() {
    return this.name || this.uuid
  }

  private getCircularAncestor(ent: Entity): string | null {
    const root = this.engine ? this.engine.rootEntity : null
    let e: Entity | null = ent

    while (e && e !== root) {
      const parent: Entity | null = e.getParent()
      if (parent === this) {
        return e.uuid
      }
      e = parent
    }

    return null
  }

  private registerAsChild() {
    const parent = this.getParent()

    if (this.uuid && parent) {
      parent.children[this.uuid] = this
    }
  }
}

/**
 * @public
 */
@EventConstructor('dcl-component-removed')
export class ComponentRemoved {
  constructor(public entity: Entity, public componentName: string, public component: ComponentLike) {
    // stub
  }
}

/**
 * @public
 */
@EventConstructor('dcl-component-added')
export class ComponentAdded {
  constructor(public entity: Entity, public componentName: string, public classId: number | null) {
    // stub
  }
}

/**
 * @public
 */
@EventConstructor('dcl-parent-changed')
export class ParentChanged {
  constructor(public entity: Entity, public parent: Entity) {
    // stub
  }
}
