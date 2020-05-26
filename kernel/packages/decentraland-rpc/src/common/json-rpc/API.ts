import { Client } from './Client'

const blacklistedMethods = ['then', 'catch']

/**
 * Builds an ES6 Proxy where api.domain.method(params) transates into client.send('{domain}.{method}', params) calls
 * api.domain.on{method} will add event handlers for {method} events
 * api.domain.emit{method} will send {method} notifications to the server
 * The api object leads itself to a very clean interface i.e `await api.Domain.func(params)` calls
 * This allows the consumer to abstract all the internal details of marshalling the message from function call to a string
 * Calling client.api('') will return an unprefixed client. e.g api.hello() is equivalient to client.send('hello')
 */
export function getApi<T extends object = any>(rpcClient: Client, _prefix: string = ''): T {
  if (!Proxy) {
    throw new Error('getApi() requires ES6 Proxy. Please use an ES6 compatible engine')
  }

  const prefix = _prefix === '' ? '' : `${_prefix}.`

  return new Proxy({} as T, {
    get: (target: any, prop: string) => {
      if (target[prop]) {
        return target[prop]
      }
      // Special handling for prototype so console intellisense works on objects
      if (prop === '__proto__' || prop === 'prototype') {
        return Object.prototype
      } else if (prop.substr(0, 2) === 'on' && prop.length > 3) {
        const method = prop.substr(2)
        target[prop] = (handler: Function) =>
          rpcClient.on(`${prefix}${method}`, (params: any) => {
            try {
              if (params && params instanceof Array) {
                handler.apply(null, params)
              } else {
                handler.call(null, params)
              }
            } catch (e) {
              rpcClient.emit('error', e)
            }
          })
      } else if (prop.substr(0, 4) === 'emit' && prop.length > 5) {
        const method = prop.substr(4)
        target[prop] = (...args: any[]) => rpcClient.notify(`${prefix}${method}`, args)
      } else if (blacklistedMethods.indexOf(prop) !== -1) {
        return undefined
      } else {
        const method = prop
        target[prop] = (...args: any[]) => rpcClient.call(`${prefix}${method}`, args)
      }

      return target[prop]
    }
  })
}
