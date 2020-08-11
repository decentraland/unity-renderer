// tslint:disable:no-console
export type ILogger = {
  error(message: string, ...args: any[]): void
  log(message: string, ...args: any[]): void
  warn(message: string, ...args: any[]): void
  info(message: string, ...args: any[]): void
  trace(message: string, ...args: any[]): void
}

export function createDummyLogger(): ILogger {
  return {
    error(message: string | Error, ...args: any[]): void {
      /*nothing*/
    },
    log(message: string, ...args: any[]): void {
      /*nothing*/
    },
    warn(message: string, ...args: any[]): void {
      /*nothing*/
    },
    info(message: string, ...args: any[]): void {
      /*nothing*/
    },
    trace(message: string, ...args: any[]): void {
      /*nothing*/
    }
  }
}

export function createLogger(prefix: string): ILogger {
  return {
    error(message: string | Error, ...args: any[]): void {
      if (typeof message === 'object' && message.stack) {
        console.error(prefix + message, ...args, message.stack)
      } else {
        console.error(prefix + message, ...args)
      }
    },
    log(message: string, ...args: any[]): void {
      if (args && args[0] && args[0].startsWith && args[0].startsWith('The entity is already in the engine.')) {
        return
      }
      console.log(prefix + message, ...args)
    },
    warn(message: string, ...args: any[]): void {
      console.log(prefix + message, ...args)
    },
    info(message: string, ...args: any[]): void {
      console.info(prefix + message, ...args)
    },
    trace(message: string, ...args: any[]): void {
      console.trace(prefix + message, ...args)
    }
  }
}

export const defaultLogger: ILogger = createLogger('')

export default defaultLogger
