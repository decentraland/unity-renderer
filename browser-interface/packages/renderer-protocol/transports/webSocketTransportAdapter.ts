import { Transport, TransportEvents } from '@dcl/rpc'
import mitt from 'mitt'
import { CommonRendererOptions } from 'unity-interface/loader'

export const defer = Promise.prototype.then.bind(Promise.resolve())
/** @deprecated
 transport to make compatibility binary and string messages swap
 TODO: Remove on ECS6 Legacy code removal
*/
export function webSocketTransportAdapter(url: string, options: CommonRendererOptions): Transport {
  const events = mitt<TransportEvents>()
  let socket: WebSocket | undefined = undefined
  let firstConnect = true

  const send = function (message: any) {
    if (!socket) return

    if (socket.readyState === socket.OPEN) {
      if (message instanceof Uint8Array || message instanceof ArrayBuffer) {
        socket.send(message)
      } else {
        const msg = JSON.stringify({ type: message.type, payload: message.payload })
        socket.send(msg)
      }
    }
  }

  const connect = function () {
    socket = new WebSocket(url)

    const queue: any[] = []

    ;(socket as any).binaryType = 'arraybuffer'

    socket.addEventListener('open', function () {
      firstConnect = false
      flush()
    })

    function flush() {
      if (socket!.readyState === socket!.OPEN) {
        for (const $ of queue) {
          send($)
        }
        queue.length = 0
      }
    }

    socket.addEventListener('close', () => events.emit('close', {}), { once: true })

    if (socket.readyState === socket.OPEN) {
      defer(() => events.emit('connect', { socket }))
    } else {
      socket.addEventListener('open', () => events.emit('connect', { socket }), { once: true })
    }

    socket.addEventListener('error', (err: any) => {
      if (firstConnect === true) {
        setTimeout(() => {
          connect()
        }, 1000)
      } else {
        if (err.error) {
          events.emit('error', err.error)
        } else if (err.message) {
          events.emit(
            'error',
            Object.assign(new Error(err.message), {
              colno: err.colno,
              error: err.error,
              filename: err.filename,
              lineno: err.lineno,
              message: err.message
            })
          )
        }
      }
    })
    socket.addEventListener('message', (message: { data: any }) => {
      if (message.data instanceof ArrayBuffer) {
        events.emit('message', new Uint8Array(message.data))
      } else {
        const m = JSON.parse(message.data)
        if (m.type && m.payload) {
          options.onMessage(m.type, m.payload)
        }
      }
    })
  }

  const transport: Transport = {
    ...events,
    get isConnected(): boolean {
      return (socket && socket.readyState === socket.OPEN) || false
    },
    sendMessage(message: any) {
      send(message)
    },
    close() {
      if (socket) socket.close()
    }
  }

  connect()

  return transport
}
