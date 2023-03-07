import { Candidate } from 'shared/catalystSelection/types'
import { getAllCatalystCandidates } from 'shared/catalystSelection/selectors'
import { store } from 'shared/store/isolatedStore'
import { checkValidRealm } from '../sagas'
import { resolveRealmBaseUrlFromRealmQueryParameter } from 'shared/realm/resolver'
import { AboutResponse } from '@dcl/protocol/out-ts/decentraland/bff/http_endpoints.gen'

export async function resolveRealmAboutFromBaseUrl(
  realmString: string
): Promise<{ about: AboutResponse; baseUrl: string } | undefined> {
  // load candidates if necessary
  const allCandidates: Candidate[] = getAllCatalystCandidates(store.getState())
  const realmBaseUrl = resolveRealmBaseUrlFromRealmQueryParameter(realmString, allCandidates).replace(/\/+$/, '')

  if (!realmBaseUrl) {
    throw new Error(`Can't resolve realm ${realmString}`)
  }

  const res = await checkValidRealm(realmBaseUrl)
  if (!res || !res.result) {
    return undefined
  }

  return { about: res.result!, baseUrl: realmBaseUrl }
}
