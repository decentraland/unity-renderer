import { hasOwnSymbol } from '../common/core/SymbolShim'

// http://gameprogrammingpatterns.com/component.html

// If there is no native Symbol
// nor polyfill, then a plain number is used for performance.
// tslint:disable-next-line
const hasSymbol = typeof Symbol === 'function' && Symbol.for

const exposedMethodSymbol = hasSymbol ? Symbol('exposedMethod') : 0xfea1

export function exposeMethod<T extends API>(
  target: T,
  propertyKey: keyof T,
  descriptor: TypedPropertyDescriptor<ExposableMethod>
) {
  const anyTarget: any = target
  if (!hasOwnSymbol(target, exposedMethodSymbol)) {
    anyTarget[exposedMethodSymbol] = new Set()
  }

  anyTarget[exposedMethodSymbol].add(propertyKey)
}

export function getExposedMethods<T extends API>(instance: T): Set<keyof T> {
  const result = new Set<keyof T>()
  let currentPrototype = Object.getPrototypeOf(instance)

  while (!!currentPrototype) {
    if (hasOwnSymbol(currentPrototype, exposedMethodSymbol)) {
      const currentList: Set<string> = currentPrototype[exposedMethodSymbol]
      currentList.forEach($ => result.add($ as any))
    }
    currentPrototype = Object.getPrototypeOf(currentPrototype)
  }

  return result
}

export function rateLimit<T>(interval: number = 100) {
  return function(
    target: T,
    propertyKey: string,
    descriptor: TypedPropertyDescriptor<(...args: any[]) => Promise<any | void>>
  ) {
    const originalValue = descriptor.value as Function
    let lastCall: number = performance.now()

    return {
      ...descriptor,
      value: function(this: T) {
        const now = performance.now()

        if (now - lastCall < interval) {
          return Promise.reject(new Error('Rate limit exceeded'))
        }

        lastCall = now
        return originalValue.apply(this, arguments)
      }
    }
  }
}

export function throttle<T>(callLimit: number, interval: number = 100) {
  return function(
    target: T,
    propertyKey: string,
    descriptor: TypedPropertyDescriptor<(...args: any[]) => Promise<any | void>>
  ) {
    const originalValue = descriptor.value as Function
    let initTime: number = performance.now()
    let calls = 0

    return {
      ...descriptor,
      value: function(this: T) {
        const now = performance.now()

        if (now - initTime >= interval) {
          calls = 0
          initTime = now
        }

        if (calls >= callLimit) {
          return Promise.reject(new Error('Throttling – Maximum rate exceeded'))
        }

        calls++

        return originalValue.apply(this, arguments)
      }
    }
  }
}

export type APIOptions = {
  apiName: string
  system: any
  on(event: string, handler: <A, O extends object>(params: Array<A> | O) => void): void
  notify(event: string, params?: Object | Array<any>): void
  expose(event: string, handler: <A, O extends object>(params: Array<A> | O) => Promise<any>): void
  getAPIInstance<X>(component: { new (options: APIOptions): X }): X
  getAPIInstance(name: string): API | null
}

export type APIClass<T> = {
  new (options: APIOptions): T
}

export type ExposableMethod = (...args: any[]) => Promise<any>

// we use an interface here because it is mergable with the class
export interface API {
  apiDidMount?(): Promise<void> | void
  apiWillUnmount?(): Promise<void> | void
}

/**
 * This pattern bears resemblance to the Gang of Four’s Strategy pattern.
 * Both patterns are about taking part of an object’s behavior and delegating
 * it to a separate subordinate object. The difference is that with the Strategy
 * pattern, the separate “strategy” object is usually stateless—it encapsulates
 * an algorithm, but no data. It defines how an object behaves, but not what it is.
 *
 * http://wiki.c2.com/?StrategyPattern
 *
 * Components are a bit more self-important. They often hold state that describes
 * the object and helps define its actual identity. However, the line may blur.
 * You may have some components that don’t need any local state. In that case,
 * you’re free to use the same component instance across multiple container objects.
 * At that point, it really is behaving more akin to a strategy.
 *
 * Components are located via service locators managed by the ComponentSystem
 *
 * A Components class defines an abstract interface to a set of operations.
 * That interface is exposed via @exposeMethod decorator. A concrete service
 * provider implements this interface. A separate service locator (ComponentSystem)
 * provides access to the service by finding an appropriate provider while hiding
 * both the provider’s concrete type and implementation and the process used to
 * locate it.
 */
export abstract class API {
  static expose = exposeMethod

  constructor(protected options: APIOptions) {
    for (let methodName of getExposedMethods(this)) {
      const theMethod: any = this[methodName]
      if (typeof theMethod === 'function') {
        if (typeof methodName === 'string') {
          this.options.expose(methodName, theMethod.bind(this))
        }
      }
    }
  }

  static factory(ctor: APIClass<API>, options: APIOptions) {
    return new ctor(options)
  }
}

export abstract class SubscribableAPI extends API {
  abstract async subscribe(event: string): Promise<void>
}

export interface ISubscribableAPI {
  subscribe(event: string): Promise<void>
  unsubscribe(event: string): Promise<void>
  onSubscribedEvent(fn: (data: any) => void): void
}
