import { EventDispatcher } from '../core/EventDispatcher'
import * as JsonRpc2 from './types'

/**
 * Creates a RPC Client.
 * It is intentional that Client does not create a WebSocket object since we prefer composability
 */
export abstract class Client extends EventDispatcher implements JsonRpc2.IClient {
  private _responsePromiseMap: Map<number, JsonRpc2.Resolvable> = new Map()
  private _nextMessageId: number = 0
  private _consoleLog: boolean = false
  private _requestQueue: string[] = []
  private _connected = false

  constructor(opts?: JsonRpc2.IClientOpts) {
    super()
    this.setLogging(opts)
  }

  abstract sendMessage(message: string): void

  public processMessage(messageStr: string | (JsonRpc2.IResponse & JsonRpc2.INotification)) {
    let message: JsonRpc2.IResponse & JsonRpc2.INotification

    if (typeof messageStr === 'string') {
      this._logMessage(messageStr, 'receive')

      // Ensure JSON is not malformed
      try {
        message = JSON.parse(messageStr)
      } catch (e) {
        return this.emit('error', e)
      }
    } else {
      message = messageStr
    }

    // Check that messages is well formed
    if (!message) {
      this.emit('error', new Error(`Message cannot be null, empty or undefined`))
    } else if (message.id) {
      if (this._responsePromiseMap.has(message.id)) {
        // Resolve promise from pending message
        const promise = this._responsePromiseMap.get(message.id) as JsonRpc2.Resolvable
        this._responsePromiseMap.delete(message.id)

        if ('result' in message) {
          promise.resolve(message.result)
        } else if ('error' in message) {
          const error = Object.assign(
            new Error('Remote error'),
            message.error,
            (message.error && message.error.data) || {}
          )
          promise.reject(error)
        } else {
          promise.reject(
            Object.assign(new Error(`Response must have result or error: ${messageStr}`), {
              code: JsonRpc2.ErrorCode.ParseError
            })
          )
        }
      } else {
        this.emit('error', new Error(`Response with id:${message.id} has no pending request`))
      }
    } else if (message.method) {
      // Server has sent a notification
      this.emit(message.method, message.params)
    } else {
      this.emit('error', new Error(`Invalid message: ${messageStr}`))
    }
  }

  /**
   * Set logging for all received and sent messages
   */
  public setLogging({ logConsole }: JsonRpc2.ILogOpts = {}) {
    this._consoleLog = !!logConsole
  }

  call(method: string): Promise<any>
  call(method: string, params: string): never
  call(method: string, params: number): never
  call(method: string, params: boolean): never
  call(method: string, params: null): never
  call<T>(method: string, params: Iterable<T>): Promise<any>
  call(method: string, params: { [key: string]: any }): Promise<any>
  call(method: string, params?: any) {
    if (typeof params !== 'undefined' && typeof params !== 'object') {
      throw new Error(`Client#call Params must be structured data (Array | Object) got ${JSON.stringify(params)}`)
    }

    const id = ++this._nextMessageId
    const message: JsonRpc2.IRequest = { id, method, params, jsonrpc: '2.0' }

    return new Promise((resolve, reject) => {
      try {
        this._responsePromiseMap.set(id, { resolve, reject })
        this._send(message)
      } catch (error) {
        return reject(error)
      }
    })
  }

  notify(method: string): void
  notify(method: string, params: string): never
  notify(method: string, params: number): never
  notify(method: string, params: boolean): never
  notify(method: string, params: null): never
  notify<T>(method: string, params: Iterable<T>): void
  notify(method: string, params: { [key: string]: any }): void
  notify(method: string, params?: any): void {
    if (typeof params !== 'undefined' && typeof params !== 'object') {
      throw new Error(`Client#notify Params must be structured data (Array | Object) got ${JSON.stringify(params)}`)
    }

    this._send({ method, params, jsonrpc: '2.0' })
  }

  protected didConnect() {
    if (this._connected === false) {
      this._connected = true
      this._sendQueuedRequests()
    }
  }

  private _send(message: JsonRpc2.INotification | JsonRpc2.IRequest) {
    this._requestQueue.push(JSON.stringify(message))

    this._sendQueuedRequests()
  }

  private _sendQueuedRequests() {
    if (this._connected) {
      const queue = this._requestQueue.splice(0, this._requestQueue.length)
      for (let messageStr of queue) {
        this._logMessage(messageStr, 'send')
        this.sendMessage(messageStr)
      }
    }
  }

  private _logMessage(message: string, direction: 'send' | 'receive') {
    if (this._consoleLog) {
      console.log(`Client ${direction === 'send' ? '>' : '<'}`, message.toString())
    }
  }
}
