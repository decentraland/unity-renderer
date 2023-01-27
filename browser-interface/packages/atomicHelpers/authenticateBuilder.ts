import { Authenticator } from '@dcl/crypto'
import { ExplorerIdentity } from 'shared/session/types'

const AUTH_CHAIN_HEADER_PREFIX = 'x-identity-auth-chain-'

export function authorizeBuilderHeaders(identity: ExplorerIdentity, method: string = 'get', path: string = '') {
  const headers: Record<string, string> = {}

  if (identity) {
    const endpoint = (method + ':' + path).toLowerCase()
    const authChain = Authenticator.signPayload(identity, endpoint)
    for (let i = 0; i < authChain.length; i++) {
      headers[AUTH_CHAIN_HEADER_PREFIX + i] = JSON.stringify(authChain[i])
    }
  }

  return headers
}
