import { EventConstructor } from '../ecs/EventManager'
import { Observable } from '../ecs/Observable'
import { DecentralandInterface, IEvents, RaycastResponsePayload } from './Types'

/**
 * @public
 */
@EventConstructor()
export class UUIDEvent<T = any> {
  constructor(public readonly uuid: string, public readonly payload: T) { }
}

/**
 * @public
 */
@EventConstructor()
export class RaycastResponse<T> {
  constructor(
    public readonly payload: RaycastResponsePayload<T>
  ) { }
}

/**
 * @public
 */
@EventConstructor()
export class PointerEvent<GlobalInputEventResult> {
  constructor(public readonly payload: GlobalInputEventResult) { }
}

let internalDcl: DecentralandInterface | void

/**
 * @internal
 * This function generates a callback that is passed to the Observable
 * constructor to subscribe to the events of the DecentralandInterface
 */
function createSubscriber(eventName: keyof IEvents) {
  return () => {
    if (internalDcl) {
      internalDcl.subscribe(eventName)
    }
  }
}

/**
 * These events are triggered after your character enters the scene.
 * @public
 */
export const onEnterScene = new Observable<IEvents['onEnterScene']>(createSubscriber('onEnterScene'))

/**
 * These events are triggered after your character leaves the scene.
 * @public
 */
export const onLeaveScene = new Observable<IEvents['onLeaveScene']>(createSubscriber('onLeaveScene'))

/**
 * @internal
 * This function adds _one_ listener to the onEvent event of dcl interface.
 * Leveraging a switch to route events to the Observable handlers.
 */
export function _initEventObservables(dcl: DecentralandInterface) {
  // store internal reference to dcl, it is going to be used to subscribe to the events
  internalDcl = dcl

  if (internalDcl) {
    internalDcl.onEvent((event) => {
      switch (event.type) {
        case 'onEnterScene': {
          onEnterScene.notifyObservers(event.data as IEvents['onEnterScene'])
          return
        }
        case 'onLeaveScene': {
          onLeaveScene.notifyObservers(event.data as IEvents['onLeaveScene'])
          return
        }
      }
    })
  }
}
