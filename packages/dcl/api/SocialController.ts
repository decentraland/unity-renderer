// tslint:disable:prefer-function-over-method
import { exposeMethod, APIOptions, registerAPI } from 'decentraland-rpc/lib/host'
import {
  addToMutedUsers,
  removeFromMutedUsers,
  getMutedUsers,
  getBlockedUsers,
  addToBlockedUsers,
  removeFromBlockedUsers,
  ProfileEvent,
  profileObservable
} from 'shared/comms/profile'
import { hideBlockedUsers, muteUsers } from 'dcl/comms/peers'

import { ExposableAPI } from '../../shared/apis/ExposableAPI'
import { EngineAPI } from 'shared/apis/EngineAPI'

export interface IProfileData {
  displayName: string
  publicKey: string
  status: string
  avatarType: string
  isMuted: boolean
  isBlocked: boolean
}

export interface SocialController {
  /**
   * Adds user to list of muted users (stored in localStorage)
   * @param user
   */
  mute(user: string): void
  /**
   * Adds user to list of blocked users (stored in localStorage)
   * @param user
   */
  block(user: string): void
}

@registerAPI('SocialController')
export class SocialController extends ExposableAPI {
  static pluginName = 'SocialController'

  constructor(options: APIOptions) {
    super(options)

    const engineAPI = options.getAPIInstance(EngineAPI)

    profileObservable.add((event: any) => {
      if (event.type === ProfileEvent.SHOW_PROFILE || event.type === ProfileEvent.HIDE_PROFILE) {
        const { type, ...data } = event
        engineAPI.sendSubscriptionEvent(event.type, data)
      }
    })
  }

  @exposeMethod
  async mute(publicKey: string) {
    if (!getMutedUsers().has(publicKey)) {
      addToMutedUsers(publicKey)
      muteUsers(getMutedUsers())
    }

    profileObservable.notifyObservers({
      type: ProfileEvent.MUTE,
      publicKey
    })
  }

  @exposeMethod
  async unmute(publicKey: string) {
    if (!getMutedUsers().has(publicKey)) {
      removeFromMutedUsers(publicKey)
      muteUsers(getMutedUsers())
    }

    profileObservable.notifyObservers({
      type: ProfileEvent.UNMUTE,
      publicKey
    })
  }

  @exposeMethod
  async block(publicKey: string) {
    if (!getBlockedUsers().has(publicKey)) {
      addToBlockedUsers(publicKey)
      hideBlockedUsers(getBlockedUsers())
    }

    if (!getMutedUsers().has(publicKey)) {
      addToMutedUsers(publicKey)
      muteUsers(getMutedUsers())
    }

    profileObservable.notifyObservers({
      type: ProfileEvent.BLOCK,
      publicKey
    })
  }

  @exposeMethod
  async unblock(publicKey: string) {
    if (!getBlockedUsers().has(publicKey)) {
      removeFromBlockedUsers(publicKey)
      hideBlockedUsers(getBlockedUsers())
    }

    if (!getMutedUsers().has(publicKey)) {
      removeFromMutedUsers(publicKey)
      muteUsers(getMutedUsers())
    }

    profileObservable.notifyObservers({
      type: ProfileEvent.UNBLOCK,
      publicKey
    })
  }
}
