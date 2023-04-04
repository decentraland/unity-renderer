import {
  subscribeParcelSceneToCommsMessages,
  unsubscribeParcelSceneToCommsMessages
} from 'shared/comms/sceneSubscriptions'
import type { ICommunicationsController } from 'shared/comms/sceneSubscriptions'

import type { PeerInformation } from 'shared/comms/interface/types'

import type { RpcServerPort } from '@dcl/rpc'
import * as codegen from '@dcl/rpc/dist/codegen'
import { sendParcelSceneCommsMessage } from 'shared/comms'
import type { PortContext } from './context'
import { CommunicationsControllerServiceDefinition } from 'shared/protocol/decentraland/kernel/apis/communications_controller.gen'

/**
 * The CommunicationsControllerService connects messages from the comms controller with the scenes of Decentraland,
 * particularly the `AvatarScene` that hosts the avatars of other players.
 *
 * @param port connection to scene
 */
export function registerCommunicationsControllerServiceServerImplementation(port: RpcServerPort<PortContext>) {
  codegen.registerService(port, CommunicationsControllerServiceDefinition, async (port, ctx) => {
    /**
     * The `receiveCommsMessage` relays messages in direction: scene -> comms
     */
    const commsController: ICommunicationsController = {
      cid: ctx.sceneData.id,
      receiveCommsMessage(data: Uint8Array, sender: PeerInformation) {
        const message = new TextDecoder().decode(data)
        ctx.sendSceneEvent('comms', {
          message,
          sender: sender.ethereumAddress
        })
      }
    }

    /**
     * Subscribe and unsubscribe to relay mesages in direction: comms -> scene
     */
    subscribeParcelSceneToCommsMessages(commsController)

    port.on('close', () => {
      unsubscribeParcelSceneToCommsMessages(commsController)
    })

    return {
      async send(req, ctx) {
        const message = new TextEncoder().encode(req.message)
        sendParcelSceneCommsMessage(ctx.sceneData.id, message)
        return {}
      }
    }
  })
}
