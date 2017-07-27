export function warn(...parts: any[]) {
  // tslint:disable-next-line:no-console
  console.warn(...parts)
}

export function log<T>(object: T): void
export function log<T>(message: string, obj: T): void
export function log(...parts: any[]) {
  // tslint:disable-next-line:no-console
  console.log(...parts)
}

export function info<T>(object: T): void
export function info<T>(message: string, obj: T): void
export function info(...parts: any[]) {
  // tslint:disable-next-line:no-console
  console.info(...parts)
}

export function error<T>(object: T): void
export function error(message: string, ...obj: any[]): void
export function error(...parts: any[]) {
  // tslint:disable-next-line:no-console
  console.error(...parts)
}
