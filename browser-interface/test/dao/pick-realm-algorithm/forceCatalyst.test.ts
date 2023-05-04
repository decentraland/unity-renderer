import { expect } from 'chai'
import { AlgorithmLink, ForceCatalystParameters } from 'shared/dao/pick-realm-algorithm/types'
import { contextForCandidates } from './test-utils'
import { Parcel } from 'shared/dao/types'
import { forceCatalystLink } from 'shared/dao/pick-realm-algorithm/forceCatalyst'

const ANY_PARCEL: Parcel = [0, 0]

describe('Force Catalyst Link', () => {
  let link: AlgorithmLink

  it('should select first option if present', () => {
    link = getLinkWith({ sortedOptions: ['first'] })
    const context = link.pick(
      contextForCandidates(ANY_PARCEL, { catalystName: 'third' }, { catalystName: 'second' }, { catalystName: 'first' })
    )

    expect(context.selected!.catalystName).to.eql('first')
  })

  it('should select second option if the first one is not present', () => {
    link = getLinkWith({ sortedOptions: ['first', 'second'] })
    const context = link.pick(
      contextForCandidates(
        ANY_PARCEL,
        { catalystName: 'third' },
        { catalystName: 'second' },
        { catalystName: 'fourth' }
      )
    )

    expect(context.selected!.catalystName).to.eql('second')
  })

  it('should select third option if the first two are not present', () => {
    link = getLinkWith({ sortedOptions: ['first', 'second', 'third'] })
    const context = link.pick(
      contextForCandidates(
        ANY_PARCEL,
        { catalystName: 'fourth' },
        { catalystName: 'fifth' },
        { catalystName: 'sixth' },
        { catalystName: 'third' }
      )
    )

    expect(context.selected!.catalystName).to.eql('third')
  })

  it('do nothing when none of the options are found', () => {
    link = getLinkWith({ sortedOptions: ['not-found'] })
    const context = link.pick(
      contextForCandidates(
        ANY_PARCEL,
        { catalystName: 'first' },
        { catalystName: 'second' },
        { catalystName: 'third' },
        { catalystName: 'fourth' }
      )
    )

    expect(context.selected).to.eql(undefined)
    expect(context.picked.map((candidate) => candidate.catalystName)).to.eql(['first', 'second', 'third', 'fourth'])
  })
})

function getLinkWith(params: ForceCatalystParameters) {
  return forceCatalystLink(params)
}
