import { registerAPI, API, exposeMethod, APIOptions, getExposedMethods } from '../../../lib/host'
import { EventDispatcher, EventDispatcherBinding } from '../../../lib/common/core/EventDispatcher'

const messageBus = new EventDispatcher()

export class IntermediateApi extends API {
  @exposeMethod
  async sayHi() {
    // stub
  }
}

@registerAPI('MessageBus')
export class MessageBusManager extends IntermediateApi {
  joinedTo: EventDispatcherBinding[] = []

  constructor(options: APIOptions) {
    super(options)
    const methods = getExposedMethods(this)

    if (!methods.has('getChannel')) {
      console.log(this)
      throw new Error('missing getChannel')
    }

    if (!methods.has('sayHi')) {
      console.log(this)
      throw new Error('missing sayHi')
    }
  }

  @exposeMethod
  async getChannel(name: string, uid: string, options: any) {
    const id = (Math.random() * 100000000).toFixed(0)

    const key = 'Broadcast_' + id

    this.joinedTo.push(
      messageBus.on(name, (message: any) => {
        try {
          this.options.notify(key, message)
        } catch (e) {
          console.error(e)
        }
      })
    )

    this.options.expose(key, async (message: any) => {
      messageBus.emit(name, message)
    })

    return { id }
  }

  apiWillUnmount() {
    // TODO: test component will unmount
    this.joinedTo.forEach($ => $.off())
    this.joinedTo.length = 0
  }
}
