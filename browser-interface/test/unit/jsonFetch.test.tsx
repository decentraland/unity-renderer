import { expect } from 'chai'
import sinon from 'sinon'
import { jsonFetch } from 'lib/javascript/jsonFetch'

describe('jsonFetch', function() {
  let originalFetch: any
  let fake: any

  beforeEach(function() {
    originalFetch = globalThis.fetch
    fake = sinon.stub()
    globalThis.fetch = fake
  })

  afterEach(function() {
    fake = undefined
    globalThis.fetch = originalFetch
  })

  it('does not cache a result if it fails', async () => {
    fake.onCall(0).returns(Promise.resolve({ ok: false }))
    fake.returns(Promise.resolve({ ok: true, json: () => ({ stub: true }) }))

    try {
      await jsonFetch('fakeurl')
      expect.fail('first jsonFetch should reject')
    } catch (e) {
      // nothing to do here
    }

    await jsonFetch('fakeurl')

    sinon.assert.calledTwice(fake)
  })

  it('caches a result if it succeeds', async () => {
    fake.returns(Promise.resolve({ ok: true, json: () => ({ stub: true }) }))

    const r1 = await jsonFetch('anotherfakeurl')
    const r2 = await jsonFetch('anotherfakeurl')

    sinon.assert.calledOnce(fake)
    expect(r1).to.eq(r2)
  })
})
