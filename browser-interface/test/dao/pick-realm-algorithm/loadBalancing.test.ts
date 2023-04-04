import { expect } from 'chai'
import { loadBalancingLink } from 'shared/dao/pick-realm-algorithm/loadBalancing'
import { AlgorithmLink } from 'shared/dao/pick-realm-algorithm/types'
import { contextForCandidates } from './test-utils'

describe('Load Balancing Link', () => {
  let link: AlgorithmLink
  beforeEach(() => {
    link = loadBalancingLink()
  })

  it('Should prefer the domain with the least amount of peers', () => {
    const context = link.pick(
      contextForCandidates(
        [0, 0],
        { catalystName: 'second', usersCount: 150, domain: 'x.y' },
        { catalystName: 'first', usersCount: 100, domain: 'asd.fgh' },
        { catalystName: 'third', usersCount: 2000, domain: 'v.w' }
      )
    )

    expect(context.picked.map((it) => it.catalystName)).to.eql(['first', 'second', 'third'])
    expect(context.selected?.catalystName).to.eql('first')
  })
})
