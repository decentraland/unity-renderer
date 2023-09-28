import { Candidate, Realm } from 'shared/dao/types'
import { AboutResponse } from 'shared/protocol/decentraland/renderer/about.gen'
import { ExplorerIdentity } from 'shared/session/types'
import { createArchipelagoConnection } from './connections/ArchipelagoConnection'
import { localArchipelago } from './connections/LocalArchipelago'
import { IRealmAdapter, OFFLINE_REALM } from './types'
import { commsLogger } from 'shared/comms/logger'

function normalizeUrl(url: string) {
  return url.replace(/^:\/\//, window.location.protocol + '//')
}

// adds the currently used protocol to the given URL
export function urlWithProtocol(urlOrHostname: string) {
  if (!urlOrHostname.startsWith('http://') && !urlOrHostname.startsWith('https://') && !urlOrHostname.startsWith('://'))
    return normalizeUrl(`https://${urlOrHostname}`)

  return normalizeUrl(urlOrHostname)
}

export async function adapterForRealmConfig(
  baseUrl: string,
  about: AboutResponse,
  identity: ExplorerIdentity
): Promise<IRealmAdapter> {
  // about.content = {
  //   healthy: false,
  //   ...about.content,
  //   publicUrl: `${about.content?.publicUrl}/asdf` || `${baseUrl}/bad`
  // }
  // about.lambdas = {
  //   healthy: false,
  //   ...about.lambdas,
  //   publicUrl: about.lambdas?.publicUrl + 'asdf' || baseUrl + '/bad'
  // }
  about.configurations = {
    networkId: 1,
    globalScenesUrn: [],
    scenesUrn: [],
    minimap: {
      enabled: true,
      dataImage: 'https://api.decentraland.org/v1/minimap.png',
      estateImage: 'https://api.decentraland.org/v1/estatemap.png'
    },
    ...about.configurations
  }

  commsLogger.info(`adapterForRealmConfig: ${JSON.stringify(about)}`)

  if (
    about.comms?.healthy &&
    'adapter' in about.comms &&
    about.comms.adapter.startsWith('archipelago:archipelago-v1')
  ) {
    const archipelagoUrl = about.comms.adapter.substring('archipelago:archipelago-v1'.length)
    commsLogger.info(
      'comms healthy creating archipelago connection, ahora esta harcodeado en healthy, entonces lo deberia crear ' +
        archipelagoUrl
    )
    return createArchipelagoConnection(baseUrl, archipelagoUrl, about, identity)
  }

  // return a mocked Archipelago
  return localArchipelago(baseUrl, about, identity)
}

export function prettyRealmName(realm: string, candidates: Candidate[]) {
  // is it a DAO realm?
  for (const candidate of candidates) {
    if (candidate.catalystName === realm) {
      return candidate.catalystName
    }
  }

  return realm
}

export function isDclEns(str: string | undefined): str is `${string}.dcl.eth` {
  return !!str?.match(/^[a-zA-Z0-9]+\.dcl\.eth$/)?.length
}

export function dclWorldUrl(dclName: string) {
  return `https://worlds-content-server.decentraland.org/world/${encodeURIComponent(dclName.toLowerCase())}`
}

export function realmToConnectionString(realm: IRealmAdapter) {
  const realmName = realm.about.configurations?.realmName

  if ((realm.about.comms?.protocol === 'v2' || realm.about.comms?.protocol === 'v3') && realmName?.match(/^[a-z]+$/i)) {
    return realmName
  }

  if (isDclEns(realmName) && realm.baseUrl === dclWorldUrl(realmName)) {
    return realmName
  }

  if (realmName === OFFLINE_REALM || realmName?.startsWith(OFFLINE_REALM + '?')) {
    return realmName
  }

  return realm.baseUrl.replace(/^https?:\/\//, '').replace(/^wss?:\/\//, '')
}

export function resolveRealmBaseUrlFromRealmQueryParameter(realmString: string, candidates: Candidate[]): string {
  // is it a DAO realm?
  for (const candidate of candidates) {
    if (candidate.catalystName === realmString) {
      return urlWithProtocol(candidate.domain)
    }
  }

  if (isDclEns(realmString)) {
    return dclWorldUrl(realmString)
  }

  return urlWithProtocol(realmString)
}

export function candidateToRealm(candidate: Candidate): Realm {
  return {
    hostname: urlWithProtocol(candidate.domain),
    protocol: candidate.protocol || 'v2',
    serverName: candidate.catalystName
  }
}
