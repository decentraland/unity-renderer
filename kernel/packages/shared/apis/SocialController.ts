import { exposeMethod, APIOptions, registerAPI } from 'decentraland-rpc/lib/host'
import { ExposableAPI } from './ExposableAPI'
import { EngineAPI } from 'shared/apis/EngineAPI'
import {
  addToMutedUsers,
  removeFromMutedUsers,
  addToBlockedUsers,
  removeFromBlockedUsers,
  avatarMessageObservable,
  getBlockedUsers,
  getMutedUsers
} from 'shared/comms/peers'
import { AvatarMessage } from 'shared/comms/interface/types'
import { AVATAR_OBSERVABLE } from 'decentraland-ecs/src/decentraland/Types'

export interface IProfileData {
  displayName: string
  publicKey: string
  status: string
  avatarType: string
  isMuted: boolean
  isBlocked: boolean
}

@registerAPI('SocialController')
export class SocialController extends ExposableAPI {
  static pluginName = 'SocialController'

  constructor(options: APIOptions) {
    super(options)

    const engineAPI = options.getAPIInstance(EngineAPI)

    avatarMessageObservable.add((event: any) => {
      engineAPI.sendSubscriptionEvent(AVATAR_OBSERVABLE as any, event)
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

  @exposeMethod
  async getBlockedUsers() {
    return [...getBlockedUsers()]
  }

  @exposeMethod
  async getMutedUsers() {
    return [...getMutedUsers()]
  }
}

export namespace avatarMock {
  export function sendMessage(msg: AvatarMessage) {
    avatarMessageObservable.notifyObservers(msg)
  }
}

// tslint:disable-next-line:semicolon
;(global as any)['avatarMock'] = avatarMock
