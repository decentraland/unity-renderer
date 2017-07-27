import { ISystem } from '../ecs/System'
import { Engine, EngineEvent } from '../ecs/Engine'
import { Entity, ComponentAdded, ComponentRemoved, ParentChanged } from '../ecs/Entity'
import { UUIDEvent } from './Events'
import {
  DisposableComponentCreated,
  DisposableComponentRemoved,
  DisposableComponentUpdated,
  isDisposableComponent,
  getComponentId,
  getComponentClassId,
  ObservableComponent
} from '../ecs/Component'

import { DecentralandInterface } from './Types'

export class DecentralandSynchronizationSystem implements ISystem {
  cachedComponents: Record<string, Record<string, string>> = {}
  engine!: Engine

  constructor(public dcl: DecentralandInterface) {}

  activate(engine: Engine) {
    this.engine = engine
    engine.eventManager.addListener(ComponentAdded, this, this.componentAdded)
    engine.eventManager.addListener(ComponentRemoved, this, this.componentRemoved)
    engine.eventManager.addListener(DisposableComponentCreated, this, this.disposableComponentCreated)
    engine.eventManager.addListener(DisposableComponentRemoved, this, this.disposableComponentRemoved)
    engine.eventManager.addListener(DisposableComponentUpdated, this, this.disposableComponentUpdated)
    engine.eventManager.addListener(ParentChanged, this, this.parentChanged)

    const rootId = engine.rootEntity.uuid

    this.dcl.addEntity(rootId)

    // TODO(agus): send disposableComponents if exist

    this.dcl.onUpdate(dt => {
      engine.update(dt)
      this.presentEntities()
    })
    this.dcl.onEvent(event => {
      switch (event.type) {
        case 'uuidEvent':
          const e = new UUIDEvent()
          e.uuid = event.data.uuid
          e.payload = event.data.payload
          engine.eventManager.fireEvent(e)
          break

        default:
          engine.eventManager.fireEvent(new EngineEvent(event))
          break
      }
    })
  }

  /**
   * system.onRemoveEntity is called by the engine when a entity is added to the
   * engine.
   */
  onAddEntity(entity: Entity) {
    if (entity && entity.isAddedToEngine()) {
      const entityId = entity.uuid
      const parent = entity.getParent()

      this.dcl.addEntity(entityId)

      if (parent) {
        // If the entity has a parent, we send the the enparenting signal
        // otherwise the engine will know the entity is set as a child of
        // engine.rootEntity by default
        this.dcl.setParent(entityId, parent.uuid)
      }

      // This creates a cache dictionary to avoid send redundant information to
      // the engine in order to avoid unnecessary work in the main thread.
      this.cachedComponents[entityId] = {}

      // this iterator sends the current components of te engine at the moment
      // of addition
      for (let componentName in entity.components) {
        const component = entity.components[componentName]
        const classId = getComponentClassId(component)

        if (classId !== null) {
          if (isDisposableComponent(component)) {
            // Send the attach component signal
            this.dcl.attachEntityComponent(entity.uuid, componentName, getComponentId(component))
          } else {
            const componentJson = JSON.stringify(component)

            // Send the updated component
            this.dcl.updateEntityComponent(entityId, componentName, classId, componentJson)

            // Update the cached copy of the sent component
            this.cachedComponents[entityId][componentName] = componentJson
          }
        }
      }
    }
  }

  /**
   * system.onRemoveEntity is called by the engine when a entity gets removed
   * from the engine.
   */
  onRemoveEntity(entity: Entity) {
    if (entity.isAddedToEngine()) {
      const entityId = entity.uuid

      // Send the removeEntity signal
      this.dcl.removeEntity(entityId)

      // Remove the caches from local memory
      delete this.cachedComponents[entityId]
    }
  }

  /**
   * This method is called at the end of every update cycle.
   * It finds and sends updates in components of the engine entities.
   */
  private presentEntities() {
    for (let i in this.engine.entities) {
      const entity = this.engine.entities[i]
      const entityId = entity.uuid

      for (let componentName in entity.components) {
        const component = entity.components[componentName]
        const classId = getComponentClassId(component)

        if (classId !== null && !isDisposableComponent(component)) {
          const componentJson = JSON.stringify(component)

          if (this.cachedComponents[entityId][componentName] !== componentJson) {
            // Send the updated component
            this.dcl.updateEntityComponent(entity.uuid, componentName, classId, componentJson)

            // Update the cached copy of the sent component
            this.cachedComponents[entityId][componentName] = componentJson
          }
        }
      }
    }

    for (let id in this.engine.disposableComponents) {
      const component = this.engine.disposableComponents[id]
      if (component instanceof ObservableComponent && component.dirty) {
        this.dcl.componentUpdated(id, JSON.stringify(component))
        component.dirty = false
      }
    }
  }

  /**
   * This method is called after a component is added to an entity. The event
   * (param 1) contains the necessary information to notify the engine about the
   * component that was added and the entity.
   */
  private componentAdded(event: ComponentAdded) {
    if (event.entity.isAddedToEngine()) {
      const component = event.entity.components[event.componentName]

      if (isDisposableComponent(component)) {
        this.dcl.attachEntityComponent(event.entity.uuid, event.componentName, getComponentId(component))
      } else if (event.classId !== null) {
        const componentJson = JSON.stringify(component)

        // Send the updated component
        this.dcl.updateEntityComponent(event.entity.uuid, event.componentName, event.classId, componentJson)

        // Update the cached copy of the sent component
        this.cachedComponents[event.entity.uuid][event.componentName] = componentJson
      }
    }
  }

  /**
   * This method is called when a component is removed from an entity.
   */
  private componentRemoved(event: ComponentRemoved) {
    if (event.entity.isAddedToEngine()) {
      this.dcl.removeEntityComponent(event.entity.uuid, event.componentName)
    }
  }

  /**
   * This method is called after a disposableComponent is created.
   * It instantiates the component in the engine, the event that updates the
   * created component is fired immediatly after.
   */
  private disposableComponentCreated(event: DisposableComponentCreated) {
    this.dcl.componentCreated(event.componentId, event.componentName, event.classId)
  }

  /**
   * This method is called after a disposableComponent is updated, once per
   * update cycle and once after creation.
   */
  private disposableComponentRemoved(event: DisposableComponentRemoved) {
    this.dcl.componentDisposed(event.componentId)
  }

  /**
   * This method is called right after a diposableComponent gets disposed. That
   * process is manual.
   *
   * TODO(menduz,dani): What happens if a disposableComponent gets disposed and
   * it remains attached to some entities?
   */
  private disposableComponentUpdated(event: DisposableComponentUpdated) {
    this.dcl.componentUpdated(event.componentId, JSON.stringify(event.component))
  }

  /**
   * This method is called when a parent changes in an entity.
   */
  private parentChanged(event: ParentChanged) {
    this.dcl.setParent(event.entity.uuid, event.parent.uuid)
  }
}
