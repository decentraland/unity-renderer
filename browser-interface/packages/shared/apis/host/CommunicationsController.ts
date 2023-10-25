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
    const eventsToProcess: Uint8Array[] = []
    /**
     * The `receiveCommsMessage` relays messages in direction: scene -> comms
     */
    const commsController: ICommunicationsController = {
      cid: ctx.sceneData.id,
      receiveCommsMessage(data: Uint8Array, sender: PeerInformation) {
        const message = new TextDecoder().decode(data)

        // String CommsMessages (old MessageBus)
        if (message) {
          ctx.sendSceneEvent('comms', {
            message,
            sender: sender.ethereumAddress
          })
        }

        // Uint8Array CommsMessage (BinaryMessageBus)
        if (data.byteLength) {
          const senderBytes = new TextEncoder().encode(sender.ethereumAddress)
          const messageLength = senderBytes.byteLength + data.byteLength + 1

          const serializedMessage = new Uint8Array(messageLength)
          serializedMessage.set(new Uint8Array([senderBytes.byteLength]), 0)
          serializedMessage.set(senderBytes, 1)
          serializedMessage.set(data, senderBytes.byteLength + 1)

          eventsToProcess.push(serializedMessage)
        }
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
      },
      async sendBinary(req, ctx) {
        for (const data of req.data) {
          sendParcelSceneCommsMessage(ctx.sceneData.id, data)
        }
        const messages = [...eventsToProcess]

        // clean messages
        eventsToProcess.length = 0
        return { data: messages }
      }
    }
  })
}
