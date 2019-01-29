import { IEventNames, IEvents } from 'decentraland-ecs/src/decentraland/Types'

export type RPCEvent<K extends IEventNames, D = any> = {
  event: K
  data: D
}

export function createEvent<T extends IEventNames>(event: T, data: IEvents[T]): RPCEvent<T, IEvents[T]> {
  return { event, data }
}
