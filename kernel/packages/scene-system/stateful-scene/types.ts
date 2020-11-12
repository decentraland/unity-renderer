
export type EntityId = string
export type ComponentId = number
export type ComponentData = any
export type Component = {
  componentId: ComponentId
  data: ComponentData
}

/**
 * An object or entity that contains an updatable definition of the scene state
 */
export interface StateContainer {
  addEntity(entityId: EntityId, components?: Component[]): void
  removeEntity(entityId: EntityId): void
  setComponent(entityId: EntityId, componentId: ComponentId, data: ComponentData): void
  removeComponent(entityId: EntityId, componentId: ComponentId): void
}

/**
 * An actor that contains the state of the scene, but it can also generate updates to it
 */
export abstract class StatefulActor implements StateContainer {
  abstract addEntity(entityId: string, components?: Component[]): void
  abstract removeEntity(entityId: string): void
  abstract setComponent(entityId: string, componentId: number, data: any): void
  abstract removeComponent(entityId: string, componentId: number): void
  abstract onAddEntity(listener: (entityId: EntityId, components?: Component[]) => void): void
  abstract onRemoveEntity(listener: (entityId: EntityId) => void): void
  abstract onSetComponent(listener: (entityId: EntityId, componentId: ComponentId, data: ComponentData) => void): void
  abstract onRemoveComponent(listener: (entityId: EntityId, componentId: ComponentId) => void): void

  /**
   * Take a @param container and update it when an change to the state occurs
   */
  forwardChangesTo(container: StateContainer) {
    this.onAddEntity((entityId, components) => container.addEntity(entityId, components))
    this.onRemoveEntity((entityId) => container.removeEntity(entityId))
    this.onSetComponent((entityId, componentId, data) => container.setComponent(entityId, componentId, data))
    this.onRemoveComponent((entityId, componentId) => container.removeComponent(entityId, componentId))
  }

}

