import { wait } from './support/Helpers'
import { registerAPI, API, ScriptingHost, APIOptions } from '../../lib/host'
import './support/MessageBusManager'
import { WebWorkerTransport } from '../../lib/client'

/**
 * This test validates that we are able to unmount workers that fail
 * to respond to the ping notification. This may happen if the worker
 * is frozen due to a computationally expensive task. It will finish
 * (and pass) if the worker is successfully terminated. Otherwise the
 * test will timeout as isAlive() will keep returning true and thus the
 * loop will continue.
 */

@registerAPI('Terminate')
export class Terminator extends API {
  private lastPing: number = 0

  constructor(opt: APIOptions) {
    super(opt)
    opt.on('Ping', () => (this.lastPing = +new Date()))
    this.lastPing = +new Date()
  }

  isAlive(): boolean {
    const time = +new Date() - this.lastPing
    return time < 1000
  }
}

describe('Terminate', function() {
  it('should kill the worker', async () => {
    const worker = await ScriptingHost.fromTransport(
      WebWorkerTransport(new Worker('test/out/fixtures/6.GreedyWorker.js'))
    )
    worker.enable()
    const api = worker.getAPIInstance(Terminator)

    while (api.isAlive()) {
      console.log('it is alive')
      await wait(20)
    }

    console.log('ending interval')
    worker.unmount()
  })
})
