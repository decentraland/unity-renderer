import { Observable } from '../../decentraland-ecs/src/ecs/Observable'

export type BrokerMessage = {
  data: Uint8Array
  channel: string
}

export interface IBrokerConnection {
  onMessageObservable: Observable<BrokerMessage>
  readonly hasUnreliableChannel: boolean
  readonly hasReliableChannel: boolean
  sendReliable(data: Uint8Array): void
  sendUnreliable(data: Uint8Array): void
  printDebugInformation(): void
  close(): void
}
