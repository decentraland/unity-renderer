import { WebWorkerTransport } from 'decentraland-rpc'
import { SceneRuntime } from './sdk/SceneRuntime'
import type { IWorker } from 'decentraland-rpc/lib/common/transports/WebWorker'

/**
 * This file starts the scene in a Node.js context.
 * The runtime ends up mocking the WebWorker environment
 * (postMessage,addEventListener,fetch, WebSocket) for fidelity.
 */

export interface VMEnvironment extends IWorker {
  global: VMEnvironment
  __env__onTick(cb: (dt: number) => void): void
  __env__error(error: Error): void
  __env__log(...args: any[]): void
  runCode(filename: string): Promise<void>
}

const globalObject: VMEnvironment = globalThis as any

if (!('global' in globalObject)) {
  ;(globalObject as any).global = globalObject
}

class CliScene extends SceneRuntime {
  onError(error: Error): void {
    globalObject.__env__error(error)
  }

  onLog(...messages: any[]): void {
    globalObject.__env__log(...messages)
  }

  async runCode(location: string, env: any): Promise<void> {
    Object.assign(globalObject, env)
    return globalObject.runCode(location)
  }

  startLoop(): void {
    globalObject.__env__onTick((dt) => this.update(dt))
  }
}

// tslint:disable-next-line
new CliScene(WebWorkerTransport(globalObject))
