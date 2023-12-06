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
 * MsgType utils to diff between old string messages, and new uint8Array messages.
 */
enum MsgType {
  String = 1,
  Uint8Array = 2
}

function decodeMessage(value: Uint8Array): [MsgType, Uint8Array] {
  const msgType = value.at(0) as MsgType
  const data = value.subarray(1)
  return [msgType, data]
}

function encodeMessage(data: Uint8Array, type: MsgType) {
  const message = new Uint8Array(data.byteLength + 1)
  message.set([type])
  message.set(data, 1)
  return message
}

/**
 * The CommunicationsControllerService connects messages from the comms controller with the scenes of Decentraland,
 * particularly the `AvatarScene` that hosts the avatars of other players.
 *
 * @param port connection to scene
 */
export function registerCommunicationsControllerServiceServerImplementation(port: RpcServerPort<PortContext>) {
  codegen.registerService(port, CommunicationsControllerServiceDefinition, async (port, ctx) => {
    const eventsToProcess: Uint8Array[] = []
    const textEncoder = new TextEncoder()
    const textDecoder = new TextDecoder()
    /**
     * The `receiveCommsMessage` relays messages in direction: scene -> comms
     */
    const commsController: ICommunicationsController = {
      cid: ctx.sceneData.id,
      receiveCommsMessage(preData: Uint8Array, sender: PeerInformation) {
        const [msgType, data] = decodeMessage(preData)
        if (msgType === MsgType.String) {
          const message = textDecoder.decode(data)

          ctx.sendSceneEvent('comms', {
            message,
            sender: sender.ethereumAddress
          })
        } else if (msgType === MsgType.Uint8Array) {
          if (!data.byteLength) return
          const senderBytes = textEncoder.encode(sender.ethereumAddress)
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
        const message = textEncoder.encode(req.message)
        sendParcelSceneCommsMessage(ctx.sceneData.id, encodeMessage(message, MsgType.String))
        return {}
      },
      async sendBinary(req, ctx) {
        // Send messages
        for (const data of req.data) {
          sendParcelSceneCommsMessage(ctx.sceneData.id, encodeMessage(data, MsgType.Uint8Array))
        }

        // Process received messages
        const messages = [...eventsToProcess]
        // clean messages
        eventsToProcess.length = 0
        return { data: messages }
      }
    }
  })
}
