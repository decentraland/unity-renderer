import { registerAPI, exposeMethod } from 'decentraland-rpc/lib/host'

import { UserData } from 'shared/types'
import { getIdentity } from 'shared/session'
import { StoreContainer } from 'shared/store/rootTypes'
import { calculateDisplayName } from 'shared/profiles/transformations/processServerProfile'
import { getCurrentUserProfile } from 'shared/profiles/selectors'

import { ExposableAPI } from './ExposableAPI'

declare const globalThis: StoreContainer

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

    return identity && identity.hasConnectedWeb3 ? identity.address : null
  }

  @exposeMethod
  async getUserData(): Promise<UserData | null> {
    const identity = getIdentity()
    const profile = getCurrentUserProfile(globalThis.globalStore.getState())

    if (!identity || !profile || !identity.address) return null

    return {
      displayName: calculateDisplayName(identity.address, profile),
      publicKey: identity.hasConnectedWeb3 ? identity.address : null,
      hasConnectedWeb3: !!identity.hasConnectedWeb3,
      userId: identity.address
    }
  }
}
