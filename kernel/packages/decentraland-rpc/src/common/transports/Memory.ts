import { ScriptingTransport } from '../json-rpc/types'
import { EventDispatcher } from '../core/EventDispatcher'

export function MemoryTransport() {
  const onConnectList: Function[] = []
  let connected = false

  const clientEd = new EventDispatcher()
  const serverEd = new EventDispatcher()

  function configureMemoryTransport(receiver: EventDispatcher, sender: EventDispatcher): ScriptingTransport {
    return {
      sendMessage(message) {
        sender.emit('message', message)
      },

      close() {
        sender.emit('close')
      },

      onMessage(handler) {
        receiver.on('message', handler)
      },

      onClose(handler) {
        receiver.on('close', handler)
      },

      onError(handler) {
        receiver.on('error', handler)
      },

      onConnect(handler) {
        if (connected == false) {
          onConnectList.push(handler)
        }
      }
    }
  }

  const client = configureMemoryTransport(clientEd, serverEd)
  const server = configureMemoryTransport(serverEd, clientEd)

  // we send a RPC.Enable message when the server gets connected as start signal
  clientEd.on('message', () => {
    if (connected === false) {
      onConnectList.forEach($ => $())
      onConnectList.length = 0
      connected = true
    }
  })

  return {
    client,
    server
  }
}
