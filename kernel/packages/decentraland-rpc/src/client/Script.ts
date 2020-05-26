import { Client } from '../common/json-rpc/Client'
import { getApi } from '../common/json-rpc/API'
import { ILogOpts, ScriptingTransport } from '../common/json-rpc/types'
import { isPromiseLike } from '../common/core/isPromiseLike'
import { hasOwnSymbol } from '../common/core/SymbolShim'

/** this is defined in the constructor ScriptingHost() */
const loadAPIsNotificationName = 'LoadComponents'

// If there is no native Symbol
// nor polyfill, then a plain number is used for performance.

// tslint:disable-next-line
const hasSymbol = typeof Symbol === 'function' && Symbol.for

const injectedAPISymbol = hasSymbol ? Symbol('injectedAPIs') : 0xfea0

interface Script {
  systemDidEnable?(): Promise<void> | void
}

export type API = any

/**
 * This function decorates parameters to load APIs
 * @param apiName name of the API to load
 */
export function inject(apiName?: string) {
  if (apiName !== undefined && !apiName) {
    throw new TypeError('API name cannot be null / empty')
  }
  return function<T extends Script>(target: T, propertyKey: keyof T) {
    if (typeof propertyKey === 'string') {
      getInjectableMap(target).set(propertyKey, apiName || propertyKey)
    } else throw new TypeError('Cannot inject APIs with non-string names')
  }
}

/**
 * Gets all the injected APIs of a script
 * @param instance A script to get the APIs
 */
export function getInjectedAPIs<T extends Script>(instance: T): Map<keyof T, string> {
  const result = new Map<keyof T, string>()
  let currentPrototype = Object.getPrototypeOf(instance)

  while (!!currentPrototype) {
    if (hasOwnSymbol(currentPrototype, injectedAPISymbol)) {
      const currentList: Map<keyof T, string> = currentPrototype[injectedAPISymbol]
      currentList.forEach((v, k) => result.set(k, v))
    }
    currentPrototype = Object.getPrototypeOf(currentPrototype)
  }

  return result
}

function getInjectableMap(target: Script): Map<any, string> {
  const anyTarget: any = target

  if (!hasOwnSymbol(target, injectedAPISymbol)) {
    anyTarget[injectedAPISymbol] = new Map()
  }

  return anyTarget[injectedAPISymbol]
}

async function _injectAPIs(target: Script) {
  let injectedMap: Map<keyof Script, string> = getInjectedAPIs(target)

  if (injectedMap.size === 0) return

  await target.loadAPIs(Array.from(injectedMap.values()))

  injectedMap.forEach(function(apiName: string, property) {
    target[property] = target.loadedAPIs[apiName]
  })
}

class Script extends Client {
  static inject = inject

  loadedAPIs: { [key: string]: API } = {}

  protected started = false

  constructor(private transport: ScriptingTransport, opt?: ILogOpts) {
    super(opt)

    if (transport.onError) {
      transport.onError(e => {
        this.emit('error', e)
      })
    }

    if (transport.onClose) {
      transport.onClose(() => {
        this.emit('transportClosed')
      })
    }

    transport.onMessage(message => {
      this.processMessage(message)
    })

    if (transport.onConnect) {
      transport.onConnect(() => {
        this.didConnect()
      })
    } else {
      this.didConnect()
    }
  }

  sendMessage(message: string) {
    this.transport.sendMessage(message)
  }

  /**
   * Provide a global point of access to a service without
   * coupling users to the concrete class that implements it.
   *
   * @param apiName Name of the plugin we are trying to obtain
   * @returns {object} loadedAPIs
   */
  async loadAPIs(apiName: string[]): Promise<{ [key: string]: any }> {
    const loadedKeys = Object.keys(this.loadedAPIs)

    const keysToRequest = apiName.filter(function($) {
      return !loadedKeys.includes($)
    })

    if (keysToRequest.length) {
      await this.call(loadAPIsNotificationName, [keysToRequest])

      // Load / request the API
      keysToRequest.forEach(async apiName => {
        this.loadedAPIs[apiName] = getApi(this, apiName)
      })
    }

    return this.loadedAPIs
  }

  protected didConnect() {
    const injection = _injectAPIs(this)

    super.didConnect()

    injection
      .then(() => {
        if (this.systemDidEnable && !this.started) {
          this.started = true
          try {
            const r = this.systemDidEnable()
            if (r && isPromiseLike(r)) {
              r.catch(e => this.emit('error', e))
            }
          } catch (e) {
            this.emit('error', e)
          }
        }
      })
      .catch(e => this.emit('error', e))
  }
}

export { Script }
