import { Engine } from '../ecs/Engine'
import { UUIDEvent } from './Events'
import { ISystem } from '../ecs/System'
import { DecentralandInterface } from './Types'
import { OnUUIDEvent } from './Components'
import { ComponentAdded, ComponentRemoved, Entity } from '../ecs/Entity'

declare var dcl: DecentralandInterface | void

/**
 * @public
 */
export class UUIDEventSystem implements ISystem {
  handlerMap: { [uuid: string]: OnUUIDEvent<any> } = {}

  activate(engine: Engine) {
    engine.eventManager.addListener(UUIDEvent, this, this.handleEvent)
    engine.eventManager.addListener(ComponentAdded, this, this.componentAdded)
    engine.eventManager.addListener(ComponentRemoved, this, this.componentRemoved)

    if (typeof dcl !== 'undefined') {
      dcl.subscribe('uuidEvent')
    }
  }

  deactivate() {
    if (typeof dcl !== 'undefined') {
      dcl.unsubscribe('uuidEvent')
    }
  }

  onAddEntity(entity: Entity) {
    for (let componentName in entity.components) {
      const component = entity.components[componentName]

      if (component instanceof OnUUIDEvent) {
        this.handlerMap[component.uuid] = component
      }
    }
  }

  onRemoveEntity(entity: Entity) {
    for (let componentName in entity.components) {
      const component = entity.components[componentName]

      if (component instanceof OnUUIDEvent) {
        delete this.handlerMap[component.uuid]
      }
    }
  }

  private componentAdded(event: ComponentAdded) {
    if (event.entity.isAddedToEngine()) {
      const component = event.entity.components[event.componentName]

      if (component instanceof OnUUIDEvent) {
        this.handlerMap[component.uuid] = component
      }
    }
  }

  private componentRemoved(event: ComponentRemoved) {
    if (event.entity.isAddedToEngine()) {
      if (event.component instanceof OnUUIDEvent) {
        delete this.handlerMap[event.component.uuid]
      }
    }
  }

  private handleEvent(event: UUIDEvent): void {
    if (event.uuid in this.handlerMap) {
      const handler = this.handlerMap[event.uuid]
      if (handler) {
        if (handler.callback && 'call' in handler.callback) {
          handler.callback(event.payload)
        }
      }
    }
  }
}
