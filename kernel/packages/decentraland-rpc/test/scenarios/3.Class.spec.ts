import { registerAPI, API, exposeMethod } from '../../lib/host'

import * as assert from 'assert'
import { future, testInWorker } from './support/Helpers'

const aFuture = future()

@registerAPI('Debugger')
export class Debugger extends API {
  @exposeMethod
  async enable() {
    return 1
  }
}

@registerAPI('Profiler')
export class Profiler extends API {
  @exposeMethod
  async enable() {
    return 1
  }

  @exposeMethod
  async start() {
    setTimeout(() => {
      this.options.notify('ExecutionContextDestroyed')
    }, 16)
  }

  @exposeMethod
  async stop() {
    aFuture.resolve(true)
    return { data: 'noice!' }
  }
}

@registerAPI('Runtime')
export class Runtime extends API {
  @exposeMethod
  async enable() {
    return 1
  }

  @exposeMethod
  async run() {
    return 1
  }
}

testInWorker('test/out/fixtures/3.Class.js', {
  plugins: [Debugger, Profiler, Runtime],
  validateResult: async result => {
    assert.equal(await aFuture, true)
  }
})
