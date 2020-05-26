/// <reference path="../../node_modules/@types/mocha/index.d.ts" />

import { ScriptingHost } from '../../lib/host'
import * as assert from 'assert'
import { future } from './support/Helpers'
import { Test } from './support/Commons'
import { WebWorkerTransport } from '../../lib/client'

it('test/out/fixtures/2.Proxy.js', async () => {
  const worker = await ScriptingHost.fromTransport(WebWorkerTransport(new Worker('test/out/fixtures/2.Proxy.js')))

  let aFuture = future()

  worker.setLogging({ logConsole: true })

  const enable = async () => void 0

  // Fool the worker to make it believe it has these plugins loaded
  worker.apiInstances.set('xDebugger', {} as any)
  worker.apiInstances.set('xProfiler', {} as any)
  worker.apiInstances.set('xRuntime', {} as any)

  worker.expose('xDebugger.enable', enable)

  worker.expose('xProfiler.enable', enable)
  worker.expose('xProfiler.start', async () => {
    setTimeout(() => {
      worker.notify('xRuntime.ExecutionContextDestroyed')
    }, 16)
  })
  worker.expose('xProfiler.stop', async () => {
    aFuture.resolve(333)
    return { data: 'noice!' }
  })

  worker.expose('xRuntime.enable', enable)
  worker.expose('xRuntime.run', enable)

  worker.enable()

  assert.equal(await aFuture, 333, 'Did stop should have been called.')

  const TestPlugin = worker.getAPIInstance(Test)

  if (!TestPlugin) throw new Error('Cannot retieve Test plugin instance')

  await TestPlugin.waitForPass()

  worker.unmount()
})
