// tslint:disable-next-line:whitespace
type DevToolsServer = import('../../shared/apis/DevTools').DevToolsServer
import { Protocol } from 'devtools-protocol'

export class DevToolsAdapter {
  exceptions: Error[] = []

  constructor(public api: DevToolsServer) {
    // sutb
  }

  get now() {
    return performance.now()
  }

  log(...args: any[]) {
    const params: Protocol.Runtime.ConsoleAPICalledEvent = {
      type: 'log',
      timestamp: this.now,
      executionContextId: 0,
      args: args.map($ => {
        let value = undefined
        let unserializableValue = undefined
        const type = typeof $

        if (type === 'object' && $ !== null) {
          try {
            JSON.stringify($)
            value = $
          } catch (error) {
            unserializableValue = Object.prototype.toString.apply($)
          }
        } else if (type === 'number' && (isNaN($) || !isFinite($))) {
          unserializableValue = Object.prototype.toString.apply($)
        } else {
          value = $
        }

        const remoteObject: Protocol.Runtime.RemoteObject = {
          type: typeof $,
          value,
          unserializableValue
        }
        return remoteObject
      })
    }

    this.api.event('Runtime.consoleAPICalled', [params]).catch(this.catchHandler)
  }

  error(e: Error) {
    const exceptionId = this.exceptions.push(e) - 1

    let value: string | void = undefined
    let unserializableValue = undefined

    try {
      value = JSON.stringify(e)
      if (value === "{}" && e instanceof Error) {
        // most Error objects serialize to empty objects
        value = JSON.stringify({
          message: e.message,
          name: e.name,
          stack: e.stack
        })
      }
    } catch (error) {
      unserializableValue = e.toString()
    }

    const exception: Protocol.Runtime.RemoteObject = {
      type: typeof e,
      value,
      unserializableValue
    }

    const param: Protocol.Runtime.ExceptionThrownEvent = {
      timestamp: this.now,
      exceptionDetails: {
        text: e.toString() + '\n' + e.stack,
        exceptionId,
        columnNumber: 0,
        lineNumber: 0,
        exception
      }
    }

    this.api.event('Runtime.exceptionThrown', [param]).catch(this.catchHandler)
  }

  // onCommand(string, any) to receive commands

  // tslint:disable-next-line:no-console
  private catchHandler = (...args: any[]) => console.log(...args)
}
