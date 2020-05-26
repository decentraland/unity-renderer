import { TestableScript, wait } from './support/ClientHelpers'
import { inject } from '../../lib/client/index'
import * as assert from 'assert'

export default class Throttling extends TestableScript {
  @inject('Throttling') throttling: any = null

  async doTest() {
    const { throttling } = this

    let failed = false

    // Calls 5 every 10 ms so it's within the 100ms range
    for (let index = 0; index < 5; index++) {
      await wait(10)
      try {
        await throttling.fiveEveryHundredMilliseconds()
      } catch (e) {
        failed = true
      }
    }

    assert.equal(failed, false)

    // Gets Throttled: calls 6 in a 5 calls per 100ms range
    await wait(100)

    for (let index = 0; index < 6; index++) {
      await wait(10)
      try {
        await throttling.fiveEveryHundredMilliseconds()
      } catch (e) {
        failed = true
      }
    }

    assert.equal(failed, true)

    // Call once, wait for the next interval, then get throttled
    failed = false
    await wait(100)
    await throttling.fiveEveryHundredMilliseconds()
    await wait(100)

    for (let index = 0; index < 6; index++) {
      await wait(10)
      try {
        await throttling.fiveEveryHundredMilliseconds()
      } catch (e) {
        failed = true
      }
    }

    assert.equal(failed, true)
  }
}
