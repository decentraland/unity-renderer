import { enableVisualTests } from '../testHelpers'
import { expect } from 'chai'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { resolveUrl } from 'atomicHelpers/parseUrl'

enableVisualTests('it runs in a web browser context', function() {
  it('should contain window', () => {
    expect(window).to.be.instanceof(Window)
  })

  // test imports work
  it('BaseEntity should exist', () => {
    expect(!!BaseEntity).to.equal(true)
  })

  it('resolveUrl keep untouched content urls', () => {
    const blob = new Blob(['asd'])
    const url = URL.createObjectURL(blob)

    expect(resolveUrl('http://google.com', url.toString())).to.equal(url.toString())
  })
})
