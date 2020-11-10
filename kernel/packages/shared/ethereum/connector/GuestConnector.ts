import { WebSocketProvider } from 'eth-connect'
import { ConnectorInterface } from './ConnectorInterface'

export class GuestConnector implements ConnectorInterface {
  private readonly wss: string

  constructor(wss: string) {
    this.wss = wss
  }

  getProvider(): any {
    return new WebSocketProvider(this.wss)
  }

  isAvailable(): boolean {
    return true
  }

  login(): Promise<any> {
    return Promise.resolve(true)
  }

  logout(): Promise<boolean> {
    return Promise.resolve(true)
  }
}
