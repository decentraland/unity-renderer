import { Candidate, Realm } from 'shared/dao/types'
import { AboutResponse } from 'shared/protocol/decentraland/bff/http_endpoints.gen'
import { ExplorerIdentity } from 'shared/session/types'
import { createBffRpcConnection } from './connections/BFFConnection'
import { localBff } from './connections/BFFLegacy'
import { IRealmAdapter, OFFLINE_REALM } from './types'

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
  // normalize about response
  about.content = {
    healthy: false,
    publicUrl: baseUrl + '/content',
    ...about.content
  }
  about.lambdas = {
    healthy: false,
    publicUrl: baseUrl + '/lambdas',
    ...about.lambdas
  }
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

  // TODO: We are checking !v2 until all migration is finished
  const isValidBff = about.comms?.protocol === 'v3' && about.bff?.healthy // about.bff?.healthy

  // connect the real BFF
  if (isValidBff) {
    return createBffRpcConnection(baseUrl, about, identity)
  }

  // return a mocked BFF
  return localBff(baseUrl, about, identity)
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

function isDclEns(str: string | undefined): str is `${string}.dcl.eth` {
  return !!str?.match(/^[a-zA-Z0-9]+\.dcl\.eth$/)?.length
}

function dclWorldUrl(dclName: string) {
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
