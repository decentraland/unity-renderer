import { IEngine, ISystem, IEntity, ComponentAdded, ComponentRemoved } from './IEntity'

import {
  getComponentName,
  getComponentId,
  DisposableComponentUpdated,
  DisposableComponentLike,
  ComponentConstructor,
  DisposableComponentCreated,
  DisposableComponentRemoved,
  getComponentClassId
} from './Component'
import { EventManager } from './EventManager'
import { ComponentGroup } from './ComponentGroup'

import { log, error } from './helpers'
import { Entity } from './Entity'

/**
 * @internal
 */
type SystemEntry = { system: ISystem; priority: number }

/**
 * @public
 */
export class Engine implements IEngine {
  readonly eventManager: EventManager = new EventManager()
  readonly rootEntity: IEntity
  readonly firstPersonCameraEntity: IEntity
  readonly avatarEntity: IEntity

  // @internal
  readonly systems: SystemEntry[] = []

  // @internal
  readonly entityLists: Record<string, Record<string, IEntity>> = {}

  // @internal
  readonly addedSystems: ISystem[] = []

  private readonly _entities: Record<string, IEntity> = {}
  private readonly _disposableComponents: Record<string, DisposableComponentLike> = {}
  private readonly _componentGroups: Record<string, ComponentGroup[]> = {}

  // systems that doesn't require any component or handle their own logic
  private readonly simpleSystems: ISystem[] = []

  get entities() {
    return this._entities as Readonly<Record<string, IEntity>>
  }

  get disposableComponents() {
    return this._disposableComponents as Readonly<Record<string, DisposableComponentLike>>
  }

  constructor(rootEntity: IEntity) {
    this.eventManager.addListener(ComponentAdded, this, this.componentAddedHandler)
    this.eventManager.addListener(ComponentRemoved, this, this.componentRemovedHandler)
    this.rootEntity = rootEntity
    this.firstPersonCameraEntity = new Entity();
    (this.firstPersonCameraEntity as any).uuid = 'FirstPersonCameraEntityReference'
    this.addEntity(this.firstPersonCameraEntity)
    this.avatarEntity = new Entity();
    (this.avatarEntity as any).uuid = 'AvatarEntityReference'
    this.addEntity(this.avatarEntity)
  }

  addEntity(entity: IEntity): IEntity {
    const parent = entity.getParent()

    if (entity.isAddedToEngine()) {
      return entity
    }

    entity.eventManager = this.eventManager;
    (entity as Entity).engine = this

    this._entities[entity.uuid] = entity

    this.checkRequirementsAndAdd(entity)

    if (!parent) {
      entity.setParent(this.rootEntity)
    } else {
      if (!parent.isAddedToEngine() && parent !== this.rootEntity) {
        log('Engine: warning, added an entity with a parent not present in the engine. Parent id: ' + parent.uuid)
      }
    }

    entity.alive = true

    for (let i in entity.children) {
      const child = entity.children[i]
      if (child) {
        if (!child.isAddedToEngine()) {
          this.addEntity(child)
        }
      }
    }

    return entity
  }

  removeEntity(entity: IEntity): boolean {
    const id = entity.uuid

    if (entity.isAddedToEngine()) {
      for (let componentName in entity.components) {
        const componentGroups = this._componentGroups[componentName]

        if (componentGroups) {
          for (let groupIndex in componentGroups) {
            componentGroups[groupIndex].removeEntity(entity)
          }
        }

        delete this.entityLists[componentName][id]
      }

      for (let i = 0; i < this.simpleSystems.length; i++) {
        const system = this.simpleSystems[i]

        if (system.onRemoveEntity) {
          system.onRemoveEntity(entity)
        }
      }

      for (let i in entity.children) {
        const child = entity.children[i]
        if (child) {
          this.removeEntity(child)
        }
      }

      entity.alive = false
      entity.eventManager = null

      delete this._entities[id]

      return true
    } else {
      log('Engine: Trying to remove non existent entity from engine.')
      if (!entity.isAddedToEngine()) {
        log(`Engine: Entity "${entity.uuid}" has not been added to any engine yet.`)
      } else {
        log('Engine: Entity id: ' + id)
      }
      log("Engine: Entity's components:")
      for (let componentName in entity.components) {
        log(componentName)
      }
      return false
    }
  }

  addSystem(system: ISystem, priority: number = 0) {
    if (this.addedSystems.indexOf(system) !== -1) {
      log('Engine: Trying to add a system that is already added. Aborting')
      return system
    }

    if (this.systems.length > 0) {
      for (let i = 0; i < this.systems.length; i++) {
        const entry = this.systems[i]
        const isLast = i === this.systems.length - 1

        if (entry.priority > priority) {
          this.addedSystems.push(system)
          this.systems.splice(i, 0, { system, priority })
          break
        } else if (isLast) {
          this.addedSystems.push(system)
          this.systems.splice(i + 1, 0, { system, priority })
          break
        }
      }
    } else {
      this.addedSystems.push(system)
      this.systems.splice(1, 0, { system, priority })
    }

    this.registerSystem(system)

    return system
  }

  removeSystem(system: ISystem) {
    const idx = this.addedSystems.indexOf(system)

    if (idx !== -1) {
      system.active = false

      if (system.deactivate) {
        system.deactivate()
      }

      this.addedSystems.splice(idx, 1)

      for (let i = 0; i < this.systems.length; i++) {
        const sys = this.systems[i].system
        if (sys === system) {
          this.systems.splice(i, 1)
        }
      }
      return true
    }
    return false
  }

  update(dt: number) {
    for (let i in this.systems) {
      const system = this.systems[i].system
      if (system.active && system.update) {
        try {
          system.update(dt)
        } catch (e) {
          error(e)
        }
      }
    }
    return this
  }

  getEntitiesWithComponent(component: string): Record<string, any>
  getEntitiesWithComponent(component: ComponentConstructor<any>): Record<string, IEntity>
  getEntitiesWithComponent(component: ComponentConstructor<any> | string): Record<string, IEntity> {
    const componentName = typeof component === 'string' ? component : getComponentName(component)

    if (componentName in this.entityLists) {
      return this.entityLists[componentName]
    } else {
      return (this.entityLists[componentName] = {})
    }
  }

  registerComponent(component: DisposableComponentLike) {
    const id = getComponentId(component)
    const name = getComponentName(component)
    const classId = getComponentClassId(component)
    this._disposableComponents[id] = component
    if (classId !== null) {
      this.eventManager.fireEvent(new DisposableComponentCreated(id, name, classId))
      this.eventManager.fireEvent(new DisposableComponentUpdated(id, component))
    }
  }

  disposeComponent(component: DisposableComponentLike) {
    const id = getComponentId(component)

    if (delete this._disposableComponents[id]) {
      this.eventManager.fireEvent(new DisposableComponentRemoved(id))

      if (component.onDispose) {
        component.onDispose()
      }
      return true
    }
    return false
  }

  updateComponent(component: DisposableComponentLike) {
    this.eventManager.fireEvent(new DisposableComponentUpdated(getComponentId(component), component))
  }

  getComponentGroup(...requires: ComponentConstructor<any>[]) {
    let componentGroup = undefined

    // Return an already created component-group if it already exists
    if (requires.length > 0) {
      // 1. get component groups for first require
      let componentGroups = this._componentGroups[getComponentName(requires[0])]

      if (componentGroups) {
        const components = requires.slice()

        // 2. search for a component group that has all the same requirements
        for (let i = 0; i < componentGroups.length; i++) {
          const traversedComponentGroup = componentGroups[i]

          if (components.length === traversedComponentGroup.requires.length) {
            for (let j = 0; j < components.length; j++) {
              if (traversedComponentGroup.requires.indexOf(components[j]) === -1) break

              if (j === (components.length - 1)) {
                componentGroup = traversedComponentGroup
              }
            }

            if (componentGroup) break
          }
        }
      }
    }

    if (componentGroup) {
      // 3. Found an existent component group with the exact same requirements
      return componentGroup
    }

    // Otherwise create and store it
    componentGroup = new ComponentGroup(...requires)

    componentGroup.active = true

    const requiresNames = componentGroup.requiresNames

    for (let i = 0; i < requiresNames.length; i++) {
      const componentName = requiresNames[i]

      let componentGroups = this._componentGroups[componentName]

      if (!componentGroups) {
        this._componentGroups[componentName] = componentGroups = []
      }

      if (componentGroups.indexOf(componentGroup) === -1) {
        componentGroups.push(componentGroup)
      }
    }

    for (let entityId in this._entities) {
      this.checkRequirements(this._entities[entityId], componentGroup)
    }

    return componentGroup
  }

  removeComponentGroup(componentGroup: ComponentGroup) {
    if (componentGroup.active) {
      componentGroup.active = false
      const requiresNames = componentGroup.requiresNames
      for (let i = 0; i < requiresNames.length; i++) {
        const componentName = requiresNames[i]

        let componentGroups = this._componentGroups[componentName]

        if (componentGroups) {
          const idx = componentGroups.indexOf(componentGroup)
          if (idx !== -1) {
            componentGroups.splice(idx, 1)
          }
        }
      }
      return true
    }
    return false
  }

  private registerSystem(system: ISystem) {
    system.active = true

    if (system.activate) {
      system.activate(this)
    }

    this.simpleSystems.push(system)
  }

  private checkRequirementsAndAdd(entity: IEntity) {
    if (!entity.isAddedToEngine()) return

    for (let componentName in entity.components) {
      if (!(componentName in this.entityLists)) {
        this.entityLists[componentName] = {}
      }

      this.entityLists[componentName][entity.uuid] = entity

      const componentGroups = this._componentGroups[componentName]

      if (componentGroups) {
        for (let systemIndex in componentGroups) {
          this.checkRequirements(entity, componentGroups[systemIndex])
        }
      }
    }

    for (let i = 0; i < this.simpleSystems.length; i++) {
      const system = this.simpleSystems[i]

      if (system.onAddEntity) {
        system.onAddEntity(entity)
      }
    }
  }

  private checkRequirements(entity: IEntity, system: ComponentGroup) {
    if (system.meetsRequirements(entity)) {
      if (!system.hasEntity(entity)) {
        system.addEntity(entity)
      }
    } else {
      if (system.hasEntity(entity)) {
        system.removeEntity(entity)
      }
    }
  }

  private componentAddedHandler(event: ComponentAdded) {
    const { entity, componentName } = event

    if (!entity.isAddedToEngine()) return

    if (!this.entityLists[componentName]) {
      this.entityLists[componentName] = { [entity.uuid]: entity }
    } else {
      this.entityLists[componentName][entity.uuid] = entity
    }

    const componentGroups = this._componentGroups[componentName]

    if (componentGroups) {
      for (let i in componentGroups) {
        this.checkRequirements(entity, componentGroups[i])
      }
    }
  }

  private componentRemovedHandler(event: ComponentRemoved) {
    // In case a single component gets removed from an entity, we inform
    // all systems that this entity lost this specific component.
    const { entity, componentName } = event

    if (!entity.isAddedToEngine()) return

    delete this.entityLists[componentName][entity.uuid]

    const componentGroups = this._componentGroups[componentName]

    if (componentGroups) {
      for (let i in componentGroups) {
        this.checkRequirements(entity, componentGroups[i])
      }
    }
  }
}
