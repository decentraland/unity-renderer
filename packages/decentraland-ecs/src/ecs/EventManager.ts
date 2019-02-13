import { error } from './helpers'

const eventNameSymbol = '__event_name__'

/**
 * @public
 */
export interface IEventConstructor<T> {
  // @internal
  [eventNameSymbol]?: string
  new (...args: any[]): T
}

const takenEventNames: string[] = []

function getEventNameFromConstructor<T>(ctor: IEventConstructor<T>): string {
  if (!(eventNameSymbol in ctor) || typeof ctor[eventNameSymbol] !== 'string') {
    throw new Error('The EventConstructor is not registered')
  }
  return (ctor[eventNameSymbol] as any) as string
}

/**
 * @public
 */
export class EventManager {
  private listeners: Record<string, Array<{ listener: any; fn: (event: any) => void }>> = {}

  addListener<T, X>(eventClass: IEventConstructor<T>, listener: X, listenerFunction: (this: X, event: T) => void) {
    if (!eventClass || typeof (eventClass as any) !== 'function') {
      throw new Error('Invalid EventConstructor')
    }
    const eventName = getEventNameFromConstructor(eventClass)

    let listeners = this.listeners[eventName]

    if (!listeners) {
      listeners = this.listeners[eventName] = []
    }

    for (let i = 0; i < listeners.length; i++) {
      const x = listeners[i]
      if (x.listener === listener) {
        throw new Error('The provided listener is already registered')
      }
    }

    listeners.push({
      listener,
      fn: listenerFunction
    })
  }

  removeListener<X>(listener: X, eventClass: IEventConstructor<any>): boolean {
    if (!eventClass || typeof (eventClass as any) !== 'function') {
      throw new Error('Invalid EventConstructor')
    }

    const eventName = getEventNameFromConstructor(eventClass)

    let listeners = this.listeners[eventName]

    if (!listeners) {
      return false
    }

    for (let i = 0; i < listeners.length; i++) {
      const x = listeners[i]
      if (x.listener === listener) {
        listeners.splice(i, 1)
        return true
      }
    }

    return false
  }

  fireEvent<T extends object>(event: T) {
    const eventName = getEventNameFromConstructor((event as any).constructor)

    let listeners = this.listeners[eventName]

    if (listeners) {
      for (let i = 0; i < listeners.length; i++) {
        try {
          const l = listeners[i]
          l.fn.call(l.listener, event)
        } catch (e) {
          error(e)
        }
      }
    }
  }
}

/**
 * @public
 */
export function EventConstructor(eventName: string): ClassDecorator {
  if (!eventName || typeof (eventName as any) !== 'string') {
    throw new Error(`Invalid event name ${eventName}`)
  }

  if (takenEventNames.indexOf(eventName) !== -1) {
    throw new Error(`The event name ${eventName} is already taken`)
  }

  takenEventNames.push(eventName)

  return (target: any) => {
    target[eventNameSymbol] = eventName
    return target
  }
}
