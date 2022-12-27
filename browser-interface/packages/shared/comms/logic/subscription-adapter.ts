import { RpcClientModule } from '@dcl/rpc/dist/codegen'
import { CommsServiceDefinition } from '@dcl/protocol/out-ts/decentraland/bff/comms_service.gen'
import {
  PeerTopicSubscriptionResultElem,
  SystemTopicSubscriptionResultElem
} from '@dcl/protocol/out-ts/decentraland/bff/topics_service.gen'

// This file exists to adapt the subscription system to its final form once the
// protocol repo changes are merged

export function listenSystemMessage(
  commsService: RpcClientModule<CommsServiceDefinition, any>,
  topic: string,
  handler: (data: SystemTopicSubscriptionResultElem) => Promise<void> | void
) {
  const iter = subscribeToSystemMessage(commsService, topic)
  let closed = false
  async function run() {
    for await (const msg of iter) {
      if (closed) break
      await handler(msg)
    }
  }
  run().catch(console.error)
  return {
    close(): void {
      closed = true
      iter.return?.call(iter).catch()
    }
  }
}

export function listenPeerMessage(
  commsService: RpcClientModule<CommsServiceDefinition, any>,
  topic: string,
  handler: (data: PeerTopicSubscriptionResultElem) => Promise<void> | void
) {
  const iter = subscribeToPeerMessage(commsService, topic)
  let closed = false
  async function run() {
    for await (const msg of iter) {
      if (closed) break
      await handler(msg)
    }
  }
  run().catch(console.error)
  return {
    close(): void {
      closed = true
      iter.return?.call(iter).catch()
    }
  }
}

export async function* subscribeToSystemMessage(
  commsService: RpcClientModule<CommsServiceDefinition, any>,
  topic: string
) {
  const subscription = await commsService.subscribeToSystemMessages({ topic })
  try {
    for await (const msg of commsService.getSystemMessages(subscription)) {
      yield msg
    }
  } finally {
    await commsService.unsubscribeToSystemMessages(subscription)
  }
}

export async function* subscribeToPeerMessage(
  commsService: RpcClientModule<CommsServiceDefinition, any>,
  topic: string
) {
  const subscription = await commsService.subscribeToPeerMessages({ topic })
  try {
    yield* commsService.getPeerMessages(subscription)
  } finally {
    await commsService.unsubscribeToPeerMessages(subscription)
  }
}
