export type ILogger = {
  error(message: string | Error, ...args: any[]): void
  log(message: string, ...args: any[]): void
  warn(message: string, ...args: any[]): void
  info(message: string, ...args: any[]): void
  trace(message: string, ...args: any[]): void
}

export function createDummyLogger(): ILogger {
  return {
    error(_message: string | Error, ..._args: any[]): void {
      /*nothing*/
    },
    log(_message: string, ..._args: any[]): void {
      /*nothing*/
    },
    warn(_message: string, ..._args: any[]): void {
      /*nothing*/
    },
    info(_message: string, ..._args: any[]): void {
      /*nothing*/
    },
    trace(_message: string, ..._args: any[]): void {
      /*nothing*/
    }
  }
}
function createDefaultLogger(type: 'kernel' | 'unity', prefix: string, subPrefix: string = ''): ILogger {
  const kernelPrefix = `${type}:${prefix} `
  return {
    error(message: string | Error, ...args: any[]): void {
      if (typeof message === 'object' && message.stack) {
        console.error(kernelPrefix, subPrefix, message, ...args, message.stack)
      } else {
        console.error(kernelPrefix, subPrefix, message, ...args)
      }
    },
    log(message: string, ...args: any[]): void {
      if (args && args[0] && args[0].startsWith && args[0].startsWith('The entity is already in the engine.')) {
        return
      }
      console.log(kernelPrefix, subPrefix, message, ...args)
    },
    warn(message: string, ...args: any[]): void {
      console.log(kernelPrefix, subPrefix, message, ...args)
    },
    info(message: string, ...args: any[]): void {
      console.info(kernelPrefix, subPrefix, message, ...args)
    },
    trace(message: string, ...args: any[]): void {
      console.trace(kernelPrefix, subPrefix, message, ...args)
    }
  }
}

export function createForwardedLogger(type: 'kernel' | 'unity', prefix: string, subPrefix: string = ''): ILogger {
  const targetWindow = globalThis.window.parent || globalThis.window
  const kernelPrefix = `${type}:${prefix} `

  function forwardMessage(type: string, args) {
    targetWindow && targetWindow.postMessage({ type, payload: { args } }, '*')
  }

  return {
    error(message: string | Error, ...args: any[]): void {
      const computedArgs = [kernelPrefix, subPrefix, message, ...args]
      if (typeof message === 'object' && message.stack) {
        computedArgs.push(message.stack)
      }

      console.error.apply(null, computedArgs)
      forwardMessage('logger.error', computedArgs)
    },
    log(message: string, ...args: any[]): void {
      if (args && args[0] && args[0].startsWith && args[0].startsWith('The entity is already in the engine.')) {
        return
      }

      const computedArgs = [kernelPrefix, subPrefix, message, ...args]
      console.log.apply(null, computedArgs)
      forwardMessage('logger.log', computedArgs)
    },
    warn(message: string, ...args: any[]): void {
      const computedArgs = [kernelPrefix, subPrefix, message, ...args]
      console.log.apply(null, computedArgs)
      forwardMessage('logger.warn', computedArgs)
    },
    info(message: string, ...args: any[]): void {
      const computedArgs = [kernelPrefix, subPrefix, message, ...args]
      console.info.apply(null, computedArgs)
      forwardMessage('logger.info', computedArgs)
    },
    trace(message: string, ...args: any[]): void {
      const computedArgs = [kernelPrefix, subPrefix, message, ...args]
      console.trace.apply(null, computedArgs)
      forwardMessage('logger.trace', computedArgs)
    }
  }
}

export function createLogger(prefix: string, subPrefix: string = ''): ILogger {
  return createDefaultLogger('kernel', prefix, subPrefix)
}

export function createUnityLogger(): ILogger {
  return createDefaultLogger('unity', '')
}

export const defaultLogger: ILogger = createLogger('')

export default defaultLogger

/**
 * Extracted from @well-known-components
 * @returns A generic log component user the default logger
 */
export function createGenericLogComponent() {
  return {
    getLogger(loggerName: string): ILogger {
      return {
        log(message: string, extra?: Record<string, string | number>) {
          defaultLogger.log(loggerName, message, extra)
        },
        warn(message: string, extra?: Record<string, string | number>) {
          defaultLogger.warn(loggerName, message, extra)
        },
        info(message: string, extra?: Record<string, string | number>) {
          defaultLogger.info(loggerName, message, extra)
        },
        trace(message: string, extra?: Record<string, string | number>) {
          defaultLogger.trace(loggerName, message, extra)
        },
        error(error: string | Error, extra?: Record<string, string | number>) {
          let message = `${error}`
          let printTrace = true
          if (error instanceof Error && 'stack' in error && typeof error.stack === 'string') {
            if (error.stack.includes(error.message)) {
              message = error.stack
              printTrace = false
            }
          }
          defaultLogger.error(loggerName, message, extra || error)
          if (printTrace) {
            console.trace()
          }
        }
      }
    }
  }
}
