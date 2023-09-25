import defaultLogger from 'lib/logger'
import type {RemoteProfileWithHash} from 'shared/profiles/types'
import { ensureRealmAdapter } from 'shared/realm/ensureRealmAdapter'

export async function requestProfile(userId: string, version?: number): Promise<RemoteProfileWithHash | null> {
  const backendConfiguration = await ensureRealmAdapter()
  try {
    let explorerUrl = backendConfiguration.services.lambdasServer.replace("/lambdas", "/explorer");
    let url = `${explorerUrl}/profiles/${userId}`
    if (version) {
      url = url + `?version=${version}`
    } else if (!userId.startsWith('default')) {
      url = url + `?no-cache=${Math.random()}`
    }

    const response = await fetch(url)

    if (!response.ok) {
      throw new Error(`Invalid response from ${url}`)
    }

    const res: RemoteProfileWithHash = await response.json()

    return res
  } catch (e: any) {
    defaultLogger.error(e)
    return null
  }
}
