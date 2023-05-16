import { expect } from 'chai'
import { AlgorithmLink, VersionCatalystParameters } from 'shared/dao/pick-realm-algorithm/types'
import { contextForCandidates } from './test-utils'
import { versionCatalystLink } from 'shared/dao/pick-realm-algorithm/versionCatalyst'
import { Parcel } from 'shared/dao/types'

const ANY_PARCEL: Parcel = [0, 0]
const DEFAULT_VERSIONS = { content: '1.0.0', lambdas: '1.0.0', bff: '1.0.0', comms: 'v3' }

describe('Version Catalyst Link', () => {
  let link: AlgorithmLink

  it('should discard those candidates that do not meet content version when it is set', () => {
    link = getLinkWith({ content: '9.0.0' })
    const context = link.pick(
      contextForCandidates(
        ANY_PARCEL,
        { catalystName: 'equal', version: { ...DEFAULT_VERSIONS, content: '9.0.0' } },
        { catalystName: 'majorGreater', version: { ...DEFAULT_VERSIONS, content: '10.0.0' } },
        { catalystName: 'midGreater', version: { ...DEFAULT_VERSIONS, content: '9.1.0' } },
        { catalystName: 'minorGreater', version: { ...DEFAULT_VERSIONS, content: '9.0.1' } },
        { catalystName: 'lower', version: { ...DEFAULT_VERSIONS, content: '8.9.9' } }
      )
    )

    expect(context.picked.map((it) => it.catalystName)).to.eql(['equal', 'majorGreater', 'midGreater', 'minorGreater'])
  })

  it('should discard those candidates that do not meet lambdas version when it is set', () => {
    link = getLinkWith({ lambdas: '9.0.0' })
    const context = link.pick(
      contextForCandidates(
        ANY_PARCEL,
        { catalystName: 'equal', version: { ...DEFAULT_VERSIONS, lambdas: '9.0.0' } },
        { catalystName: 'majorGreater', version: { ...DEFAULT_VERSIONS, lambdas: '10.0.0' } },
        { catalystName: 'midGreater', version: { ...DEFAULT_VERSIONS, lambdas: '9.1.0' } },
        { catalystName: 'minorGreater', version: { ...DEFAULT_VERSIONS, lambdas: '9.0.1' } },
        { catalystName: 'lower', version: { ...DEFAULT_VERSIONS, lambdas: '8.9.9' } }
      )
    )

    expect(context.picked.map((it) => it.catalystName)).to.eql(['equal', 'majorGreater', 'midGreater', 'minorGreater'])
  })

  it('should discard those candidates that do not meet bff version when it is set', () => {
    link = getLinkWith({ bff: '9.0.0' })
    const context = link.pick(
      contextForCandidates(
        ANY_PARCEL,
        { catalystName: 'equal', version: { ...DEFAULT_VERSIONS, bff: '9.0.0' } },
        { catalystName: 'majorGreater', version: { ...DEFAULT_VERSIONS, bff: '10.0.0' } },
        { catalystName: 'midGreater', version: { ...DEFAULT_VERSIONS, bff: '9.1.0' } },
        { catalystName: 'minorGreater', version: { ...DEFAULT_VERSIONS, bff: '9.0.1' } },
        { catalystName: 'lower', version: { ...DEFAULT_VERSIONS, bff: '8.9.9' } }
      )
    )

    expect(context.picked.map((it) => it.catalystName)).to.eql(['equal', 'majorGreater', 'midGreater', 'minorGreater'])
  })

  it('should discard those candidates that do not meet comms protocol version when it is set', () => {
    link = getLinkWith({ comms: 'v3' })
    const context = link.pick(
      contextForCandidates(
        ANY_PARCEL,
        { catalystName: 'equal', version: { ...DEFAULT_VERSIONS, comms: 'v3' } },
        { catalystName: 'greater', version: { ...DEFAULT_VERSIONS, comms: 'v4' } },
        { catalystName: 'filered', version: { ...DEFAULT_VERSIONS, comms: 'v2' } },
        { catalystName: 'filtered', version: { ...DEFAULT_VERSIONS, comms: 'v1' } }
      )
    )

    expect(context.picked.map((it) => it.catalystName)).to.eql(['equal', 'greater'])
  })

  it('should discard those candidates that do not meet two required version when set while preserving order', () => {
    link = getLinkWith({ bff: '9.0.0', content: '7.6.3' })
    const context = link.pick(
      contextForCandidates(
        ANY_PARCEL,
        { catalystName: 'first', version: { ...DEFAULT_VERSIONS, bff: '9.0.0', content: '7.7.2' } },
        { catalystName: 'second', version: { ...DEFAULT_VERSIONS, bff: '10.0.0', content: '7.6.3' } },
        { catalystName: 'filtered', version: { ...DEFAULT_VERSIONS, bff: '9.1.0', content: '7.1.2' } },
        { catalystName: 'third', version: { ...DEFAULT_VERSIONS, bff: '9.0.1', content: '11.1.1' } },
        { catalystName: 'filtered', version: { ...DEFAULT_VERSIONS, bff: '8.9.9', content: '6.9.9' } },
        { catalystName: 'filtered', version: { ...DEFAULT_VERSIONS, bff: '9.0.0' } },
        { catalystName: 'fourth', version: { ...DEFAULT_VERSIONS, bff: '10.0.0', content: '7.7.0' } }
      )
    )

    expect(context.picked.map((it) => it.catalystName)).to.eql(['first', 'second', 'third', 'fourth'])
  })

  it('should not discard candidates if base version was wrongly set', () => {
    link = getLinkWith({ bff: '90.0', comms: '3', content: '99.234', lambdas: '12.3,4' })
    const context = link.pick(
      contextForCandidates(
        ANY_PARCEL,
        { catalystName: 'first' },
        { catalystName: 'second' },
        { catalystName: 'third' },
        { catalystName: 'fourth' }
      )
    )

    expect(context.picked.map((it) => it.catalystName)).to.eql(['first', 'second', 'third', 'fourth'])
  })

  it('should set selected when only a single candidate pass the', () => {
    link = getLinkWith({ content: '9.0.0' })
    const context = link.pick(
      contextForCandidates(
        ANY_PARCEL,
        { catalystName: 'filtered', version: { ...DEFAULT_VERSIONS } },
        { catalystName: 'filtrered', version: { ...DEFAULT_VERSIONS } },
        { catalystName: 'filtered', version: { ...DEFAULT_VERSIONS } },
        { catalystName: 'single', version: { ...DEFAULT_VERSIONS, content: '9.0.0' } },
        { catalystName: 'filtered', version: { ...DEFAULT_VERSIONS } }
      )
    )

    expect(context.picked.map((it) => it.catalystName)).to.eql(['single'])
    expect(context.selected?.catalystName).to.eql('single')
  })

  it('should set selected when only a single candidate pass the even when a base version is wrongly set', () => {
    link = getLinkWith({ content: '9.0.0', comms: '3' })
    const context = link.pick(
      contextForCandidates(
        ANY_PARCEL,
        { catalystName: 'filtered', version: { ...DEFAULT_VERSIONS } },
        { catalystName: 'filtrered', version: { ...DEFAULT_VERSIONS } },
        { catalystName: 'filtered', version: { ...DEFAULT_VERSIONS } },
        { catalystName: 'single', version: { ...DEFAULT_VERSIONS, content: '9.0.0' } },
        { catalystName: 'filtered', version: { ...DEFAULT_VERSIONS } }
      )
    )

    expect(context.picked.map((it) => it.catalystName)).to.eql(['single'])
    expect(context.selected?.catalystName).to.eql('single')
  })

  it('should not fail/filter if the single semver provided is invalid', () => {
    link = getLinkWith({ content: 'invalid' })
    const context = link.pick(
      contextForCandidates(
        ANY_PARCEL,
        { catalystName: 'first', version: { ...DEFAULT_VERSIONS } },
        { catalystName: 'second', version: { ...DEFAULT_VERSIONS, content: '9.0.0' } }
      )
    )
    expect(context.picked.map((it) => it.catalystName)).to.eql(['first', 'second'])
  })

  it('should not fail/filter if the single version provided is comms and it is invalid', () => {
    link = getLinkWith({ comms: 'invalid' })
    const context = link.pick(
      contextForCandidates(
        ANY_PARCEL,
        { catalystName: 'first', version: { ...DEFAULT_VERSIONS } },
        { catalystName: 'second', version: { ...DEFAULT_VERSIONS, comms: 'v1' } }
      )
    )
    expect(context.picked.map((it) => it.catalystName)).to.eql(['first', 'second'])
  })

  // These first two use cases are temporary until to avoid critical side-effects issues
  // until we refactor loadBalancing link to get candidates from .all instead of .picked
  // so we can use it as fall-back at the end of the rules
  describe('should not fail or filter out all the candidates when', () => {
    it('an invalid semver is received from candidates list', () => {
      link = getLinkWith({ content: '1.0.0' })
      const context = link.pick(
        contextForCandidates(
          ANY_PARCEL,
          { catalystName: 'first', version: { ...DEFAULT_VERSIONS, content: 'invalid' } },
          { catalystName: 'second', version: { ...DEFAULT_VERSIONS, content: 'invalid' } }
        )
      )
      expect(context.picked.map((it) => it.catalystName)).to.eql(['first', 'second'])
    })

    it('an invalid protocol comms version is received in the candidates', () => {
      link = getLinkWith({ comms: 'v2' })
      const context = link.pick(
        contextForCandidates(
          ANY_PARCEL,
          { catalystName: 'first', version: { ...DEFAULT_VERSIONS, comms: 'invalid' } },
          { catalystName: 'second', version: { ...DEFAULT_VERSIONS, comms: 'invalid' } }
        )
      )
      expect(context.picked.map((it) => it.catalystName)).to.eql(['first', 'second'])
    })

    it('an invalid link configuration is provided', () => {
      link = getLinkWith()
      const context = link.pick(
        contextForCandidates(
          ANY_PARCEL,
          { catalystName: 'first', version: { ...DEFAULT_VERSIONS, comms: 'invalid' } },
          { catalystName: 'second', version: { ...DEFAULT_VERSIONS, comms: 'invalid' } }
        )
      )
      expect(context.picked.map((it) => it.catalystName)).to.eql(['first', 'second'])
    })
  })
})

function getLinkWith(params?: VersionCatalystParameters) {
  return versionCatalystLink(params)
}
