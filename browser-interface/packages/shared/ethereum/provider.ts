import { RequestManager } from 'eth-connect'
import { now } from 'lib/javascript/now'
import { StoredSession } from 'shared/session/types'

export const requestManager = new RequestManager((window as any).ethereum ?? null)

export function isSessionExpired(userData: StoredSession) {
  if (!userData || !userData.identity) {
    return false
  }
  const expiration = userData.identity.expiration as any
  if (!expiration) {
    return true
  } else if (typeof expiration === 'number') {
    return expiration < now()
  } else if (expiration.getTime) {
    return expiration.getTime() < now()
  } else {
    return new Date(expiration).getTime() < now()
  }
}

export async function getUserAccount(
  requestManager: RequestManager,
  returnChecksum: boolean = false
): Promise<string | undefined> {
  try {
    const accounts = await requestManager.eth_accounts()

    if (!accounts || accounts.length === 0) {
      return undefined
    }

    return returnChecksum ? accounts[0] : accounts[0].toLowerCase()
  } catch (error: any) {
    throw new Error(`Could not access eth_accounts: "${error.message}"`)
  }
}
