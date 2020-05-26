import * as JsonRpc2 from './types'

import { EventDispatcher, EventDispatcherBinding } from '../core/EventDispatcher'
import { isPromiseLike } from '../core/isPromiseLike'

const errorColumns = { name: 1, message: 1, stack: 1, columnNumber: 1, fileName: 1, lineNumber: 1 }

function sanitizeError(error: any) {
  if (error instanceof Error) {
    const ret: any = Object.assign({}, error)

    for (let i in errorColumns) {
      ret[i] = (error as any)[i]
    }

    return ret
  } else {
    return error
  }
}

/**
 * Creates a RPC Server.
 * It is intentional that Server does not create a Worker object since we prefer composability
 */
export abstract class Server<ClientType = any> extends EventDispatcher implements JsonRpc2.IServer {
  private _exposedMethodsMap: Map<string, (params: any) => JsonRpc2.PromiseOrNot<any>> = new Map()
  private _consoleLog: boolean = false
  private _isEnabled = false

  get isEnabled(): boolean {
    return this._isEnabled
  }

  constructor(opts: JsonRpc2.IServerOpts = {}) {
    super()
    this.setLogging(opts)
  }

  abstract sendMessage(to: ClientType, message: string): void
  abstract getAllClients(): Iterable<ClientType>

  on(method: 'error', callback: (error: any) => void, once?: boolean): EventDispatcherBinding
  on(method: string, callback: (params: any, sender: ClientType) => void, once?: boolean): EventDispatcherBinding
  on(method: string, callback: (params: any, sender: ClientType) => void, once?: boolean): EventDispatcherBinding {
    return super.on(method, callback, once)
  }

  once(method: 'error', callback: (error: any) => void): EventDispatcherBinding
  once(method: string, callback: (params: any, sender: ClientType) => void): EventDispatcherBinding
  once(method: string, callback: (params: any, sender: ClientType) => void): EventDispatcherBinding {
    return super.once(method, callback)
  }

  /**
   * Set logging for all received and sent messages
   */
  public setLogging({ logConsole }: JsonRpc2.ILogOpts = {}) {
    this._consoleLog = !!logConsole
  }

  expose(method: string, handler: (...params: any[]) => Promise<any>): void {
    this._exposedMethodsMap.set(method, handler)
  }

  notify(method: string): void
  notify(method: string, params: string): never
  notify(method: string, params: number): never
  notify(method: string, params: boolean): never
  notify(method: string, params: null): never
  notify<T>(method: string, params: Iterable<T>): void
  notify(method: string, params?: Object): void
  notify(method: string, params?: any): void {
    if (typeof params !== 'undefined' && typeof params !== 'object') {
      throw new Error(`Server#notify Params must be structured data (Array | Object) got ${JSON.stringify(params)}`)
    }
    // Broadcast message to all clients
    const clients = this.getAllClients()

    if (clients) {
      for (let client of clients) {
        this._send(client, { method, params, jsonrpc: '2.0' })
      }
    } else {
      throw new Error('Server does not support broadcasting. No "getAllClients: ClientType[]" returned null')
    }
  }

  /**
   * Execute this method after configuring the RPC methods and listeners.
   * It will send an empty notification to the client, then it (the client) will send all the enqueued messages.
   */
  protected enable() {
    if (!this._isEnabled) {
      this._isEnabled = true
      this.notify('RPC.Enabled')
    }
  }

  protected disable() {
    if (this._isEnabled) {
      this._isEnabled = false
    }
  }

  protected processMessage(from: ClientType, messageStr: string): void {
    this._logMessage(messageStr, 'receive')
    let request: JsonRpc2.IRequest

    try {
      if (typeof (messageStr as any) === 'string' && messageStr.charAt(0) === '{') {
        // Ensure JSON is not malformed
        request = JSON.parse(messageStr)
      } else {
        throw new Error(`Unable to parse message ${JSON.stringify(messageStr)}`)
      }
    } catch (e) {
      return this._sendError(from, null, JsonRpc2.ErrorCode.ParseError, e)
    }

    // Ensure method is atleast defined
    if (request && request.method && typeof (request.method as any) === 'string') {
      if (request.id && typeof (request.id as any) === 'number') {
        const handler = this._exposedMethodsMap.get(request.method)
        // Handler is defined so lets call it
        if (handler) {
          if (request.params && typeof request.params !== 'object') {
            this._sendError(
              from,
              request,
              JsonRpc2.ErrorCode.InvalidParams,
              new Error('params is not an Array or Object')
            )
          } else {
            try {
              const result: JsonRpc2.PromiseOrNot<any> =
                request.params instanceof Array
                  ? handler.apply(this, request.params as any)
                  : handler.call(this, request.params)

              if (isPromiseLike(result)) {
                // Result is a promise, so lets wait for the result and handle accordingly
                result
                  .then((actualResult: any) => {
                    this._send(from, {
                      jsonrpc: '2.0',
                      id: request.id,
                      result: typeof actualResult === 'undefined' ? null : actualResult
                    })
                  })
                  .catch((error: Error) => {
                    this._sendError(from, request, JsonRpc2.ErrorCode.InternalError, error)
                  })
              } else {
                // Result is not a promise so send immediately
                this._send(from, {
                  jsonrpc: '2.0',
                  id: request.id,
                  result: typeof result === 'undefined' ? null : result
                })
              }
            } catch (error) {
              this._sendError(from, request, JsonRpc2.ErrorCode.InternalError, error)
            }
          }
        } else {
          console.log(`Method ${request.method} not present in `, this._exposedMethodsMap)
          this._sendError(from, request, JsonRpc2.ErrorCode.MethodNotFound)
        }
      } else {
        // Message is a notification, so just emit
        this.emit(request.method, request.params)
      }
    } else {
      // No method property, send InvalidRequest error
      this._sendError(from, request, JsonRpc2.ErrorCode.InvalidRequest)
    }
  }

  private _logMessage(messageStr: string, direction: 'send' | 'receive') {
    if (this._consoleLog) {
      console.log(`${direction === 'send' ? 'Server > Client' : 'Server < Client'}`, messageStr)
    }
  }

  private _send(receiver: ClientType, message: JsonRpc2.IResponse | JsonRpc2.INotification) {
    let messageStr: string

    messageStr = JSON.stringify(message)

    this._logMessage(messageStr, 'send')
    this.sendMessage(receiver, messageStr)
  }

  private _sendError(
    receiver: ClientType,
    request: JsonRpc2.IRequest | null,
    errorCode: JsonRpc2.ErrorCode,
    error?: Error
  ) {
    try {
      this._send(receiver, {
        jsonrpc: '2.0',
        id: (request && request.id) || -1,
        error: this._errorFromCode(errorCode, sanitizeError(error), request && request.method)
      })
    } catch (error) {
      // Since we can't even send errors, do nothing. The connection was probably closed.
    }
  }

  private _errorFromCode(code: JsonRpc2.ErrorCode, data: any = null, method: string | null = null): JsonRpc2.IError {
    let message = ''

    switch (code) {
      case JsonRpc2.ErrorCode.InternalError:
        message = `InternalError: Internal Error when calling '${method}'`
        break
      case JsonRpc2.ErrorCode.MethodNotFound:
        message = `MethodNotFound: '${method}' wasn't found`
        break
      case JsonRpc2.ErrorCode.InvalidRequest:
        message = 'InvalidRequest: JSON sent is not a valid request object'
        break
      case JsonRpc2.ErrorCode.ParseError:
        message = 'ParseError: invalid JSON received'
        break
    }

    return { code, message, data }
  }
}
