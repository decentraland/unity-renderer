import * as assert from 'assert'
import { shouldFail, TestableScript } from './support/ClientHelpers'
import { Methods } from './support/ClientCommons'
import { inject, getInjectedAPIs } from '../../lib/client/index'

export class BaseTestMethods extends TestableScript {
  @inject('Methods') m: Methods | null = null

  async doTest() {
    throw new Error('This should be overwritten and never called')
  }
}

export default class TestMethods extends BaseTestMethods {
  @inject() Logger: any = null
  @inject('Test') testComponent: any = null
  @inject('Test') xxx: any = null

  async doTest() {
    assert.deepEqual(Array.from(getInjectedAPIs(this)), [
      ['Logger', 'Logger'],
      ['testComponent', 'Test'],
      ['xxx', 'Test'],
      ['m', 'Methods']
    ])

    if (!this.m) {
      throw new Error('Methods was not loaded')
    }

    if (!this.Logger) {
      throw new Error('Logger was not loaded')
    }

    if (!this.testComponent) {
      throw new Error('Test was not loaded')
    }

    if (!this.xxx) {
      throw new Error('xxx was not loaded')
    }

    const Methods = this.m

    assert.equal(this.xxx, this.testComponent)

    assert.equal(await Methods.enable(), 1)
    assert.equal(typeof (await Methods.getRandomNumber()), 'number')
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
