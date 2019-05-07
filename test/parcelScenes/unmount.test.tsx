import { expect } from 'chai'

import { future } from 'fp-future'
import { ScriptingHostEvents } from 'decentraland-rpc/lib/host'
import { loadTestParcel, waitToBeLoaded } from '../testHelpers'

loadTestParcel('Unmount parcelScenes 1', 200, 10, function(_root, futureScene, futureWorker) {
  const disableFuture = future()

  it.skip('waits for the system to load', async () => {
    const worker = await futureWorker
    const parcelScene = await futureScene

    const system = await worker.system

    system.on(ScriptingHostEvents.systemDidUnmount, () => {
      disableFuture.resolve(ScriptingHostEvents.systemDidUnmount)
    })

    await waitToBeLoaded(parcelScene.context.rootEntity)

    expect(system.isEnabled).to.eq(true, 'system should be enabled')
  })

  it.skip('unrefs the parcelScene', async () => {
    const worker = await futureWorker
    worker.dispose()
    return disableFuture
  })
  true
})
