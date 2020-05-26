import { Server } from '../common/json-rpc/Server'
import { IServerOpts, ScriptingTransport } from '../common/json-rpc/types'

export class TransportBasedServer extends Server<ScriptingTransport> {
  constructor(public transport: ScriptingTransport, opt: IServerOpts = {}) {
    super(opt)

    if (!this.transport) {
      throw new TypeError('transport cannot be undefined or null')
    }

    this.transport.onMessage(msg => {
      this.processMessage(this.transport, msg)
    })

    if (this.transport.onError) {
      this.transport.onError(err => this.emit('error', err))
    }

    if (this.transport.onClose) {
      this.transport.onClose(() => this.disable())
    }
  }

  sendMessage(receiver: ScriptingTransport, message: string) {
    receiver.sendMessage(message)
  }

  getAllClients() {
    return [this.transport]
  }
}
