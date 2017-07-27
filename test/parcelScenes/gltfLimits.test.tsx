import { loadTestParcel, waitToBeLoaded } from '../testHelpers'
import { future } from 'fp-future'
import { expect } from 'chai'
import { ScriptingHostEvents } from 'decentraland-rpc/lib/host'

loadTestParcel('Unmount parcelScenes due limits in gltf', -1, 36, function(root, parcelScene, worker) {
  const didTriggerLimits = future()
  const disableFuture = future()

  it('waits for the system to load', async () => {
    const ret = await parcelScene

    ret.on('limitsExceeded', evt => {
      didTriggerLimits.resolve(evt)
    })
  })

  it('the parcelScene should unload due exceeded limits', async function() {
    this.timeout(5000)
    const parcelSceneEntity = await parcelScene

    const system = await (await worker).system

    expect(system.isEnabled).to.eq(true, 'system should be enabled')

    system.on(ScriptingHostEvents.systemDidUnmount, () => {
      disableFuture.resolve(ScriptingHostEvents.systemDidUnmount)
    })

    await waitToBeLoaded(parcelSceneEntity.context.rootEntity)

    parcelSceneEntity.checkLimits()
  })

  it('should trigger exceeded limits', async function() {
    this.timeout(200)

    await didTriggerLimits
  })

  it('should unmount the system', async () => {
    await disableFuture
  })
})
