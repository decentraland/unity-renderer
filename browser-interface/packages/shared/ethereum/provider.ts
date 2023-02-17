import { AuthIdentity } from '@dcl/crypto'
import { RequestManager } from 'eth-connect'
import { now } from 'lib/javascript/now'

export const requestManager = new RequestManager((window as any).ethereum ?? null)

export function isSessionExpired(userData?: { identity: AuthIdentity }) {
  if (!userData || !userData.identity || !userData.identity.expiration) {
    return false
  }
  const expiration = userData.identity.expiration
  if (typeof expiration === 'number') {
    return expiration < now()
  }
  if ((expiration as any).getTime) {
    return expiration.getTime() < now()
  }
  return false
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
