import { expect } from 'chai'
import { resolveRealmConfigFromString } from 'shared/dao'
import * as r from 'shared/realm/resolver'

function eq<T>(given: T, expected: T) {
  try {
    expect(given).to.deep.eq(expected)
  } catch (e) {
    console.log({ given, expected })
    throw e
  }
}

describe('Comms resolver', () => {
  it('resolveRealmConfigFromString', async () => {
    eq(await resolveRealmConfigFromString('offline'), {
      about: {
        bff: undefined,
        comms: {
          healthy: false,
          protocol: 'offline',
          fixedAdapter: 'offline:offline'
        },
        configurations: {
          realmName: 'offline',
          networkId: 1,
          globalScenesUrn: [],
          scenesUrn: [],
          cityLoaderContentServer: undefined
        },
        content: {
          healthy: true,
          publicUrl: 'https://peer.decentraland.org/content'
        },
        healthy: true,
        lambdas: {
          healthy: true,
          publicUrl: 'https://peer.decentraland.org/lambdas'
        },
        acceptingUsers: true
      },
      baseUrl: 'https://peer.decentraland.org'
    })

    eq(await resolveRealmConfigFromString('offline?baseUrl=peer.decentraland.zone'), {
      about: {
        bff: undefined,
        comms: {
          healthy: false,
          protocol: 'offline',
          fixedAdapter: 'offline:offline'
        },
        configurations: {
          realmName: 'offline?baseUrl=peer.decentraland.zone',
          networkId: 1,
          globalScenesUrn: [],
          scenesUrn: [],
          cityLoaderContentServer: undefined
        },
        content: {
          healthy: true,
          publicUrl: 'https://peer.decentraland.zone/content'
        },
        healthy: true,
        lambdas: {
          healthy: true,
          publicUrl: 'https://peer.decentraland.zone/lambdas'
        },
        acceptingUsers: true
      },
      baseUrl: 'https://peer.decentraland.zone'
    })
    eq(await resolveRealmConfigFromString('offline?baseUrl=https://peer.decentraland.zone'), {
      about: {
        bff: undefined,
        comms: {
          healthy: false,
          protocol: 'offline',
          fixedAdapter: 'offline:offline'
        },
        configurations: {
          realmName: 'offline?baseUrl=https://peer.decentraland.zone',
          networkId: 1,
          globalScenesUrn: [],
          scenesUrn: [],
          cityLoaderContentServer: undefined
        },
        content: {
          healthy: true,
          publicUrl: 'https://peer.decentraland.zone/content'
        },
        healthy: true,
        lambdas: {
          healthy: true,
          publicUrl: 'https://peer.decentraland.zone/lambdas'
        },
        acceptingUsers: true
      },
      baseUrl: 'https://peer.decentraland.zone'
    })
    eq(await resolveRealmConfigFromString('offline?baseUrl=http://peer.decentraland.zone'), {
      about: {
        bff: undefined,
        comms: {
          healthy: false,
          protocol: 'offline',
          fixedAdapter: 'offline:offline'
        },
        configurations: {
          realmName: 'offline?baseUrl=http://peer.decentraland.zone',
          networkId: 1,
          globalScenesUrn: [],
          scenesUrn: [],
          cityLoaderContentServer: undefined
        },
        content: {
          healthy: true,
          publicUrl: 'http://peer.decentraland.zone/content'
        },
        healthy: true,
        lambdas: {
          healthy: true,
          publicUrl: 'http://peer.decentraland.zone/lambdas'
        },
        acceptingUsers: true
      },
      baseUrl: 'http://peer.decentraland.zone'
    })
  })
  it('resolveCommsConnectionString', async () => {
    eq(r.resolveRealmBaseUrlFromRealmQueryParameter('unknown', []), 'https://unknown')

    eq(
      r.resolveRealmBaseUrlFromRealmQueryParameter('unknown', [
        { catalystName: 'unknown', domain: 'unknowndomain', protocol: 'v2' } as any
      ]),
      'https://unknowndomain'
    )

    eq(
      r.resolveRealmBaseUrlFromRealmQueryParameter('dg', [
        { catalystName: 'dg', domain: 'peer.decentral1.io', protocol: 'v2' } as any
      ]),
      'https://peer.decentral1.io'
    )

    eq(
      r.resolveRealmBaseUrlFromRealmQueryParameter('peer.decentral2.io', [
        { catalystName: 'dg', domain: 'peer.decentral2.io', protocol: 'v2' } as any
      ]),
      'https://peer.decentral2.io'
    )

    eq(
      r.resolveRealmBaseUrlFromRealmQueryParameter('https://peer.decentral.io', [
        { catalystName: 'dg', domain: 'peer.decentral.io', protocol: 'v2' } as any
      ]),
      'https://peer.decentral.io'
    )

    eq(
      r.resolveRealmBaseUrlFromRealmQueryParameter('http://peer.decentral.io', [
        { catalystName: 'dg', domain: 'https://peer.decentral.io', protocol: 'v2' } as any
      ]),
      'http://peer.decentral.io'
    )
    eq(
      r.resolveRealmBaseUrlFromRealmQueryParameter('https://peer.decentral.io', [
        { catalystName: 'dg', domain: 'https://peer.decentral.io', protocol: 'v2' } as any
      ]),
      'https://peer.decentral.io'
    )
  })
})
