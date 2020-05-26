import * as assert from 'assert'
import { test, shouldFail } from './support/ClientHelpers'
import { Methods } from './support/ClientCommons'

test(async ScriptingClient => {
  const { Methods } = (await ScriptingClient.loadAPIs(['Methods'])) as {
    Methods: Methods
  }

  assert.equal(await Methods.enable(), 1)
  assert.equal(typeof await Methods.getRandomNumber(), 'number')
  assert((await Methods.getRandomNumber()) > 0)

  const sentObject = {
    x: await Methods.getRandomNumber()
  }

  assert.equal(await Methods.ret0(), 0)
  assert.equal(await Methods.retFalse(), false)
  assert.equal(await Methods.retNull(), null)
  assert.equal(await Methods.retEmptyStr(), '')
  assert.equal(await Methods.retTrue(), true)

  assert.deepEqual(await Methods.receiveObject(sentObject), {
    received: sentObject
  })

  await Methods.failsWithoutParams(1)
  await Methods.failsWithParams()

  await shouldFail(() => Methods.failsWithoutParams(), 'failsWithoutParams')
  await shouldFail(() => Methods.failsWithParams(1), 'failsWithParams')

  const sentElements = [1, true, null, false, 'xxx', { a: null }]

  assert.deepEqual(await Methods.bounce(...sentElements), sentElements)
  assert.deepEqual(await Methods.singleBounce(sentElements), sentElements)

  for (let $ of sentElements) {
    assert.deepEqual(await Methods.singleBounce($), $)
  }
})
