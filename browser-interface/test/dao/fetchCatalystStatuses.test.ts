import sinon from 'sinon'
import { expect } from 'chai'
import { fetchCatalystStatuses } from 'shared/dao'
import * as ping from 'shared/dao/utils/ping'
import { Candidate } from 'shared/dao/types'

const EXPECTED: Candidate = {
  protocol: 'v2',
  catalystName: 'loki',
  domain: 'peer.decentraland.org',
  elapsed: 309,
  maxUsers: 2000,
  status: 0,
  usersCount: 59,
  acceptingUsers: true,
  usersParcels: [
    [1, 1],
    [1, 1],
    [1, 1]
  ],
  version: { content: '1.0.0', lambdas: '1.0.0', bff: '1.0.0', comms: 'v2' }
}

describe('Fetch catalyst server status', () => {
  beforeEach(() => {
    sinon.reset()
    sinon.restore()
  })

  it('Should return catalyst status servers not included in the deny list', async () => {
    const askFunction: typeof ping.ask = async (domain) => {
      if (domain.endsWith('/about')) {
        return {
          status: 0,
          elapsed: 309,
          httpStatus: 200,
          result: {
            comms: {
              protocol: EXPECTED.protocol,
              version: EXPECTED.protocol
            },
            configurations: {
              realmName: EXPECTED.catalystName
            },
            bff: {
              userCount: EXPECTED.usersCount,
              version: '1.0.0'
            },
            content: {
              version: '1.0.0'
            },
            lambdas: {
              version: '1.0.0'
            },
            acceptingUsers: true
          }
        }
      } else {
        return {
          status: 0,
          elapsed: 309,
          httpStatus: 200,
          result: {
            parcels: [
              {
                peersCount: 3,
                parcel: {
                  x: 1,
                  y: 1
                }
              }
            ]
          }
        }
      }
    }

    const NODES = [
      {
        domain: 'peer.decentraland.org'
      },
      {
        domain: 'peer-ec1.decentraland.org'
      }
    ]

    const results = await fetchCatalystStatuses(NODES, ['peer-ec1.decentraland.org'], askFunction)
    expect(results.length).to.eql(1)
    console.log({ a: results[0], b: EXPECTED })
    expect(results[0]).to.eql(EXPECTED)
  })
})
