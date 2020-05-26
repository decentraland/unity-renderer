import * as assert from 'assert'
import { shouldFail, TestableScript } from './support/ClientHelpers'
import { Methods } from './support/ClientCommons'
import { inject } from '../../lib/client/index'

export default class TestMethods extends TestableScript {
  @inject() Methods: Methods | null = null

  async doTest() {
    if (!this.Methods) {
      throw new Error('Methods was not loaded')
    }

    const Methods = this.Methods

    assert.equal(await Methods.enable(), 1)
    assert.equal(typeof await Methods.getRandomNumber(), 'number')
    assert((await Methods.getRandomNumber()) > 0)

    const sentObject = {
      x: await Methods.getRandomNumber()
    }

    assert.deepEqual(await Methods.receiveObject(sentObject), {
      received: sentObject
    })

    await Methods.failsWithoutParams(1)
    await Methods.failsWithParams()

    await shouldFail(() => Methods.failsWithoutParams(), 'failsWithoutParams')
    await shouldFail(() => Methods.failsWithParams(1), 'failsWithParams')

    const sentElements = [1, true, null, false, 'xxx', { a: null }]

    assert.deepEqual(await Methods.bounce(...sentElements), sentElements)

    console.log('If you see this console.log, it did work')
  }
}
