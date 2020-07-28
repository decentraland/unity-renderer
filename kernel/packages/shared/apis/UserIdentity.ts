import { registerAPI, exposeMethod } from 'decentraland-rpc/lib/host'

import { UserData } from 'shared/types'
import { getCurrentUser } from 'shared/comms/peers'
import { getIdentity } from 'shared/session'

import { ExposableAPI } from './ExposableAPI'

export interface IUserIdentity {
  /**
   * Return the Ethereum address of the user
   */
  getUserPublicKey(): Promise<string | null>

  /**
   * Return the user's data
   */
  getUserData(): Promise<UserData | null>
}

@registerAPI('Identity')
export class UserIdentity extends ExposableAPI implements IUserIdentity {
  @exposeMethod
  async getUserPublicKey(): Promise<string | null> {
    const identity = getIdentity()
    const user = getCurrentUser()
    if (!user || !user.userId) return null

    return identity.hasConnectedWeb3 ? user.userId : null
  }

  @exposeMethod
  async getUserData(): Promise<UserData | null> {
    const identity = getIdentity()
    const user = getCurrentUser()

    if (!user || !user.profile || !user.userId) return null

    return {
      displayName: user.profile.name,
      publicKey: identity.hasConnectedWeb3 ? user.userId : null,
      hasConnectedWeb3: !!identity.hasConnectedWeb3,
      userId: user.userId
    }
  }
}
