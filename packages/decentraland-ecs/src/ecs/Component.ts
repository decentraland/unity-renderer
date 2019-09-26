import { newId } from './helpers'
import { EventConstructor } from './EventManager'
import { UIValue } from './UIValue'

const componentSymbol = '__name__symbol_'
const componentClassIdSymbol = '__classId__symbol_'
const componentIdSymbol = '__component__id_'

/**
 * @public
 */
export interface ComponentLike {
  // @internal
  [componentSymbol]?: string
  // @internal
  [componentClassIdSymbol]?: number

  // @internal
  addedToEntity?(entity: any): void
  // @internal
  removedFromEntity?(entity: any): void
}

/**
 * @public
 */
export interface DisposableComponentLike extends ComponentLike {
  // @internal
  [componentIdSymbol]?: string
  onDispose?(): void
}

/**
 * @public
 */
export interface ComponentConstructor<T extends ComponentLike> {
  // @internal
  [componentSymbol]?: string
  // @internal
  [componentClassIdSymbol]?: number
  isComponent?: boolean
  originalClassName?: string
  new (...args: any[]): T
}

/**
 * @public
 */
export interface DisposableComponentConstructor<T extends DisposableComponentLike> {
  // @internal
  [componentSymbol]?: string
  // @internal
  [componentClassIdSymbol]?: number
  isComponent?: boolean
  isDisposableComponent?: true
  originalClassName?: string
  new (...args: any[]): T
}

/**
 * @public
 */
@EventConstructor()
export class DisposableComponentCreated {
  constructor(public componentId: string, public componentName: string, public classId: number) {
    // stub
  }
}

/**
 * @public
 */
@EventConstructor()
export class DisposableComponentRemoved {
  constructor(public componentId: string) {
    // stub
  }
}

/**
 * @public
 */
@EventConstructor()
export class DisposableComponentUpdated {
  constructor(public componentId: string, public component: DisposableComponentLike) {
    // stub
  }
}

/**
 * @public
 */
export function Component(componentName: string, classId?: number) {
  return function<TFunction extends ComponentConstructor<any>>(target: TFunction): TFunction | void {
    if (target.isComponent) {
      throw new TypeError(
        `You cannot extend a component. Trying to extend ${target.originalClassName} with: ${componentName}`
      )
    }

    const extendedClass = target as any

    const RegisteredComponent: any = function RegisteredComponent() {
      const args = Array.prototype.slice.call(arguments)
      const ret = new extendedClass(...args)

      Object.defineProperty(ret, componentSymbol, {
        enumerable: false,
        writable: false,
        configurable: false,
        value: componentName
      })

      if (classId !== undefined) {
        Object.defineProperty(ret, componentClassIdSymbol, {
          enumerable: false,
          writable: false,
          configurable: false,
          value: classId
        })
      }

      return ret
    }

    if (classId !== undefined) {
      RegisteredComponent[componentClassIdSymbol] = classId
    }

    RegisteredComponent[componentSymbol] = componentName
    RegisteredComponent.isComponent = true
    RegisteredComponent.originalClassName = componentName

    RegisteredComponent.prototype = target.prototype
    RegisteredComponent.prototype.constructor = target

    return RegisteredComponent as TFunction
  }
}

/**
 * @public
 */

export function DisposableComponent(componentName: string, classId: number) {
  return function<TFunction extends DisposableComponentConstructor<any>>(target: TFunction): TFunction | void {
    if (target.isComponent) {
      throw new TypeError(
        `You cannot extend a component. Trying to extend ${target.originalClassName} with: ${componentName}`
      )
    }

    if (typeof (classId as any) !== 'number' || isNaN(classId)) {
      throw new Error(`classId: ${classId} is an invalid integer`)
    }

    const extendedClass = target as any

    const RegisteredComponent: any = function RegisteredComponent() {
      if (!DisposableComponent.engine) {
        throw new Error('You need to set a DisposableComponent.engine before creating disposable components')
      }

      const args = Array.prototype.slice.call(arguments)
      const ret = new extendedClass(...args)
      const id = newId('C')

      Object.defineProperty(ret, componentSymbol, {
        enumerable: false,
        writable: false,
        configurable: false,
        value: componentName
      })

      Object.defineProperty(ret, componentIdSymbol, {
        enumerable: false,
        writable: false,
        configurable: false,
        value: id
      })

      if ((classId as any) !== undefined) {
        Object.defineProperty(ret, componentClassIdSymbol, {
          enumerable: false,
          writable: false,
          configurable: false,
          value: classId
        })
      }

      if (DisposableComponent.engine) {
        DisposableComponent.engine.registerComponent(ret)
      }

      return ret
    }

    if ((classId as any) !== undefined) {
      RegisteredComponent[componentClassIdSymbol] = classId
    }

    RegisteredComponent[componentSymbol] = componentName
    RegisteredComponent.isComponent = true
    RegisteredComponent.isDisposableComponent = true
    RegisteredComponent.originalClassName = componentName

    RegisteredComponent.prototype = target.prototype
    RegisteredComponent.prototype.constructor = target

    return RegisteredComponent as TFunction
  }
}

/** @internal */
export namespace DisposableComponent {
  /** @internal */
  // tslint:disable-next-line:whitespace
  export let engine: any = null
}

/**
 * @public
 */
export function getComponentName<T extends Record<any, any> = any>(component: T | ComponentConstructor<T>): string {
  if (!component) {
    throw new TypeError(component + ' is not a component.')
  }
  if (component[componentSymbol]) {
    return component[componentSymbol] as string
  }
  throw new TypeError(component + ' is not a registered component.')
}

/**
 * @public
 */
export function getComponentClassId<T extends Record<any, any> = any>(
  component: T | ComponentConstructor<T>
): number | null {
  if (!component) {
    throw new TypeError(component + ' is not a component.')
  }
  if (component[componentClassIdSymbol]) {
    return component[componentClassIdSymbol] as number
  }
  if (!component[componentSymbol]) {
    throw new TypeError(component + ' is not a registered component.')
  }

  return null
}

/**
 * @public
 */
export function getComponentId<T extends DisposableComponentLike>(component: T): string {
  if (!component) {
    throw new TypeError(component + ' is not a component.')
  }
  if (component[componentIdSymbol]) {
    return (component[componentIdSymbol] as any) as string
  }
  throw new TypeError(component + ' is not a registered disposable component.')
}

/** @public */
export type ObservableComponentSubscription = (key: string, newVal: any, oldVal: any) => void

/**
 * @public
 */
export class ObservableComponent {
  dirty: boolean = false
  data: any = {}
  private subscriptions: Array<ObservableComponentSubscription> = []

  static component(target: ObservableComponent, propertyKey: string) {
    if (delete (target as any)[propertyKey]) {
      const componentSymbol = propertyKey + '_' + Math.random()
      ;(target as any)[componentSymbol] = undefined

      Object.defineProperty(target, componentSymbol, {
        ...Object.getOwnPropertyDescriptor(target, componentSymbol),
        enumerable: false
      })

      Object.defineProperty(target, propertyKey.toString(), {
        get: function() {
          return this[componentSymbol]
        },
        set: function(value) {
          const oldValue = this[componentSymbol]

          if (value) {
            this.data[propertyKey] = getComponentId(value)
          } else {
            this.data[propertyKey] = null
          }

          this[componentSymbol] = value

          if (value !== oldValue) {
            this.dirty = true

            for (let i = 0; i < this.subscriptions.length; i++) {
              this.subscriptions[i](propertyKey, value, oldValue)
            }
          }
        },
        enumerable: true
      })
    }
  }

  static field(target: ObservableComponent, propertyKey: string) {
    if (delete (target as any)[propertyKey]) {
      Object.defineProperty(target, propertyKey.toString(), {
        get: function(this: ObservableComponent) {
          return this.data[propertyKey]
        },
        set: function(this: ObservableComponent, value) {
          const oldValue = this.data[propertyKey]
          this.data[propertyKey] = value

          if (value !== oldValue) {
            this.dirty = true

            for (let i = 0; i < this.subscriptions.length; i++) {
              this.subscriptions[i](propertyKey, value, oldValue)
            }
          }
        },
        enumerable: true
      })
    }
  }

  static uiValue(target: ObservableComponent, propertyKey: string) {
    if (delete (target as any)[propertyKey]) {
      Object.defineProperty(target, propertyKey.toString(), {
        get: function(this: ObservableComponent): string | number {
          return this.data[propertyKey].toString()
        },
        set: function(this: ObservableComponent, value: string | number) {
          const oldValue = this.data[propertyKey]

          const finalValue = new UIValue(value)

          this.data[propertyKey] = finalValue

          if (finalValue !== oldValue) {
            this.dirty = true

            for (let i = 0; i < this.subscriptions.length; i++) {
              this.subscriptions[i](propertyKey, finalValue, oldValue)
            }
          }
        },
        enumerable: true
      })
    }
  }

  static readonly(target: ObservableComponent, propertyKey: string) {
    if (delete (target as any)[propertyKey]) {
      Object.defineProperty(target, propertyKey.toString(), {
        get: function(this: ObservableComponent) {
          if (propertyKey in this.data === false) {
            throw new Error(`The field ${propertyKey} is uninitialized`)
          }
          return this.data[propertyKey]
        },
        set: function(this: ObservableComponent, value) {
          if (propertyKey in this.data) {
            throw new Error(`The field ${propertyKey} is readonly`)
          }
          this.data[propertyKey] = value
          this.dirty = true
        },
        enumerable: true,
        configurable: false
      })
    }
  }

  onChange(fn: ObservableComponentSubscription) {
    this.subscriptions.push(fn)
    return this
  }

  toJSON() {
    return this.data
  }
}

/**
 * @public
 */
export function isDisposableComponent(component: ComponentLike) {
  return componentIdSymbol in component
}
