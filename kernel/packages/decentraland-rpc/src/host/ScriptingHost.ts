import { Dictionary } from '../common/core/EventDispatcher'
import { TransportBasedServer } from './TransportBasedServer'
import { APIClass, API, APIOptions } from './API'
import { ScriptingTransport } from '../common/json-rpc/types'
import { hasOwnSymbol } from '../common/core/SymbolShim'

// If there is no native Symbol
// nor polyfill, then a plain number is used for performance.
// tslint:disable-next-line
const hasSymbol = typeof Symbol === 'function' && Symbol.for

const apiNameSymbol: any = hasSymbol ? Symbol('pluginName') : 0xfea2

const registeredAPIs: Dictionary<APIClass<API>> = {}

namespace PrivateHelpers {
  export function _registerAPI(apiName: string, api: APIClass<API>) {
    const hasName = hasOwnSymbol(api, apiNameSymbol)
    if (hasName) {
      throw new Error(`The API you are trying to register is already registered`)
    }

    if (apiName in registeredAPIs) {
      throw new Error(`The API ${apiName} is already registered`)
    }

    if (typeof (api as any) !== 'function') {
      throw new Error(`The API ${apiName} is not a class, it is of type ${typeof api}`)
    }

    // save the registered name
    // tslint:disable-next-line:semicolon
    ;(api as any)[apiNameSymbol] = apiName

    registeredAPIs[apiName] = api
  }

  export function unmountAPI(api: API) {
    if (api.apiWillUnmount) {
      const promise = api.apiWillUnmount()
      if (promise && 'catch' in promise) {
        promise.catch(error => console.error('Error unmounting API', { api, error }))
      }
    }
  }

  export function mountAPI(api: API) {
    if (api.apiDidMount) {
      const promise = api.apiDidMount()
      if (promise && 'catch' in promise) {
        promise.catch(error => console.error('Error mounting API', { api, error }))
      }
    }
  }
}

// HERE WE START THE EXPORTS

export enum ScriptingHostEvents {
  systemWillUnmount = 'systemWillUnmount',
  systemWillEnable = 'systemWillEnable',
  systemDidUnmount = 'systemDidUnmount'
}

export function getAPIName(klass: APIClass<API>): string | null {
  return (klass as any)[apiNameSymbol] || klass.name || null
}

export function registerAPI(apiName: string): (klass: APIClass<API>) => void {
  return function(api: APIClass<API>) {
    PrivateHelpers._registerAPI(apiName, api)
  }
}

export class ScriptingHost extends TransportBasedServer {
  unmounted = false

  apiInstances: Map<string, API> = new Map()

  private constructor(worker: ScriptingTransport) {
    super(worker)

    this.expose('LoadComponents', this.RPCLoadAPIs.bind(this))
  }

  static async fromTransport(transport: ScriptingTransport) {
    return new ScriptingHost(transport)
  }

  /**
   * This methdod should be called only from the interface that manages the ScriptingHosts.
   * It initializes the system and it's queued components. It also sends a first notification
   * to the implementation of the system telling it is now enabled. In that moment, the
   * implementation will send the queued messages and execute the queued methods against the
   * materialized components.
   *
   * It:
   *  1) emits a ComponentSystemEvents.systemWillEnable event
   *  2) mounts all the components
   *  3) sends the notification to the actual system implementation
   */
  enable() {
    this.emit(ScriptingHostEvents.systemWillEnable)
    this.apiInstances.forEach(PrivateHelpers.mountAPI)
    super.enable()
  }

  /**
   * This is a service locator, it locates or instantiate the requested component
   * for this instance of ComponentSystem.
   *
   * @param api A class constructor
   */
  getAPIInstance<X>(api: { new (options: APIOptions): X }): X

  /**
   * This is a service locator, it locates or instantiate the requested component
   * for this instance of ComponentSystem.
   *
   * @param name The name of used to register the component
   */
  getAPIInstance(name: string): API | null

  getAPIInstance(api: any) {
    if (typeof api === 'string') {
      if (this.apiInstances.has(api)) {
        return this.apiInstances.get(api)
      }
      if (api in registeredAPIs) {
        return this.initializeAPI(registeredAPIs[api])
      }
      return null
    } else if (typeof api === 'function') {
      const apiName = getAPIName(api)

      // if it has a name, use that indirection to find in the instance's map
      if (apiName !== null) {
        if (this.apiInstances.has(apiName)) {
          return this.apiInstances.get(apiName)
        }

        // If we don't have a local instance, create the instance of the component
        return this.initializeAPI(api)
      }
    }

    throw Object.assign(new Error('Cannot get instance of the specified component'), { api })
  }

  /**
   * This method unmounts all the components and releases the Worker
   */
  unmount() {
    if (this.unmounted) return
    this.notify('SIGKILL')

    this.emit(ScriptingHostEvents.systemWillUnmount)

    try {
      this.apiInstances.forEach(PrivateHelpers.unmountAPI)
      this.apiInstances.clear()
    } catch (e) {
      this.emit('error', e)
    }

    this.transport.close()

    this.emit(ScriptingHostEvents.systemDidUnmount)

    this.unmounted = true
  }

  protected initializeAPI<X extends API>(ctor: {
    new (options: APIOptions): X
    factory?(ctor: { new (options: APIOptions): X }, options: APIOptions): X
  }): X {
    const apiName = getAPIName(ctor)

    if (apiName === null) {
      throw new Error('The plugin is not registered')
    }

    if (this.apiInstances.has(apiName)) {
      return this.apiInstances.get(apiName) as X
    }

    const apiOptions: APIOptions = {
      apiName,
      on: (event, handler) => this.on(`${apiName}.${event}`, handler),
      notify: (event, params?) => this.notify(`${apiName}.${event}`, params),
      expose: (event, handler) => this.expose(`${apiName}.${event}`, handler),
      getAPIInstance: (name: any) => {
        // tslint:disable-next-line
        return this.getAPIInstance(name) as any
      },
      system: this
    }

    const instance = ctor.factory ? ctor.factory(ctor, apiOptions) : new ctor(apiOptions)

    this.apiInstances.set(apiName, instance)

    if (this.isEnabled) {
      PrivateHelpers.mountAPI(instance)
    }

    return instance
  }

  /**
   * Preloads a list of components
   */
  private async RPCLoadAPIs(apiNames: string[]) {
    // tslint:disable-next-line
    if (typeof apiNames !== 'object' || !(apiNames instanceof Array)) {
      throw new TypeError('RPCLoadComponents(names) name must be an array of strings')
    }

    const notFound = apiNames
      .map(name => ({ api: this.getAPIInstance(name), name }))
      .filter($ => $.api === null)
      .map($ => $.name)

    if (notFound.length) {
      const message = `Components not found ${notFound.join(',')}`
      throw new TypeError(message)
    }
  }
}
