import { expect } from 'chai'
import { closePeersScoreLink } from 'shared/dao/pick-realm-algorithm/closePeers'
import { defaultClosePeersScoreConfig } from 'shared/dao/pick-realm-algorithm/defaults'
import { AlgorithmLink } from 'shared/dao/pick-realm-algorithm/types'
import { contextForCandidates } from './test-utils'

describe('Close Peers Score Link', () => {
  let link: AlgorithmLink
  beforeEach(() => {
    link = closePeersScoreLink(defaultClosePeersScoreConfig)
  })

  it('Should prefer close peers', () => {
    const context = link.pick(
      contextForCandidates(
        [100, 100],
        {
          catalystName: 'second',
          usersCount: 13,
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
            [100, 100],
            [100, 100],
            [100, 100]
          ]
        },
        {
          catalystName: 'first',
          usersCount: 20,
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
        { catalystName: 'third', usersCount: 100 }
      )
    )

    expect(context.picked.map((it) => it.catalystName)).to.eql(['first', 'second'])
  })

  it('should select using close peers when inside definitive selection threshold', () => {
    const context = link.pick(
      contextForCandidates(
        [100, 100],
        {
          catalystName: 'second',
          usersCount: 5,
          usersParcels: [
            [100, 100],
            [100, 100],
            [100, 100],
            [100, 100],
            [0, 0]
          ]
        },
        {
          catalystName: 'first',
          usersCount: 20,
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
        { catalystName: 'third', usersCount: 100 }
      )
    )

    expect(context.picked.map((it) => it.catalystName)).to.eql(['first'])
    expect(context.selected?.catalystName).to.eql('first')
  })
})
