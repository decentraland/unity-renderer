import { ScriptingTransport } from '../json-rpc/types'

export interface IWorker {
  terminate?(): void
  close?(): void
  postMessage(message: any): void
  addEventListener(type: 'message' | 'error', listener: Function, options?: any): void
}

export function WebWorkerTransport(worker: IWorker): ScriptingTransport {
  const api: ScriptingTransport = {
    onConnect(handler) {
      worker.addEventListener('message', () => handler(), { once: true })
    },
    onError(handler) {
      worker.addEventListener('error', (err: ErrorEvent) => {
        if (err.error) {
          handler(err.error)
        } else if (err.message) {
          handler(
            Object.assign(new Error(err.message), {
              colno: err.colno,
              error: err.error,
              filename: err.filename,
              lineno: err.lineno,
              message: err.message
            })
          )
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
