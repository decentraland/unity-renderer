import { Client } from 'decentraland-rpc/lib/common/json-rpc/Client'
import { ScriptingTransport } from 'decentraland-rpc/lib/common/json-rpc/types'

export class Adapter extends Client {
  constructor(public transport: ScriptingTransport) {
    super({})

    if (transport.onError) {
      transport.onError(e => {
        this.emit('error', e)
      })
    }

    if (transport.onClose) {
      transport.onClose(() => {
        this.emit('transportClosed')
      })
    }

    transport.onMessage(message => {
      this.processMessage(message)
    })

    if (transport.onConnect) {
      transport.onConnect(() => {
        this.didConnect()
      })
    } else {
      this.didConnect()
    }
  }

  sendMessage(message: string) {
    this.transport.sendMessage(message)
  }
}
