import { registerAPI, exposeMethod } from 'decentraland-rpc/lib/host'
import { ExposableAPI } from './ExposableAPI'
import { UserData } from 'shared/types'
import { getCurrentUser } from 'shared/comms/peers'

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
    const user = getCurrentUser()
    if (!user) return null

    return user.publicKey
  }

  @exposeMethod
  async getUserData(): Promise<UserData | null> {
    const user = getCurrentUser()

    if (!user) return null

    return {
      displayName: user.displayName,
      publicKey: user.publicKey
    }
  }
}
