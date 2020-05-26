/**
 * Interface according to spec from http://www.jsonrpc.org/specification
 * JSON-RPC is a stateless, light-weight remote procedure call (RPC) protocol.
 * Primarily this specification defines several data structures.
 * It is transport agnostic in that the concepts can be used within the same process,
 * over sockets, over http, or in many various message passing environments. I
 * It uses JSON (RFC 4627) as data format.
 */

/**
 * Client can send a request with no expectation of a response.
 * Server can send a notification without an explicit request by a client.
 */
export type INotification = {
  /** Name of the method to be invoked. */
  method: string

  /** Parameter values to be used during the invocation of the method. */
  params?: any

  /** Version of the JSON-RPC protocol. MUST be exactly "2.0". */
  jsonrpc: '2.0'
}

/**
 * Request object representation of a rpc call.
 * Server always replies with a Response object having the same id.
 */
export type IRequest = INotification & {
  /** An identifier established by the Client */
  id: number
}

/**
 * Response object representation of a rpc call.
 * Response will always contain a result property unless an error occured.
 * In which case, an error property is present.
 */
export type IResponse = {
  /** An identifier established by the Client. */
  id: number

  /** Result object from the Server if method invocation was successful. */
  result?: any

  /** Error object from Server if method invocation resulted in an error. */
  error?: IError

  /** Version of the JSON-RPC protocol. MUST be exactly "2.0". */
  jsonrpc: '2.0'
}

/**
 * Error object representation when a method invocation fails.
 */
export type IError = {
  /** Indicates the error type that occurred. */
  code: ErrorCode

  /** A short description of the error. */
  message: string

  /** Additional information about the error */
  data?: any
} /*
  /** Error codes are same as xml-rpc codes. See http://xmlrpc-epi.sourceforge.net/specs/rfc.fault_codes.php */
export const enum ErrorCode {
  /** Parse error Invalid JSON was received by the Server. */
  ParseError = -32700,

  /** Invalid Request The JSON sent is not a valid Request object. */
  InvalidRequest = -32600,

  /** The method does not exist / is not available. */
  MethodNotFound = -32601,

  /** Invalid method parameter(s). */
  InvalidParams = -(-32602),

  /** Internal JSON-RPC error. */
  InternalError = -32603

  /** -32000 to -32099: Reserved for implementation-defined Server errors.  */
}

export type PromiseOrNot<T> = Promise<T> | T

export type Resolvable<T = any> = {
  resolve(arg: T): void
  reject(arg: Error): void
}

export type ILogOpts = {
  /** All messages will be logged to console */
  logConsole?: boolean
}

/**
 * A JsonRPC Client that abstracts the transportation of messages to and from the Server.
 */
export type IClient = {
  /** Creates a Request object and sends to the Server. Returns the Response from the Server as a Promise. */
  call(method: string, params: any): Promise<any>

  /** Invokes the handler function when Server sends a notification. */
  on(method: string, handler: (params: any) => void): void

  /** Sends a notification to the Server. */
  notify(method: string, params?: any): void
}

export type IClientOpts = ILogOpts

/** A JsonRPC Server that abstracts the transportation of messages to and from the Client */
export type IServer = {
  /**
   * Invokes the handler function when Client sends a Request and sends the Response back.
   * If handler function returns a Promise, then it waits for the promise to be resolved or rejected before returning.
   * It also wraps the handler in a trycatch so it can send an error response when an exception is thrown.
   */
  expose(method: string, handler: (params: any) => Promise<any>): void

  /** Invokes the handler function when Client sends a notification. */
  on(method: string, handler: (params: any) => void): void

  /** Sends a notification to the Client. */
  notify(method: string, params?: any): void
}

export type IServerOpts = ILogOpts

export interface ScriptingTransport {
  /** the allowBinary property tells if the transport allows binary serialization */
  allowBinary?: boolean

  /** sendMessage is used to send a string message thru the transport */
  sendMessage(message: string): void

  /** the onConnect callback is called when the transport gets connected */
  onConnect?(callback: () => void): void

  /** the onMessage callback is called when the transport receives a message */
  onMessage(callback: (message: string) => void): void

  /** the onError callback is called when the transport triggers an error */
  onError?(callback: (e: Error) => void): void

  /** the onClose callback is called when the transport gets disconnected */
  onClose?(callback: () => void): void

  /** the close function will be called when it is decided to end the communication */
  close(): void
}
