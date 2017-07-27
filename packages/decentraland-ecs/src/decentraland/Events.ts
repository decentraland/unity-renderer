import { EventConstructor } from '../ecs/EventManager'

/**
 * @public
 */
@EventConstructor('uuidEvent')
export class UUIDEvent<T = any> {
  uuid!: string
  payload!: T
}
