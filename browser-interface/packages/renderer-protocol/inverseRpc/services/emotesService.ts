import { RpcServerPort } from '@dcl/rpc'
import { RendererProtocolContext } from '../context'
import * as codegen from '@dcl/rpc/dist/codegen'
import { EmotesKernelServiceDefinition } from 'shared/protocol/decentraland/renderer/kernel_services/emotes_kernel.gen'
import { allScenesEvent } from 'shared/world/parcelSceneManager'
import { sendPublicChatMessage } from 'shared/comms'

export function registerEmotesKernelService(port: RpcServerPort<RendererProtocolContext>) {
  codegen.registerService(port, EmotesKernelServiceDefinition, async () => ({
    async triggerExpression(req, _) {
      allScenesEvent({
        eventType: 'playerExpression',
        payload: {
          expressionId: req.id
        }
      })

      const body = `␐${req.id} ${req.timestamp}`

      sendPublicChatMessage(body)
      return {}
    }
  }))
}
