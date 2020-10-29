import { ScriptingTransport } from 'decentraland-rpc/lib/common/json-rpc/types'

export function CustomWebWorkerTransport(worker: Worker): ScriptingTransport {
  const api: ScriptingTransport = {
    onConnect(handler) {
      worker.addEventListener('message', () => handler(), { once: true })
    },
    onError(handler) {
      worker.addEventListener('error', (err: ErrorEvent) => {
        if (err.error) {
          handler(err.error)
        } else if (err.message) {
          handler(err as any)
        }
      })
    },
    onMessage(handler) {
      worker.addEventListener('message', (message: MessageEvent) => {
        handler(message.data)
      })
    },
    sendMessage(message) {
      worker.postMessage(message)
    },
    close() {
      if ('terminate' in worker) {
        // tslint:disable-next-line:semicolon
        ;(worker as any).terminate()
      } else if ('close' in worker) {
        // tslint:disable-next-line:semicolon
        ;(worker as any).close()
      }
    }
  }

  return api
}
