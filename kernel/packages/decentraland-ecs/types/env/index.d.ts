/// <reference path="./es5.d.ts" />

interface PromiseConstructor {
  /**
   * A reference to the prototype.
   */
  readonly prototype: Promise<any>

  /**
   * Creates a new Promise.
   * @param executor A callback used to initialize the promise. This callback is passed two arguments:
   * a resolve callback used resolve the promise with a value or the result of another promise,
   * and a reject callback used to reject the promise with a provided reason or error.
   */
  new <T>(executor: (resolve: (value?: T | PromiseLike<T>) => void, reject: (reason?: any) => void) => void): Promise<T>

  /**
   * Creates a Promise that is resolved with an array of results when all of the provided Promises
   * resolve, or rejected when any Promise is rejected.
   * @param values An array of Promises.
   * @returns A new Promise.
   */
  all<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
    values: [
      T1 | PromiseLike<T1>,
      T2 | PromiseLike<T2>,
      T3 | PromiseLike<T3>,
      T4 | PromiseLike<T4>,
      T5 | PromiseLike<T5>,
      T6 | PromiseLike<T6>,
      T7 | PromiseLike<T7>,
      T8 | PromiseLike<T8>,
      T9 | PromiseLike<T9>,
      T10 | PromiseLike<T10>
    ]
  ): Promise<[T1, T2, T3, T4, T5, T6, T7, T8, T9, T10]>

  /**
   * Creates a Promise that is resolved with an array of results when all of the provided Promises
   * resolve, or rejected when any Promise is rejected.
   * @param values An array of Promises.
   * @returns A new Promise.
   */
  all<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
    values: [
      T1 | PromiseLike<T1>,
      T2 | PromiseLike<T2>,
      T3 | PromiseLike<T3>,
      T4 | PromiseLike<T4>,
      T5 | PromiseLike<T5>,
      T6 | PromiseLike<T6>,
      T7 | PromiseLike<T7>,
      T8 | PromiseLike<T8>,
      T9 | PromiseLike<T9>
    ]
  ): Promise<[T1, T2, T3, T4, T5, T6, T7, T8, T9]>

  /**
   * Creates a Promise that is resolved with an array of results when all of the provided Promises
   * resolve, or rejected when any Promise is rejected.
   * @param values An array of Promises.
   * @returns A new Promise.
   */
  all<T1, T2, T3, T4, T5, T6, T7, T8>(
    values: [
      T1 | PromiseLike<T1>,
      T2 | PromiseLike<T2>,
      T3 | PromiseLike<T3>,
      T4 | PromiseLike<T4>,
      T5 | PromiseLike<T5>,
      T6 | PromiseLike<T6>,
      T7 | PromiseLike<T7>,
      T8 | PromiseLike<T8>
    ]
  ): Promise<[T1, T2, T3, T4, T5, T6, T7, T8]>

  /**
   * Creates a Promise that is resolved with an array of results when all of the provided Promises
   * resolve, or rejected when any Promise is rejected.
   * @param values An array of Promises.
   * @returns A new Promise.
   */
  all<T1, T2, T3, T4, T5, T6, T7>(
    values: [
      T1 | PromiseLike<T1>,
      T2 | PromiseLike<T2>,
      T3 | PromiseLike<T3>,
      T4 | PromiseLike<T4>,
      T5 | PromiseLike<T5>,
      T6 | PromiseLike<T6>,
      T7 | PromiseLike<T7>
    ]
  ): Promise<[T1, T2, T3, T4, T5, T6, T7]>

  /**
   * Creates a Promise that is resolved with an array of results when all of the provided Promises
   * resolve, or rejected when any Promise is rejected.
   * @param values An array of Promises.
   * @returns A new Promise.
   */
  all<T1, T2, T3, T4, T5, T6>(
    values: [
      T1 | PromiseLike<T1>,
      T2 | PromiseLike<T2>,
      T3 | PromiseLike<T3>,
      T4 | PromiseLike<T4>,
      T5 | PromiseLike<T5>,
      T6 | PromiseLike<T6>
    ]
  ): Promise<[T1, T2, T3, T4, T5, T6]>

  /**
   * Creates a Promise that is resolved with an array of results when all of the provided Promises
   * resolve, or rejected when any Promise is rejected.
   * @param values An array of Promises.
   * @returns A new Promise.
   */
  all<T1, T2, T3, T4, T5>(
    values: [
      T1 | PromiseLike<T1>,
      T2 | PromiseLike<T2>,
      T3 | PromiseLike<T3>,
      T4 | PromiseLike<T4>,
      T5 | PromiseLike<T5>
    ]
  ): Promise<[T1, T2, T3, T4, T5]>

  /**
   * Creates a Promise that is resolved with an array of results when all of the provided Promises
   * resolve, or rejected when any Promise is rejected.
   * @param values An array of Promises.
   * @returns A new Promise.
   */
  all<T1, T2, T3, T4>(
    values: [T1 | PromiseLike<T1>, T2 | PromiseLike<T2>, T3 | PromiseLike<T3>, T4 | PromiseLike<T4>]
  ): Promise<[T1, T2, T3, T4]>

  /**
   * Creates a Promise that is resolved with an array of results when all of the provided Promises
   * resolve, or rejected when any Promise is rejected.
   * @param values An array of Promises.
   * @returns A new Promise.
   */
  all<T1, T2, T3>(values: [T1 | PromiseLike<T1>, T2 | PromiseLike<T2>, T3 | PromiseLike<T3>]): Promise<[T1, T2, T3]>

  /**
   * Creates a Promise that is resolved with an array of results when all of the provided Promises
   * resolve, or rejected when any Promise is rejected.
   * @param values An array of Promises.
   * @returns A new Promise.
   */
  all<T1, T2>(values: [T1 | PromiseLike<T1>, T2 | PromiseLike<T2>]): Promise<[T1, T2]>

  /**
   * Creates a Promise that is resolved with an array of results when all of the provided Promises
   * resolve, or rejected when any Promise is rejected.
   * @param values An array of Promises.
   * @returns A new Promise.
   */
  all<T>(values: (T | PromiseLike<T>)[]): Promise<T[]>

  /**
   * Creates a Promise that is resolved or rejected when any of the provided Promises are resolved
   * or rejected.
   * @param values An array of Promises.
   * @returns A new Promise.
   */
  race<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
    values: [
      T1 | PromiseLike<T1>,
      T2 | PromiseLike<T2>,
      T3 | PromiseLike<T3>,
      T4 | PromiseLike<T4>,
      T5 | PromiseLike<T5>,
      T6 | PromiseLike<T6>,
      T7 | PromiseLike<T7>,
      T8 | PromiseLike<T8>,
      T9 | PromiseLike<T9>,
      T10 | PromiseLike<T10>
    ]
  ): Promise<T1 | T2 | T3 | T4 | T5 | T6 | T7 | T8 | T9 | T10>

  /**
   * Creates a Promise that is resolved or rejected when any of the provided Promises are resolved
   * or rejected.
   * @param values An array of Promises.
   * @returns A new Promise.
   */
  race<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
    values: [
      T1 | PromiseLike<T1>,
      T2 | PromiseLike<T2>,
      T3 | PromiseLike<T3>,
      T4 | PromiseLike<T4>,
      T5 | PromiseLike<T5>,
      T6 | PromiseLike<T6>,
      T7 | PromiseLike<T7>,
      T8 | PromiseLike<T8>,
      T9 | PromiseLike<T9>
    ]
  ): Promise<T1 | T2 | T3 | T4 | T5 | T6 | T7 | T8 | T9>

  /**
   * Creates a Promise that is resolved or rejected when any of the provided Promises are resolved
   * or rejected.
   * @param values An array of Promises.
   * @returns A new Promise.
   */
  race<T1, T2, T3, T4, T5, T6, T7, T8>(
    values: [
      T1 | PromiseLike<T1>,
      T2 | PromiseLike<T2>,
      T3 | PromiseLike<T3>,
      T4 | PromiseLike<T4>,
      T5 | PromiseLike<T5>,
      T6 | PromiseLike<T6>,
      T7 | PromiseLike<T7>,
      T8 | PromiseLike<T8>
    ]
  ): Promise<T1 | T2 | T3 | T4 | T5 | T6 | T7 | T8>

  /**
   * Creates a Promise that is resolved or rejected when any of the provided Promises are resolved
   * or rejected.
   * @param values An array of Promises.
   * @returns A new Promise.
   */
  race<T1, T2, T3, T4, T5, T6, T7>(
    values: [
      T1 | PromiseLike<T1>,
      T2 | PromiseLike<T2>,
      T3 | PromiseLike<T3>,
      T4 | PromiseLike<T4>,
      T5 | PromiseLike<T5>,
      T6 | PromiseLike<T6>,
      T7 | PromiseLike<T7>
    ]
  ): Promise<T1 | T2 | T3 | T4 | T5 | T6 | T7>

  /**
   * Creates a Promise that is resolved or rejected when any of the provided Promises are resolved
   * or rejected.
   * @param values An array of Promises.
   * @returns A new Promise.
   */
  race<T1, T2, T3, T4, T5, T6>(
    values: [
      T1 | PromiseLike<T1>,
      T2 | PromiseLike<T2>,
      T3 | PromiseLike<T3>,
      T4 | PromiseLike<T4>,
      T5 | PromiseLike<T5>,
      T6 | PromiseLike<T6>
    ]
  ): Promise<T1 | T2 | T3 | T4 | T5 | T6>

  /**
   * Creates a Promise that is resolved or rejected when any of the provided Promises are resolved
   * or rejected.
   * @param values An array of Promises.
   * @returns A new Promise.
   */
  race<T1, T2, T3, T4, T5>(
    values: [
      T1 | PromiseLike<T1>,
      T2 | PromiseLike<T2>,
      T3 | PromiseLike<T3>,
      T4 | PromiseLike<T4>,
      T5 | PromiseLike<T5>
    ]
  ): Promise<T1 | T2 | T3 | T4 | T5>

  /**
   * Creates a Promise that is resolved or rejected when any of the provided Promises are resolved
   * or rejected.
   * @param values An array of Promises.
   * @returns A new Promise.
   */
  race<T1, T2, T3, T4>(
    values: [T1 | PromiseLike<T1>, T2 | PromiseLike<T2>, T3 | PromiseLike<T3>, T4 | PromiseLike<T4>]
  ): Promise<T1 | T2 | T3 | T4>

  /**
   * Creates a Promise that is resolved or rejected when any of the provided Promises are resolved
   * or rejected.
   * @param values An array of Promises.
   * @returns A new Promise.
   */
  race<T1, T2, T3>(values: [T1 | PromiseLike<T1>, T2 | PromiseLike<T2>, T3 | PromiseLike<T3>]): Promise<T1 | T2 | T3>

  /**
   * Creates a Promise that is resolved or rejected when any of the provided Promises are resolved
   * or rejected.
   * @param values An array of Promises.
   * @returns A new Promise.
   */
  race<T1, T2>(values: [T1 | PromiseLike<T1>, T2 | PromiseLike<T2>]): Promise<T1 | T2>

  /**
   * Creates a Promise that is resolved or rejected when any of the provided Promises are resolved
   * or rejected.
   * @param values An array of Promises.
   * @returns A new Promise.
   */
  race<T>(values: (T | PromiseLike<T>)[]): Promise<T>

  /**
   * Creates a new rejected promise for the provided reason.
   * @param reason The reason the promise was rejected.
   * @returns A new rejected Promise.
   */
  reject<T = never>(reason?: any): Promise<T>

  /**
   * Creates a new resolved promise for the provided value.
   * @param value A promise.
   * @returns A promise whose internal state matches the provided promise.
   */
  resolve<T>(value: T | PromiseLike<T>): Promise<T>

  /**
   * Creates a new resolved promise .
   * @returns A resolved promise.
   */
  resolve(): Promise<void>
}

declare var Promise: PromiseConstructor

/// --- FETCH ---

type RequestRedirect = 'follow' | 'error' | 'manual'
type ResponseType = 'basic' | 'cors' | 'default' | 'error' | 'opaque' | 'opaqueredirect'

interface RequestInit {
  // whatwg/fetch standard options
  body?: string
  headers?: { [index: string]: string }
  method?: string
  redirect?: RequestRedirect
}

interface ReadOnlyHeaders {
  get(name: string): string | null
  has(name: string): boolean
  forEach(callbackfn: (value: string, key: string, parent: ReadOnlyHeaders) => void, thisArg?: any): void
}

interface Response {
  readonly headers: ReadOnlyHeaders
  readonly ok: boolean
  readonly redirected: boolean
  readonly status: number
  readonly statusText: string
  readonly type: ResponseType
  readonly url: string

  json(): Promise<any>
  text(): Promise<string>
}

declare function fetch(url: string, init?: RequestInit): Promise<Response>

/// --- WebSocket ---

interface Event {
  readonly type: string
}

interface MessageEvent extends Event {
  /**
   * Returns the data of the message.
   */
  readonly data: any
}

interface CloseEvent extends Event {
  readonly code: number
  readonly reason: string
  readonly wasClean: boolean
}

interface WebSocket {
  readonly bufferedAmount: number
  readonly extensions: string
  onclose: ((this: WebSocket, ev: CloseEvent) => any) | null
  onerror: ((this: WebSocket, ev: Event) => any) | null
  onmessage: ((this: WebSocket, ev: MessageEvent) => any) | null
  onopen: ((this: WebSocket, ev: Event) => any) | null
  readonly protocol: string
  readonly readyState: number
  readonly url: string
  close(code?: number, reason?: string): void
  send(data: string): void
  readonly CLOSED: number
  readonly CLOSING: number
  readonly CONNECTING: number
  readonly OPEN: number
}

declare var WebSocket: {
  prototype: WebSocket
  new (url: string, protocols?: string | string[]): WebSocket
  readonly CLOSED: number
  readonly CLOSING: number
  readonly CONNECTING: number
  readonly OPEN: number
}
