/// <reference path="../../node_modules/@types/mocha/index.d.ts" />

import { ScriptingHost } from '../../lib/host'
import * as assert from 'assert'
import { future } from './support/Helpers'
import { WebWorkerTransport } from '../../lib/client'

it('test/out/fixtures/1.Echo.js', async () => {
  const worker = await ScriptingHost.fromTransport(WebWorkerTransport(new Worker('test/out/fixtures/1.Echo.js')))

  const randomNumber = Math.random()
  const aFuture = future()

  // worker.setLogging({ logConsole: true, logEmit: true });

  worker.expose('MethodX', async message => {
    return { number: randomNumber }
  })

  worker.expose('JumpBack', async data => {
    aFuture.resolve(data.number)
  })

  worker.enable()

  assert.equal(await aFuture, randomNumber, 'exchanged numbers must match')

  worker.unmount()
})
