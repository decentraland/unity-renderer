import { Transport, TransportEvents } from '@dcl/rpc'
import mitt from 'mitt'

export type WebTransportOptions = {
  wasmModule: any
}

export function webTransport(options: WebTransportOptions, unityDclInstance: any) {
  const events = mitt<TransportEvents>()
  const ALLOC_SIZE = 8388608
  let heapPtr: number
  let sendMessageToRenderer: undefined | ((ptr: number, length: number) => void) = undefined

  if (!!options.wasmModule._call_BinaryMessage) {
    heapPtr = options.wasmModule._malloc(ALLOC_SIZE)
    sendMessageToRenderer = options.wasmModule.cwrap('call_BinaryMessage', null, ['number', 'number'])
  }

  let isClosed = false
  let didConnect = false

  unityDclInstance.BinaryMessageFromEngine = function (data: Uint8Array) {
    if (!didConnect) {
      throw new Error('Received data from unity before connection was established')
    }
    const copiedData = new Uint8Array(data)
    events.emit('message', copiedData)
  }

  const transport: Transport = {
    ...events,
    get isConnected() {
      return didConnect
    },
    sendMessage(message) {
      if (!didConnect) {
        throw new Error('Tried to send a message before connection was established')
      }
      if (isClosed) {
        throw new Error('Trying to send a message to a closed binary transport')
      }
      queueMicrotask(() => {
        if (!!sendMessageToRenderer && !isClosed) {
          options.wasmModule.HEAPU8.set(message, heapPtr)
          sendMessageToRenderer(heapPtr, message.length)
        }
      })
    },
    close() {
      if (!isClosed) {
        isClosed = true
        events.emit('close', {})
      }
    }
  }

  events.on('connect', () => {
    didConnect = true
  })

  // connect the transport
  events.emit('connect', {})

  return transport
}
