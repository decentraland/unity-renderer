import { expect } from 'chai'
import { overloadedCatalystLink } from 'shared/dao/pick-realm-algorithm/overloadedCatalyst'
import { AlgorithmLink } from 'shared/dao/pick-realm-algorithm/types'
import { contextForCandidates } from './test-utils'
import { Parcel } from 'shared/dao/types'

const ANY_PARCEL: Parcel = [0, 0]

describe('Overloaded Catalyst Link', () => {
  let link: AlgorithmLink
  beforeEach(() => {
    link = overloadedCatalystLink()
  })

  it('should filter out the nodes that are not accepting users', () => {
    const context = link.pick(
      contextForCandidates(
        ANY_PARCEL,
        { catalystName: 'filtered', acceptingUsers: false },
        { catalystName: 'filtered', acceptingUsers: false },
        { catalystName: 'first', acceptingUsers: true },
        { catalystName: 'second', acceptingUsers: true }
      )
    )

    expect(context.picked.map((it) => it.catalystName)).to.eql(['first', 'second'])
  })

  it('should preserve list order when there are more than a single pick', () => {
    const context = link.pick(
      contextForCandidates(
        [0, 0],
        { catalystName: 'first', acceptingUsers: true },
        { catalystName: 'filtered', acceptingUsers: false },
        { catalystName: 'filtered', acceptingUsers: false },
        { catalystName: 'second', acceptingUsers: true }
      )
    )

    expect(context.picked.map((it) => it.catalystName)).to.eql(['first', 'second'])
  })

  it('should select when there is only one candidate inside definitive selection threshold', () => {
    const context = link.pick(
      contextForCandidates(
        [0, 0],
        { catalystName: 'filtered', acceptingUsers: false },
        { catalystName: 'filtered', acceptingUsers: false },
        { catalystName: 'single', acceptingUsers: true },
        { catalystName: 'filtered', acceptingUsers: false }
      )
    )

    expect(context.picked.map((it) => it.catalystName)).to.eql(['single'])
    expect(context.selected?.catalystName).to.eql('single')
  })
})
