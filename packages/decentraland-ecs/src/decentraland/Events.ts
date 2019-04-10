import { EventConstructor } from '../ecs/EventManager'

/**
 * @public
 */
@EventConstructor()
export class UUIDEvent<T = any> {
  constructor(public readonly uuid: string, public readonly payload: T) {}
}
