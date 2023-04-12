import defaultLogger from 'lib/logger'
import type { RemoteProfile } from 'shared/profiles/types'
import { ensureRealmAdapter } from 'shared/realm/ensureRealmAdapter'

export async function requestProfile(userId: string, version?: number): Promise<RemoteProfile | null> {
  const backendConfiguration = await ensureRealmAdapter()
  try {
    let url = `${backendConfiguration.services.lambdasServer}/profiles/${userId}`
    if (version) {
      url = url + `?version=${version}`
    } else if (!userId.startsWith('default')) {
      url = url + `?no-cache=${Math.random()}`
    }

    const response = await fetch(url)

    if (!response.ok) {
      throw new Error(`Invalid response from ${url}`)
    }

    const res: RemoteProfile = await response.json()

    return res
  } catch (e: any) {
    defaultLogger.error(e)
    return null
  }
}
