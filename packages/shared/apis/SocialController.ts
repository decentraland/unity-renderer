// tslint:disable:prefer-function-over-method
import { exposeMethod, APIOptions, registerAPI } from 'decentraland-rpc/lib/host'
import { ExposableAPI } from './ExposableAPI'
import { EngineAPI } from 'shared/apis/EngineAPI'
import {
  addToMutedUsers,
  removeFromMutedUsers,
  addToBlockedUsers,
  removeFromBlockedUsers,
  avatarMessageObservable
} from 'shared/comms/peers'
import { AvatarMessageType } from 'shared/comms/types'

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

    avatarMessageObservable.add((event: any) => {
      if (event.type === AvatarMessageType.SHOW_PROFILE || event.type === AvatarMessageType.HIDE_PROFILE) {
        const { type, ...data } = event
        engineAPI.sendSubscriptionEvent(event.type, data)
      }
    })
  }

  @exposeMethod
  async mute(publicKey: string) {
    addToMutedUsers(publicKey)
  }

  @exposeMethod
  async unmute(publicKey: string) {
    removeFromMutedUsers(publicKey)
  }

  @exposeMethod
  async block(publicKey: string) {
    addToBlockedUsers(publicKey)
  }

  @exposeMethod
  async unblock(publicKey: string) {
    removeFromBlockedUsers(publicKey)
    removeFromMutedUsers(publicKey)
  }
}
