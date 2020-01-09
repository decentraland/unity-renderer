// tslint:disable:prefer-function-over-method
import { registerAPI, exposeMethod, APIOptions } from 'decentraland-rpc/lib/host'

import {
  sendParcelSceneCommsMessage,
  subscribeParcelSceneToCommsMessages,
  unsubscribeParcelSceneToCommsMessages
} from 'shared/comms'
import { ExposableAPI } from 'shared/apis/ExposableAPI'
import { EngineAPI } from 'shared/apis/EngineAPI'
import { ParcelIdentity } from './ParcelIdentity'
import { PeerInformation } from 'shared/comms/interface/types'

@registerAPI('CommunicationsController')
export class CommunicationsController extends ExposableAPI {
  parcelIdentity = this.options.getAPIInstance(ParcelIdentity)
  engineAPI = this.options.getAPIInstance(EngineAPI)

  get cid() {
    return this.parcelIdentity.cid
  }

  constructor(options: APIOptions) {
    super(options)
    subscribeParcelSceneToCommsMessages(this)
  }

  apiWillUnmount() {
    // Unsubscribe this parcel from events
    unsubscribeParcelSceneToCommsMessages(this)
  }

  receiveCommsMessage(message: string, sender: PeerInformation) {
    this.engineAPI.sendSubscriptionEvent('comms', {
      message,
      sender: sender.uuid
    })
  }

  @exposeMethod
  async send(message: string): Promise<void> {
    sendParcelSceneCommsMessage(this.cid, message)
  }
}
