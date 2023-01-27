import { RpcClientModule } from '@dcl/rpc/dist/codegen'
import { AsyncQueue } from '@dcl/rpc/dist/push-channel'
import { CommsServiceDefinition } from '@dcl/protocol/out-ts/decentraland/bff/comms_service.gen'
import {
  PeerTopicSubscriptionResultElem,
  SystemTopicSubscriptionResultElem
} from '@dcl/protocol/out-ts/decentraland/bff/topics_service.gen'

export function localCommsService(): RpcClientModule<CommsServiceDefinition, any> {
  type TestTopicListener = {
    pattern: string
    queue: AsyncQueue<PeerTopicSubscriptionResultElem & SystemTopicSubscriptionResultElem>
    subscriptionId: number
  }
  let count = 0
  const subscriptions = new Map<number, TestTopicListener>()

  function newSub(topic: string) {
    ++count
    const sub: TestTopicListener = {
      pattern: topic,
      queue: new AsyncQueue(() => void 0),
      subscriptionId: count
    }
    subscriptions.set(sub.subscriptionId, sub)
    return sub
  }

  function unsub(id: number) {
    const sub = subscriptions.get(id)

    if (sub) {
      sub.queue.close()
      subscriptions.delete(id)
    }

    return { ok: !!sub }
  }

  return {
    getPeerMessages(subscription) {
      const sub = subscriptions.get(subscription.subscriptionId)
      if (!sub) throw new Error('Subscription not found')
      return sub.queue
    },
    getSystemMessages(subscription) {
      const sub = subscriptions.get(subscription.subscriptionId)
      if (!sub) throw new Error('Subscription not found')
      return sub.queue
    },
    async publishToTopic(message) {
      subscriptions.forEach((l) => {
        const sPattern = l.pattern.split('.')
        const sTopic = message.topic.split('.')
        if (sPattern.length !== sTopic.length) {
          return
        }
        for (let i = 0; i < sTopic.length; i++) {
          if (sPattern[i] !== '*' && sPattern[i] !== sTopic[i]) {
            return
          }
        }
        l.queue.enqueue({
          payload: message.payload,
          sender: 'self',
          topic: message.topic
        })
      })
      return { ok: true }
    },
    async subscribeToPeerMessages(message) {
      return newSub(message.topic)
    },
    async subscribeToSystemMessages(message) {
      return newSub(message.topic)
    },
    async unsubscribeToPeerMessages(message) {
      return unsub(message.subscriptionId)
    },
    async unsubscribeToSystemMessages(message) {
      return unsub(message.subscriptionId)
    }
  }
}
