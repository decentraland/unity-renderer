import { DecentralandInterface, ModuleDescriptor, IEvents } from './Types'
import { Observable, Observer } from '../ecs/Observable'
import { error } from '../ecs/helpers'

declare const dcl: DecentralandInterface

let communicationsController: ModuleDescriptor | null = null
let communicationsControllerPromise: PromiseLike<ModuleDescriptor> | null = null

let _messageObserver: null | Observable<IEvents['comms']> = null

/**
 * @internal
 */
export function getMessageObserver() {
  if (!_messageObserver) {
    _messageObserver = new Observable<IEvents['comms']>()
  }
  return _messageObserver
}

function ensureCommunicationsController() {
  if (!communicationsControllerPromise) {
    communicationsControllerPromise = dcl.loadModule('@decentraland/CommunicationsController')

    communicationsControllerPromise.then($ => {
      communicationsController = $
    })

    const observer = getMessageObserver()

    dcl.subscribe('comms')
    dcl.onEvent(event => {
      if (event.type === 'comms') {
        observer.notifyObservers(event.data as any)
      }
    })
  }
  return communicationsControllerPromise
}

/**
 * @public
 */
export class MessageBus {
  private messageQueue: string[] = []
  private connected = false
  private flushing = false

  constructor() {
    ensureCommunicationsController().then($ => {
      this.connected = true
      this.flush()
    })
  }

  on(message: string, callback: (value: any, sender: string) => void): Observer<IEvents['comms']> {
    return getMessageObserver().add(e => {
      try {
        let m = JSON.parse(e.message)

        if (m.message === message) {
          callback(m.payload, e.sender)
        }
      } catch (e) {
        dcl.error('Error parsing comms message ' + e.message, e)
      }
    })!
  }

  // @internal
  sendRaw(message: string) {
    this.messageQueue.push(message)

    if (this.connected) {
      this.flush()
    }
  }

  emit(message: string, payload: Record<any, any>) {
    const messageToSend = JSON.stringify({ message, payload })
    this.sendRaw(messageToSend)
    getMessageObserver().notifyObservers({ message: messageToSend, sender: 'self' })
  }

  private flush() {
    if (this.messageQueue.length === 0) return
    if (!this.connected) return
    if (!communicationsController) return
    if (this.flushing) return

    const message = this.messageQueue.shift()

    this.flushing = true

    dcl.callRpc(communicationsController.rpcHandle, 'send', [message]).then(
      _ => {
        this.flushing = false
        this.flush()
      },
      e => {
        this.flushing = false
        error('Error flushing MessageBus', e)
      }
    )
  }
}
