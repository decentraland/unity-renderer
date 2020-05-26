/// <reference path="../../node_modules/@types/mocha/index.d.ts" />

import { ScriptingHost } from '../../lib/host'
import * as assert from 'assert'
import { future } from './support/Helpers'
import { Script, MemoryTransport } from '../../lib/client'

it('test/out/1.Echo.withoutWebWorker.spec', async () => {
  const memory = MemoryTransport()

  // CLIENT

  const ScriptingClient = new Script(memory.client)

  const x = async () => {
    const data: object = await ScriptingClient.call('MethodX', ['a worker generated string'])
    await ScriptingClient.call('JumpBack', data)
  }
  x().catch(x => console.error(x))

  // SERVER

  const worker = await ScriptingHost.fromTransport(memory.server)

  const randomNumber = Math.random()
  const aFuture = future()

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
