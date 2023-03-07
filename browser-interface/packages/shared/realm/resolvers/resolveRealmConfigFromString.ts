import { resolveRealmAboutFromBaseUrl } from './resolveRealmAboutFromBaseUrl'
import { resolveOfflineRealmAboutFromConnectionString } from './resolveOfflineRealmAboutFromConnectionString'

export async function resolveRealmConfigFromString(realmString: string) {
  return (
    (await resolveOfflineRealmAboutFromConnectionString(realmString)) ||
    (await resolveRealmAboutFromBaseUrl(realmString))
  )
}
