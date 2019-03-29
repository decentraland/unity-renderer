export type ILogger = {
  error(message: string, ...args: any[]): void
  log(message: string, ...args: any[]): void
  warn(message: string, ...args: any[]): void
  info(message: string, ...args: any[]): void
  trace(message: string, ...args: any[]): void
}

export function createLogger(prefix: string): ILogger {
  return {
    error(message: string | Error, ...args: any[]): void {
      if (typeof message === 'object' && message.stack) {
        // tslint:disable-next-line:no-console
        console.error(prefix + message, ...args, message.stack)
      } else {
        // tslint:disable-next-line:no-console
        console.error(prefix + message, ...args)
      }
    },
    log(message: string, ...args: any[]): void {
      // tslint:disable-next-line:no-console
      console.log(prefix + message, ...args)
    },
    warn(message: string, ...args: any[]): void {
      // tslint:disable-next-line:no-console
      console.log(prefix + message, ...args)
    },
    info(message: string, ...args: any[]): void {
      // tslint:disable-next-line:no-console
      console.info(prefix + message, ...args)
    },
    trace(message: string, ...args: any[]): void {
      // tslint:disable-next-line:no-console
      console.trace(prefix + message, ...args)
    }
  }
}

export const defaultLogger: ILogger = createLogger('')
