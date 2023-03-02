import { expect } from 'chai'
import { defaultLargeLatencyConfig } from 'shared/dao/pick-realm-algorithm/defaults'
import { largeLatencyLink } from 'shared/dao/pick-realm-algorithm/largeLatency'
import { AlgorithmLink } from 'shared/dao/pick-realm-algorithm/types'
import { contextForCandidates } from './test-utils'

describe('Large Latency Algorithm Link', () => {
  let link: AlgorithmLink
  beforeEach(() => {
    link = largeLatencyLink(defaultLargeLatencyConfig)
  })

  it('should sort candidates by latency', () => {
    const context = link.pick(
      contextForCandidates(
        [0, 0],
        { elapsed: 400, catalystName: 'second' },
        { elapsed: 200, catalystName: 'first' },
        { elapsed: 700, catalystName: 'fourth' },
        { elapsed: 500, catalystName: 'third' }
      )
    )

    expect(context.picked.map((it) => it.catalystName)).to.eql(['first', 'second', 'third', 'fourth'])
  })

  it('should filter those candidates with large latency', () => {
    const context = link.pick(
      contextForCandidates(
        [0, 0],
        { elapsed: 400, catalystName: 'not-filtered-1' },
        { elapsed: 500, catalystName: 'not-filtered-2' },
        { elapsed: 4000, catalystName: 'filtered' }
      )
    )

    expect(context.picked.map((it) => it.catalystName)).to.eql(['not-filtered-1', 'not-filtered-2'])
  })

  it('should consider large latency relative to the one with least latency only', () => {
    const context = link.pick(
      contextForCandidates(
        [0, 0],
        { elapsed: 4500, catalystName: 'first' },
        { elapsed: 5000, catalystName: 'second' },
        { elapsed: 5500, catalystName: 'third' }
      )
    )

    expect(context.picked.map((it) => it.catalystName)).to.eql(['first', 'second', 'third'])
  })

  it('should pick one if it has large latency gap with all the rest', () => {
    const context = link.pick(
      contextForCandidates(
        [0, 0],
        { elapsed: 400, catalystName: 'the-one' },
        { elapsed: 4100, catalystName: 'not-you' },
        { elapsed: 4200, catalystName: 'neither-you' }
      )
    )

    expect(context.picked.map((it) => it.catalystName)).to.eql(['the-one'])
    expect(context.selected?.catalystName).to.eql('the-one')
  })
})
