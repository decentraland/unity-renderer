import { EventDispatcher } from '../../../lib/common/core/EventDispatcher'
import { Script } from '../../../lib/client'

export interface IMessageBusOptions {}

export interface IMessage {
  event: string
  args: any[]
  sender: string
}

export class MessageBusClient<T = any> extends EventDispatcher<T> {
  private broadcastIdentifier = `Broadcast_${this.id}`

  private constructor(protected api: any, protected id: string, protected busClientId: string) {
    super()
    api[`on${this.broadcastIdentifier}`]((message: IMessage) => {
      if (this.busClientId !== message.sender) {
        super.emit(message.event, ...message.args)
      }
    })
  }

  static async acquireChannel(system: Script, channelName: string, options: IMessageBusOptions = {}) {
    const busId = Math.random().toString(36)
    const { MessageBus } = await system.loadAPIs(['MessageBus'])

    const bus = await MessageBus.getChannel(channelName, busId, options)

    return new MessageBusClient(MessageBus, bus.id, busId)
  }

  emit(event: string, ...args: any[]) {
    this.api[this.broadcastIdentifier]({
      event,
      args,
      sender: this.busClientId
    } as IMessage)
    super.emit(event, ...args)
  }
}
