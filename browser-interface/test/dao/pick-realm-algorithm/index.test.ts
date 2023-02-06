import { expect } from 'chai'
import { createAlgorithm } from 'shared/dao/pick-realm-algorithm'
import { defaultChainConfig } from 'shared/dao/pick-realm-algorithm/defaults'
import { buildCandidates } from './test-utils'

describe('Pick realm algorithm default configuration', () => {
  let algorithm: ReturnType<typeof createAlgorithm>
  beforeEach(() => {
    algorithm = createAlgorithm(defaultChainConfig)
  })

  it.skip('Should select using large latency first', () => {
    const candidate = algorithm.pickCandidate(
      buildCandidates(
        { elapsed: 4000, catalystName: 'second', usersCount: 1000 },
        { elapsed: 200, catalystName: 'first', usersCount: 10 },
        { elapsed: 4600, catalystName: 'fourth', usersCount: 5000 },
        { elapsed: 4500, catalystName: 'third', usersCount: 10000 }
      ),
      [0, 0]
    )

    expect(candidate.catalystName).to.eql('first')
  })

  it.skip('Should select using close peers second', () => {
    const candidate = algorithm.pickCandidate(
      buildCandidates(
        { catalystName: 'second', usersCount: 1000 },
        {
          catalystName: 'first',
          usersCount: 11,
          usersParcels: [
            [100, 100],
            [100, 100],
            [100, 100],
            [100, 100],
            [100, 100],
            [100, 100],
            [100, 100],
            [100, 100],
            [100, 100],
            [100, 100],
            [100, 100]
          ]
        },
        { catalystName: 'fourth', usersCount: 5000 },
        { catalystName: 'third', usersCount: 10000 }
      ),
      [100, 100]
    )

    expect(candidate.catalystName).to.eql('first')
  })

  it('Should select using all peers third', () => {
    const candidate = algorithm.pickCandidate(
      buildCandidates(
        { catalystName: 'second', usersCount: 5100, elapsed: 2000 },
        { catalystName: 'first', usersCount: 5000 },
        { catalystName: 'fourth', usersCount: 2000 },
        { catalystName: 'third', usersCount: 3000 }
      ),
      [0, 0]
    )

    expect(candidate.catalystName).to.eql('first')
  })

  it('Should select using load balancing last', () => {
    const candidate = algorithm.pickCandidate(
      buildCandidates(
        { catalystName: 'second', usersCount: 2000, domain: 'foo.bar' },
        { catalystName: 'first', usersCount: 2000, domain: 'asd.fgh' },
        { catalystName: 'third', usersCount: 2000, domain: 'foo.bar' }
      ),
      [0, 0]
    )

    expect(candidate.catalystName).to.eql('first')
  })

  it('should pick the first one if all the candidates are the same', () => {
    const candidate = algorithm.pickCandidate(
      buildCandidates({ catalystName: 'first' }, { catalystName: 'second' }, { catalystName: 'third' }),
      [0, 0]
    )

    expect(candidate.catalystName).to.eql('first')
  })

  it('throw error if an empty list of candidates is provided', () => {
    expect(() => algorithm.pickCandidate([], [0, 0])).to.throw('Cannot pick candidates from an empty list')
  })
})
